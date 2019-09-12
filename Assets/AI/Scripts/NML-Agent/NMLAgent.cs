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
    bool pathToTarget;

    [SerializeField]
    GameObject target;

    Vector3 idleTarget;

    public EnemyAgentWeaponManager weaponManager;

	// Use this for initialization
	void Start () {
        actionMode = false;
        health = 100;
        ammo = 16;
        idleTimer = 8.0f;
        escapeCollisionTimer = 0.0f;
        idleTarget = new Vector3(0, 0, 0);
        pathPosition = 0;
        pathSize = 0;
	}


    void SearchArea()
    {
        //Check if player is within the agents camera
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player1"))
        {
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
            idleTimer = 8.0f;
            escapeCollisionTimer = 3.0f;
        }
        else
            escapeCollisionTimer -= Time.deltaTime;
    }

    void Idle()
    {
        if(idleTimer == 8.0f)
        {
            //Find random target location
            idleTarget = new Vector3(gameObject.transform.position.x + Random.Range(-150, 150) , gameObject.transform.position.y + Random.Range(-150, 150), -10);

            //Find a path to the target location
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, idleTarget);
            pathSize = pathGrid.path.Count;
            idleTimer -= Time.deltaTime;

        }
        else
        {

            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos)
                pathPosition++;

            if (pathPosition >= pathSize)
            {
                idleTimer = 8.0f;
                pathPosition = 0;
            }
            else
            {
                //rotate towards this direction (determined by difficulty setting)
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //Find the angle between the players current direction and its target
                float angle = Vector3.Angle(gameObject.transform.up, direction);

                //snap to correct rotation 
                if(angle > 2)
                transform.Rotate(new Vector3(0, 0, angle));

                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);
                idleTimer -= Time.deltaTime;

            }
        }
    }



    void Attack()
    {

        if (!pathToTarget)
        {
            //Find a path to the target location
            pathPosition = 0;
            pathFinder.FindPath(pathGrid, gameObject.transform.position, target.transform.position);
            pathSize = pathGrid.path.Count;
            pathToTarget = true;
        }
        else
        {
            //depending on how close to the right direction, start moving
            if (gameObject.transform.position == pathGrid.path[pathPosition].worldPos)
                pathPosition++;

            if (pathPosition >= pathSize)
            {
                idleTimer = 8.0f;
                pathPosition = 0;
            }
            else
            {
                //rotate towards this direction (determined by difficulty setting)
                //find the vector pointing from our position to the target
                Vector3 direction = (pathGrid.path[pathPosition].worldPos - transform.position);

                //Find the angle between the players current direction and its target
                float angle = Vector3.Angle(gameObject.transform.up, direction);

                //snap to correct rotation 
                if (angle > 2)
                    transform.Rotate(new Vector3(0, 0, angle));

                //if facing the right direction, shoot
                Vector3 shootAngle = (pathGrid.path[pathSize - 1].worldPos - transform.position);
                if (Vector3.Angle(gameObject.transform.up, shootAngle) < 2)
                {
                    weaponManager.Shoot(1);
                }

                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, pathGrid.path[pathPosition].worldPos, 0.3f);
            }
        }
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
	}

}
