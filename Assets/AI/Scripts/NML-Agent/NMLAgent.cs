using UnityEngine;

//something fucks up when respawns but player doesnt move, then corrects itself.

public class NMLAgent : MonoBehaviour {

    public float health;
    int ammo;
    public Camera personalCamera;

    bool actionMode;

    //For pathfinding functionality
    public PathFinder pathFinder;
    public PathGrid pathGrid;


    float idleTimer;
    float escapeCollisionTimer;


    int pathPosition;
    int pathSize;
    //bool pathToTarget;
    bool canShoot;
    bool pathCompleted;
    [SerializeField]
    GameObject target;
    Vector3 targetPosition;
    [SerializeField]
    Vector3 idleTarget;

    public GameObject agentTrainer;

    public bool idlecontroller;

    public GameObject idlecube;

    public EnemyAgentWeaponManager weaponManager;

    public bool isAlive;

    public bool justRespawned;

	// Use this for initialization
	void Start () {

        justRespawned = false;
        actionMode = false;
        health = 100;
        ammo = 16;
        idleTimer = 1.0f;
        escapeCollisionTimer = 0.0f;
        idleTarget = new Vector3(0, 0, 0);
        pathPosition = 0;
        pathSize = 0;
        canShoot = false;
        pathCompleted = false;
	}


    void SearchArea()
    {
        //Check if player is within the agents camera, 
        //uncomment this for game code, change agenttrainer in this function to g.
        //foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player1"))
        //{
        //    if (g == this.gameObject)
        //        continue;
        if (agentTrainer.GetComponent<EnemyAgentController>().isAlive)
        {
            Vector3 screenPoint = personalCamera.WorldToViewportPoint(agentTrainer.transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            if (!idlecontroller)
                actionMode = onScreen;
            else
                actionMode = false;

            if (onScreen)
                target = agentTrainer;
        }
        else
        {
            actionMode = false;
        }
       // }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (escapeCollisionTimer <= 0.0f)
        {
            idleTimer = 1.0f;
            escapeCollisionTimer = 3.0f;
        }
        else
            escapeCollisionTimer -= Time.deltaTime;

    }

    void Idle()
    {
        if(idleTimer == 1.0f)
        {
            //Assign random target location
            idleTarget = new Vector3(gameObject.transform.position.x + Random.Range(-150, 150) , gameObject.transform.position.y + Random.Range(-150, 150), -10);

            //Keep looking for random target until it is a viable position
            while(pathGrid.GetNodeEmpty(idleTarget) == false)
            {
                idleTarget = new Vector3(gameObject.transform.position.x + Random.Range(-150, 150), gameObject.transform.position.y + Random.Range(-150, 150), -10);
            }
            Debug.Log("new path");
            //Find a path to the target location
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, idleTarget);
            pathCompleted = false;
            pathSize = pathGrid.path.Count;
            idleTimer -= Time.deltaTime;
            

        }
        else
        {

            if (pathCompleted)
            {
                pathSize = 0;
                idleTimer -= Time.deltaTime;

                if (idleTimer <= 0.0f)
                {
                    idleTimer = 1.0f;
                    return;
                }
            }

            //Debug.Log("path position : " + pathPosition + " , path size : " + pathSize);
            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos && pathPosition < pathSize - 1)
            {
                pathPosition++;             
            }

            if (pathPosition == pathSize - 2)
            {
                pathCompleted = true;
            }

            if(pathPosition < pathSize -2 && pathSize != 0)
            {
                //rotate towards this direction (determined by difficulty setting)
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //rotate towards the way the agent is facing
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);
               // Debug.Log("rotating and moving");
                //Prevent getting stuck on walls when idling
                //Record position then move
                Vector3 previousPosition = gameObject.transform.position;               
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);

                //Reset path if stuck (position hasn't changed
                if (gameObject.transform.position == previousPosition)
                {
                    idleTimer = 1.0f;
                    return;
                }
            }
        }
    }



    void Attack()
    {
        CheckCanShoot();

        if (targetPosition != target.transform.position || justRespawned)
        {
            //Find a path to the targets new location
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.position);
            pathSize = pathGrid.path.Count;
            justRespawned = false;
        }

        //If the AI is currently unable to get a shot
        if (!canShoot)
        {
            //start moving towards the player
            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos && pathPosition <= pathSize - 2)
                pathPosition++;

            //If still moving
            if (pathPosition < pathSize - 2)
            {
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //rotate towards the way the agent is facing
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //if the AI happens to be facing the right direction, shoot
                Vector3 shootAngle = (pathGrid.path[pathSize - 1].worldPos - transform.position);
                if (Vector3.Angle(gameObject.transform.up, shootAngle) < 2)
                {
                   // weaponManager.Shoot(1);
                }
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);
            }

        }
        else//If the agent can shoot from where they are
        if (canShoot)
        {
            //Check again that they can shoot 
            CheckCanShoot();

            //If they still can, shoot
            if (canShoot)
            {
                //weaponManager.Shoot(1);
            }
        }
    }

    void CheckCanShoot()
    {
        //Check agent is still alive
        if (!agentTrainer.GetComponent<EnemyAgentController>().isAlive)
        {
            actionMode = false;
            canShoot = false;
            return;
        }

        //Calculate direction to target player
        Vector3 directionToTarget = target.transform.position - gameObject.transform.position;

        //check if there is an obstruction between the enemy and the player
        RaycastHit hit;

        if (Physics.Linecast(transform.position, target.transform.position, out hit))
        {
            //If there is no obstruction
            if (hit.transform.tag == "Player1")
            {
                //Rotate towards them and shoot immediately
                //Calculate the direction to face
                Vector3 direction = target.transform.position - gameObject.transform.position;
                //Calculate the required rotation
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                //Slerp at 0.1 to mimic smooth rotation from mouse
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //If the agent is close enough, allow it to shoot
                if (Vector3.Distance(gameObject.transform.position, target.transform.position) < 55)
                    canShoot = true;
                else
                    canShoot = false;
            }
        }
        else 
            canShoot = false;
    }

    public void Respawn()
    {
        //Set the players position to a random space within the range offered by the academies parameters
        gameObject.transform.position = new Vector3(Random.Range(-target.GetComponent<CurriculumReinforcement>().resetParams["x-position"], target.GetComponent<CurriculumReinforcement>().resetParams["x-position"]),
            Random.Range(-target.GetComponent<CurriculumReinforcement>().resetParams["y-position"], target.GetComponent<CurriculumReinforcement>().resetParams["y-position"]), -10);
        
        //Reset controller variables
        health = target.GetComponent<CurriculumReinforcement>().resetParams["health"];
        weaponManager.currentWeapon = (int)target.GetComponent<CurriculumReinforcement>().resetParams["weapon"];

        for (int i = 0; i < weaponManager.clipSize.Length; i++)
        {
            weaponManager.currentAmmo[i] = weaponManager.clipSize[i];
        }

        Debug.Log("resetting");
        //Reset Agents direction and rotation
        //gameObject.transform.up = new Vector3(Random.Range(-target.GetComponent<CurriculumReinforcement>().resetParams["x-direction"], target.GetComponent<CurriculumReinforcement>().resetParams["x-direction"]), 
        //    Random.Range(-target.GetComponent<CurriculumReinforcement>().resetParams["y-direction"], target.GetComponent<CurriculumReinforcement>().resetParams["y-direction"]), 0);

        //float rotation = Mathf.Atan2(gameObject.transform.up.x, gameObject.transform.up.y) * Mathf.Rad2Deg;
        //gameObject.transform.rotation = Quaternion.Euler(0, 0, -rotation);

        canShoot = false;
        actionMode = false;
        isAlive = true;
        idleTimer = 1.0f;
        pathCompleted = true;
        pathPosition = 0;
        pathSize = 0;
        justRespawned = true;

        //Find a path to the targets new location
        pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.position);
        pathSize = pathGrid.path.Count;

    }

    // Update is called once per frame
    void Update () {

        //Check if still alive
        if (health <= 0)
            isAlive = false;

        if (isAlive)
        {
            if (idlecontroller)
                actionMode = false;

            if (!actionMode)
                SearchArea();

            if (!actionMode)
                Idle();
            else
                Attack();

            idlecube.transform.position = idleTarget;

            if (target)
                targetPosition = target.transform.position;

            if (actionMode)
            {
                Debug.Log("attacking");
                Debug.Log("can shoot : " + canShoot);
            }
            else
                Debug.Log("idling");

            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }else
        {
            Respawn();
        }
    }


}
