using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NMLAgent : MonoBehaviour {

    int health;
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

    public GameObject idlecube;

    public EnemyAgentWeaponManager weaponManager;

	// Use this for initialization
	void Start () {
        actionMode = false;
        health = 100;
        ammo = 16;
        idleTimer = 1.0f;
        escapeCollisionTimer = 0.0f;
        idleTarget = new Vector3(0, 0, 0);
        Debug.Log("start");
        pathPosition = 0;
        pathSize = 0;
        canShoot = false;
        pathCompleted = false;
	}


    void SearchArea()
    {
        //Check if player is within the agents camera
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player1"))
        {
            if (g == this.gameObject)
                continue;

            Vector3 screenPoint = personalCamera.WorldToViewportPoint(g.transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            actionMode = onScreen;

            if (onScreen)
            {
                target = g;
                Debug.Log("found target");
            }

        }
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

                //Find the angle between the players current direction and its target
                float angle = Vector3.Angle(gameObject.transform.up, direction);

                //snap to correct rotation within error margin of 2 degrees 
                //if (angle > 2)
                // transform.Rotate(new Vector3(0, 0,angle));
                Debug.Log("in idle");
                //Prevent getting stuck on walls when idling
                Vector3 previousPosition = gameObject.transform.position;

                
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);

                //Reset path if stuck
                if (gameObject.transform.position == previousPosition)
                {
                    Debug.Log("is stuck");
                    idleTimer = 1.0f;
                    return;
                }
            }
        }
    }



    void Attack()
    {
        CheckCanShoot();

        if (targetPosition != target.transform.position)
        {
            //Find a path to the targets new location
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.position);
            pathSize = pathGrid.path.Count;
        }

        //If the AI is currently unable to get a shot
        if (!canShoot)
        {
            //start moving towards the player
            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos && pathPosition <= pathSize - 2)
                pathPosition++;

            Debug.Log("getting into cant shoot");

            //If still moving
            if (pathPosition < pathSize - 2)
            {
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //Find the angle between the players current direction and its target
                float angle = Vector3.Angle(gameObject.transform.up, direction);

                //snap to correct rotation within boundary
                 if (angle > 2)
                     transform.Rotate(new Vector3(0, 0, angle));
                //transform.LookAt(target.transform, -transform.up);
                //if the AI happens to be facing the right direction, shoot

                Vector3 shootAngle = (pathGrid.path[pathSize - 1].worldPos - transform.position);
                if (Vector3.Angle(gameObject.transform.up, shootAngle) < 2)
                {
                   // weaponManager.Shoot(1);
                }
                Debug.Log("attempting movement");
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);
            }

        }
        else//If the agent can shoot from where they are
        if (canShoot)
        {
            Debug.Log("getting into can shoot");
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
        //Calculate direction to target player
        Vector3 directionToTarget = target.transform.position - gameObject.transform.position;

        //check if there is an obstruction between the enemy and the player
        RaycastHit hit;

        if (Physics.Linecast(transform.position, target.transform.position, out hit))
        {
            //If there is no obstruction
            if (hit.transform.tag == "Player1")
            {
                Debug.Log("getting into linecast");
                //Rotate towards them and shoot immediately
                float angle = Vector3.Angle(gameObject.transform.up, directionToTarget);

                Debug.Log(angle);
                if (angle > 2)
                {
                    transform.Rotate(new Vector3(0, 0, angle));
                    Debug.Log("still rotating");
                }

                if (Vector3.Distance(gameObject.transform.position, target.transform.position) < 15)
                    canShoot = true;
                else
                    canShoot = false;
            }
        }
        else 
            canShoot = false;
    }
    void ResetAgent()
    {
        //Reset stats
        health = 100;
        ammo = 16;

        //Reset position
        transform.position = new Vector3(Random.Range(-250, 250), Random.Range(-250, 250), -10);

        //Set back to idle
        actionMode = false;

    }

    // Update is called once per frame
    void Update () {
        if (!actionMode)
            SearchArea();

        if (!actionMode)
            Idle();
        else
            Attack();

        idlecube.transform.position = idleTarget;

        if(target)
        targetPosition = target.transform.position;

        //if (actionMode)
        //    Debug.Log("attacking");
        //else
        //    Debug.Log("idling");

        GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }


}
