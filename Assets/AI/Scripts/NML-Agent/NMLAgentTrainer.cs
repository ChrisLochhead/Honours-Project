using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NMLAgentTrainer : MonoBehaviour {

    //Stats
    public float health;
    int ammo;

    //Camera
    public Camera personalCamera;

    //Dictates the agents action, if it is in action mode, it will actively attack
    //otherwise, it will wander around and look for enemies
    bool actionMode;

    //regulates if the player can shoot (with reference to if it can see a player in its sights)
    bool canShoot;

    //Reference to the AI this agent is fighting (for training purposes)
    public GameObject agentTrainer;

    //Reference to the agents weapons
    public EnemyAgentWeaponManager weaponManager;

    //records whether the agent is alive
    public bool isAlive;

    public GameObject worldPosition;

    Vector3 wanderPositon;

    void Start()
    {

        //Initialise all booleans not set
        //in inspector to false as default
        actionMode = false;
        canShoot = false;

        //Initialise stats
        health = 100;
        ammo = 16;

    }


    void SearchArea()
    {

        if (agentTrainer.GetComponent<EnemyAgentController>().isAlive)
        {
            //Discern if the agents enemy is within the agents field of view
            Vector3 screenPoint = personalCamera.WorldToViewportPoint(agentTrainer.transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            actionMode = onScreen;
        }
        else
        {
            actionMode = false;
        }
    }

    //Sequence of actions to take if the enemy can see an agent
    void Attack()
    {
        //Check if the agent is still on the enemies screen or if they are facing 
        //the right direction to shoot them
        CheckCanShoot();
       
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
        if (!agentTrainer.GetComponent<EnemyAgentController>().isAlive)
        {
            actionMode = false;
            canShoot = false;
            return;
        }

        //check if there is an obstruction between the enemy and the player
        RaycastHit hit;

        if (Physics.Linecast(transform.position, agentTrainer.transform.position, out hit))
        {
            //If there is no obstruction
            if (hit.transform.tag == "Player1")
            {
                //Rotate towards them and shoot immediately
                //Calculate the direction to face
                Vector3 direction = agentTrainer.transform.position - gameObject.transform.position;
                //Calculate the required rotation
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                //Slerp at 0.1 to mimic smooth rotation from mouse
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

                //If the agent is close enough, allow it to shoot
                if (Vector3.Distance(gameObject.transform.position, agentTrainer.transform.position) < 55)
                    canShoot = true;
                else
                    canShoot = false;
            }
        }
        else
            canShoot = false;
    }

    public void Idle()
    {
        if (wanderPositon == null || wanderPositon == gameObject.transform.position)
        {
            //find a new wander position
            wanderPositon = new Vector3(Random.Range(-agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["x-position"], agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["x-position"]) + worldPosition.transform.position.x,
            Random.Range(-agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["y-position"], agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["y-position"]) + worldPosition.transform.position.y, -10);
        }

        //Performs lightweight pathfinding suitable for training purposes without grid
        //yet appoximate enough to simulate movement
        if(wanderPositon != gameObject.transform.position)
        {
            Vector3 direction = (wanderPositon - transform.position);

            //rotate towards the way the agent is facing
            float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, Quaternion.Euler(0, 0, -rotation), 0.1f);

            //Prevent getting stuck on walls when idling
            //move         
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, wanderPositon, 0.3f);

        }
    }

    public void Respawn()
    {
        //Set the players position to a random space within the range offered by the academies parameters
        gameObject.transform.position = new Vector3(Random.Range(-agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["x-position"], agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["x-position"]) + worldPosition.transform.position.x,
            Random.Range(-agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["y-position"], agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["y-position"]) + worldPosition.transform.position.y, -10);

        //Reset controller variables
        health = agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["health"];
        weaponManager.currentWeapon = (int)agentTrainer.GetComponent<CurriculumReinforcement>().resetParams["weapon"];

        //Reset the ammo in all weapons
        for (int i = 0; i < weaponManager.clipSize.Length; i++)
        {
            weaponManager.currentAmmo[i] = weaponManager.clipSize[i];
        }

        //Reset action dependent variables
        canShoot = false;
        actionMode = false;
        isAlive = true;

    }

    void Update()
    {

        //Check if still alive
        if (health <= 0)
            isAlive = false;

        //if the agent is alive, go into the state machine
        if (isAlive)
        {
            //if the player didn't see an enemy last frame,
            //search again
            if (!actionMode)
            {
                SearchArea();
               
            }

            //If it still cant find an enemy, wander around
            //otherwise, go into action mode
            if (actionMode)
            {
                Attack();
            }
            else
            {
                Idle();
            }

            //Zero off the physics to negate any collision forces while
            //retaining collision detection
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        else
        {
            //If the agent is dead, call the respawn routine
            Respawn();
        }
    }

}
