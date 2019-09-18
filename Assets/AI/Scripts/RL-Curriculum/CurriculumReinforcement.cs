using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class CurriculumReinforcement : Agent {

    //Training module

    /*
    Camera is 78 wide by 22 X*Y
    */
    public EnemyAgentController controller;
    Vector2 cameraDimensions = new Vector2(39, 11);

    //AI health
    private float health;

    //AI players rank
    private float rank;

    //Direction the AI is facing
    private Vector3 direction;

    //Closest enemy to the AI agent
    private GameObject closestPlayer;

    //Weapon manager
    public EnemyAgentWeaponManager weaponManager;

    //Academy variables
    public ResetParameters resetParams;

    public GameObject [] trainingAgents;

    [SerializeField]
    bool checkCanShoot = false;

    public Vector3 previousPosition;

    public Camera personalCamera;

    public Vector2 spawnCentre;

    public bool enemyVisible = false;

    private void Start()
    {
        previousPosition = gameObject.transform.position;
    }

    //AI specific functionality
    public override void CollectObservations()
    {
        //Generate a list of visible players complete with attributes
        GeneratePlayerInfo();
        GenerateAIInfo();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log(controller.isAlive);
        //Debug.Log(vectorAction[0] + " :: "+  vectorAction[1] + " :: " + vectorAction[2] + " :: " + vectorAction[3] + " :: " + vectorAction[4]);
        /*Generates 5 actions in 6 actions
         * Move, (can only move in the direction it is facing, so no extra variables for moving left/right/backwards)
         * Rotate
         * Shoot (only available if canshoot is true, but can still be attempted)
         * Reload
         * Change Weapon (like shoot, can be called even if it physically cant change weapon)
         */
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.discrete)
        {
            //Move agent using first two actions as movement and rotation amounts
            controller.Move(new Vector2(Mathf.RoundToInt(Mathf.Clamp01(vectorAction[0])), vectorAction[1]));

            //Decides whether agent should shoot (clamped to 0 or 1 for dont shoot and shoot)
            controller.Shoot(vectorAction[2]);

            //if (vectorAction[2] > 0)
            //    AddReward(-0.01f);

            //Decides whether agent should reload (clamped to 0 or 1 for dont reload and reload)
            controller.Reload(vectorAction[3]);

            //Decides to change weapon (0 for dont, 1 - 5 for which weapon to attempt to change to)
            //controller.ChangeWeapon(vectorAction[4]);
        }

        //Set previous position to the current
        previousPosition = gameObject.transform.position;


        //Trigger if the controller has died
        if (!controller.isAlive)
        {
            SetReward(-0.5f);
            //Done();
        }

        //Penalise for loosing health
        if(controller.health < 100)
        {
            float difference = 100 - health;
            difference = difference * -0.000001f;
            SetReward(difference);
        }

        //existential penalty
        SetReward(-0.000001f);

        //penalty for hitting a wall
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "obstacle")
        {
            AddReward(-0.001f);
            Debug.Log("still colliding");
        }
    }

    public void GainedKill()
    {
        Debug.Log("gained kill");
        SetReward(30f);
        Done();
    }

    public bool FoundEnemy()
    {
        ////If any of these return true, the AI can try to shoot
        //foreach (GameObject g3 in visiblePlayers)
        //{
        //    Vector3 aimDirection = g3.transform.position - gameObject.transform.position;
        //    if (Vector3.Angle(aimDirection, controller.transform.up) < 2)
        //    {
        //        return true;
        //    }
        //}
        return false;
    }

    public void GenerateAIInfo()
    {
        //Players weapon info
        //AddVectorObs(weaponManager.currentAmmo[weaponManager.currentWeapon]);

        checkCanShoot = FoundEnemy();

        if (checkCanShoot)
            AddVectorObs(1);
        else
            AddVectorObs(0);      
    }

    public void GeneratePlayerInfo()
    {
        Debug.Log(transform.up);
        //Add the players x and y co-ordinates (z never changes so it wont be necessary)
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.y);

        enemyVisible = false;

        //Get list of players
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player1");

        //Check if the player is visible to the AI
        foreach (GameObject g1 in allPlayers)
        {
            if (g1.transform.position == gameObject.transform.position)
            {
                continue;
            }

            //Check if enemy is within the agents camera
            Vector3 screenPoint = personalCamera.WorldToViewportPoint(g1.transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            if (onScreen)
            {
                Vector3 directionToAim = gameObject.transform.position - g1.transform.position;
               // Vector3 angleBetweenAgents = Vector3.Angle(gameObject.transform.up, -directionToAim);

                if (Vector3.Angle(gameObject.transform.up, -directionToAim) < 2)
                {
                    AddReward(0.0001f);
                    enemyVisible = true;
                }

            }

        }

        if (enemyVisible)
            AddVectorObs(1);
        else
            AddVectorObs(0);


    }

    public override void InitializeAgent()
    {
        Academy academy = Object.FindObjectOfType<Academy>();
        resetParams = academy.resetParameters;
    }

    public override void AgentReset()
    {
        //Reset the parameters when the Agent is reset.
        ResetAgentModel();
        controller.isAlive = true;
       
    }

    private void ResetAgentModel()
    {
        //Set the players position to a random space within the range offered by the academies parameters
        gameObject.transform.position = new Vector3(Random.Range(-resetParams["x-position"], resetParams["x-position"]) + spawnCentre.x, Random.Range(-resetParams["y-position"], resetParams["y-position"]) + spawnCentre.y, -10);

        //Reset controller variables
        controller.health = resetParams["health"];
        weaponManager.currentWeapon = (int)resetParams["weapon"];

        for (int i = 0; i < weaponManager.clipSize.Length; i++)
        {
            weaponManager.currentAmmo[i] = weaponManager.clipSize[i];
        }

        controller.rank = (int)resetParams["rank"];

        //Reset Agents direction and rotation
        controller.transform.up = new Vector3(Random.Range(-resetParams["x-direction"], resetParams["x-direction"]), Random.Range(-resetParams["y-direction"], resetParams["y-direction"]), 0);
        float rotation = Mathf.Atan2(controller.transform.up.x, controller.transform.up.y) * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, -rotation);

        controller.kills = 0;
        controller.hittingWall = false;

        //Reset the agents training this agent
        if(trainingAgents.Length > 0 && trainingAgents[0].GetComponent<NMLAgent>())
        {
            for(int i = 0; i < trainingAgents.Length; i++)
            {
                trainingAgents[i].GetComponent<NMLAgent>().Respawn();
            }
        }
    }
}
