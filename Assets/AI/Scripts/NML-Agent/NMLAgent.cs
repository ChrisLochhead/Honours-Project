using UnityEngine;

//something fucks up when respawns but player doesnt move, then corrects itself.

public class NMLAgent : MonoBehaviour {

    //Stats
    public float health;

    //Camera
    public Camera personalCamera;

    //Dictates the agents action, if it is in action mode, it will actively attack
    //otherwise, it will wander around and look for enemies
    bool actionMode;

    [SerializeField]
    bool isStudy = false;
    //For pathfinding functionality
    //finds the path
    public PathFinder pathFinder;

    //represents the grid
    public PathGrid pathGrid;

    //For regulating position to make sure the agent doesnt get stuck when idling
    Vector3 previousPosition;

    //timer to run down between idle paths to follow
    float idleTimer;

    //represents path position and the total path size
    int pathPosition;
    int pathSize;
    bool pathCompleted;

    //regulates if the player can shoot (with reference to if it can see a player in its sights)
    bool canShoot;

    //Represents physical target
    [SerializeField]
    GameObject target;
    Vector3 targetPosition;

    //Represents random pathway ending when idling
    [SerializeField]
    Vector3 idleTarget;

    //Reference to the AI this agent is fighting (for training purposes)
    public GameObject agentTrainer;

    //Debug functionality to test AI functionality
    //allows manual control of which state the agent is in
    public bool idlecontroller;

    //Reference to the agents weapons
    public EnemyAgentWeaponManager weaponManager;

    //records whether the agent is alive
    public bool isAlive;

    //records if a new path has been found since
    //the agents last respawn
    public bool justRespawned;

    public Vector2 spawnCentre;
    GameObject[] spawnPoints;

	void Start () {

        //Check if in a study session
        if (GameObject.Find("Player Prefab test (2)(Clone)").GetComponent<Client>().isStudy)
        {
            isStudy = true;
            spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint1");
            agentTrainer = GameObject.Find("Player Prefab test (2)(Clone)");
            target = agentTrainer;
            pathGrid = GameObject.Find("PathGrid").GetComponent<PathGrid>();
        }else
        {
            Destroy(GameObject.Find("PathGrid").GetComponent<PathGrid>());
        }

        //Initialise all booleans not set
        //in inspector to false as default
        justRespawned = false;
        actionMode = false;
        canShoot = false;
        pathCompleted = false;
        
        //Initialise stats
        health = 100;

        //Initialise pathing variables
        idleTimer = 1.0f;
        idleTarget = new Vector3(0, 0, 0);
        pathPosition = 0;
        pathSize = 0;

	}


    void SearchArea()
    {
        //Check if player is within the agents camera, 
        //uncomment this for game code, change agenttrainer in this function to g.
        bool enemyAlive;

        if (isStudy)
        {
            enemyAlive = agentTrainer.GetComponent<Client>().isDead;
            if (enemyAlive)
                enemyAlive = false;
            else
                enemyAlive = true;
        }
        else
        {
            enemyAlive = agentTrainer.GetComponent<EnemyAgentController>().isAlive;
        }

        if (enemyAlive)
        {
            //Discern if the agents enemy is within the agents field of view
            Vector3 screenPoint = personalCamera.WorldToViewportPoint(agentTrainer.transform.Find("Player1").position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            
            //Assign the correct action status accordingly
            if (!idlecontroller)
                actionMode = onScreen;
            else
                actionMode = false;

            //Assign the target (training only)
            if (onScreen)
                target = agentTrainer;
        }
        else
        {
            actionMode = false;
        }
    }

    void Idle()
    {
        if(idleTimer == 1.0f)
        {
            pathSize = 0;
            //Assign random target location
            while (pathSize < 3)
            {
                idleTarget = new Vector3(gameObject.transform.position.x + Random.Range(-150, 150), gameObject.transform.position.y + Random.Range(-150, 150), -10);

                //Keep looking for random target until it is a viable position
                while (pathGrid.GetNodeEmpty(idleTarget) == false)
                {
                    idleTarget = new Vector3(gameObject.transform.position.x + Random.Range(-150, 150), gameObject.transform.position.y + Random.Range(-150, 150), -10);
                }

                //Find a path to the target location
                pathPosition = 0;
                pathFinder.FindPath(pathGrid, gameObject.transform.position, idleTarget);
                pathCompleted = false;
                pathSize = pathGrid.path.Count;
            }
            idleTimer -= Time.deltaTime;
        }
        else
        {
            //if the current path has came to an end
            if (pathCompleted)
            {
                //Reset the pathing variables
                pathSize = 0;
                Debug.Log("pathdone");
                idleTimer -= Time.deltaTime;

                //After idling is over, reset the timer to trigger a new path to be found
                if (idleTimer <= 0.0f)
                {
                    idleTimer = 1.0f;
                    return;
                }
            }


            //Once the path is completed, set it to complete
            if (pathPosition == pathSize - 2)
            {               
                pathCompleted = true;
                return;
            }

            //iterate the path position once it has reached the next node in the path
            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos && pathPosition < pathSize - 1)
            {
                pathPosition++;             
            }

            //While the path hasn't been completed
            if(pathPosition < pathSize -2 && pathSize != 0)
            {
                //rotate towards this direction (determined by difficulty setting)
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //rotate towards the way the agent is facing
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //Prevent getting stuck on walls when idling
                //move         
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);

                //Reset path if stuck (position hasn't changed
                if (gameObject.transform.position == previousPosition)
                {
                    idleTimer = 1.0f;
                    return;
                }

                //record position for the next frame
                previousPosition = gameObject.transform.position;
            }
        }
    }


    //Sequence of actions to take if the enemy can see an agent
    void Attack()
    {
        //Check if the agent is still on the enemies screen or if they are facing 
        //the right direction to shoot them
        CheckCanShoot();

        //If the player hasn't moved since the last frame
        //or the agent has died since the last path was found
        //Debug.Log(gameObject.transform.Find("Player1").position);// + " " + target.transform.Find("Player1").position,);
        if (targetPosition != target.transform.Find("Player1").position || justRespawned)
        {
            //Find a path to the targets new location            
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.Find("Player1").position);
            pathSize = pathGrid.path.Count;
            justRespawned = false;
        }

        //If the AI is currently unable to get a shot
        if (!canShoot)
        {
           // Debug.Log("calling cant shoot" + " pathpos : " + pathPosition + " : pathsize : " + pathSize + " pos " + gameObject.transform.Find("Player1").position + " target : " + target.transform.Find("Player1").position);
            //start moving towards the player
            if (pathPosition <= pathSize - 2)
            {
                if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos)
                    pathPosition++;
            }

            //If still moving
            if (pathPosition < pathSize - 2 && pathSize >= 2)
            {
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //rotate towards the way the agent is facing
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //Move
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);
                Debug.Log("moving");
                //if the AI happens to be facing the right direction, shoot
                Vector3 shootAngle = (pathGrid.path[pathSize - 2].worldPos - transform.position);
                if (Vector3.Angle(gameObject.transform.up, shootAngle) < 2)
                {
                    weaponManager.Shoot(1);
                }
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
                weaponManager.Shoot(1);
            }
        }
    }

    void CheckCanShoot()
    {
        //Check agent is still alive
        bool StudyShoot;
        if(!isStudy)
        {
            StudyShoot = !agentTrainer.GetComponent<EnemyAgentController>().isAlive;
        }else
        {
            StudyShoot = agentTrainer.GetComponent<Client>().isDead;
        }

        if (StudyShoot)
        {
            actionMode = false;
            canShoot = false;
            return;
        }

        //check if there is an obstruction between the enemy and the player
        RaycastHit hit;

        Vector3 targetPos;
        if (isStudy)
            targetPos = target.transform.Find("Player1").transform.position;
        else
            targetPos = target.transform.position;

        if (Physics.Linecast(transform.position, targetPos, out hit))
        {
            //If there is no obstruction
            if (hit.transform.tag == "Player1")
            {
                //Rotate towards them and shoot immediately
                //Calculate the direction to face
                Vector3 direction = targetPos - gameObject.transform.position;
                //Calculate the required rotation
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                //Slerp at 0.1 to mimic smooth rotation from mouse
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //If the agent is close enough, allow it to shoot
                if (Vector3.Distance(gameObject.transform.position, targetPos) < 55)
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
        if (isStudy)
        {
            int SpawnIndex = Random.Range(0, spawnPoints.Length);
            gameObject.transform.position = new Vector3(spawnPoints[SpawnIndex].transform.position.x, spawnPoints[SpawnIndex].transform.position.y, -10);
            health = 100;
            weaponManager.currentWeapon = 0;
        }
        else
        {
            //Set the players position to a random space within the range offered by the academies parameters
            gameObject.transform.position = new Vector3(Random.Range(-target.GetComponent<AIController>().resetParams["x-position"], target.GetComponent<AIController>().resetParams["x-position"]) + spawnCentre.x,
                Random.Range(-target.GetComponent<AIController>().resetParams["y-position"], target.GetComponent<AIController>().resetParams["y-position"]) + spawnCentre.y, -10);
            //Reset controller variables
            health = target.GetComponent<AIController>().resetParams["health"];
            weaponManager.currentWeapon = (int)target.GetComponent<AIController>().resetParams["weapon"];

        }

        //Reset the ammo in all weapons
        for (int i = 0; i < weaponManager.clipSize.Length; i++)
        {
            weaponManager.currentAmmo[i] = weaponManager.clipSize[i];
        }

        //Reset action dependent variables
        canShoot = false;
        actionMode = false;
        isAlive = true;
        idleTimer = 1.0f;

        //Reset pathing variables
        pathCompleted = true;
        pathPosition = 0;
        pathSize = 0;
        justRespawned = true;

        //Find a path to the targets new location
        pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.position);
        pathSize = pathGrid.path.Count;

    }

    void Update () {

        //Check if still alive
        if (health <= 0)
            isAlive = false;

        //if the agent is alive, go into the state machine
        if (isAlive)
        {
            //For debugging of each mode in the editor
            if (idlecontroller)
                actionMode = false;

            //if the player didn't see an enemy last frame,
            //search again
            if (!actionMode)
                SearchArea();

            //If it still cant find an enemy, wander around
            //otherwise, go into action mode
            if (!actionMode)
                Idle();
            else
                Attack();

            //assign the target (training only)
            if (target)
                targetPosition = target.transform.Find("Player1").position;

            //Zero off the physics to negate any collision forces while
            //retaining collision detection
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }else
        {
            //If the agent is dead, call the respawn routine
            Respawn();
        }
    }


}
