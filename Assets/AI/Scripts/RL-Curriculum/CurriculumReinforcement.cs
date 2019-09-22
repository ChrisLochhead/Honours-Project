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

    public List<GameObject> visiblePlayers = new List<GameObject>();
    public Vector3 previousPosition;

    public Camera personalCamera;

    public GameObject[] walls;

    public GameObject worldPosition;

    RayPerception3D rayPerception;
    float rayDistance = 50.0f;
    float[] rayAngles = { 20, 40, 60, 80, 100, 120, 140, 160, 180, 200, 220, 240, 260, 280, 300, 320, 340, 360 };
    string[] detectableObjects = { "Obstacle", "Player1" };
    List<float> debugRays = new List<float>();

    private void Start()
    {
        previousPosition = gameObject.transform.position;
        rayPerception = GetComponent<RayPerception3D>();

    }

    private void OnDrawGizmos()
    {
        //foreach (float f in debugRays) {
        //    Vector3 dir = 
        //    Debug.DrawRay(gameObject.transform.position, );
        //}
    }

    //AI specific functionality
    public override void CollectObservations()
    {
        List<float> tmp = rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0.0f, 0.0f);
        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0.0f, 0.0f));
        //Generate a list of visible players complete with attributes
        //GeneratePlayerInfo();
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
            //    AddReward(-0.001f);

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
            AddReward(-1f);
            Done();
        }

        ////Penalise for loosing health
        //if (controller.health < 100)
        //{
        //    float difference = 100 - health;
        //    difference = difference * -0.000001f;
        //    SetReward(difference);
        //}

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            //ebug.Log("colliding");
            AddReward(-0.1f);
            Done();
        }
    }

    public void GainedKill()
    {
        Debug.Log("gained kill");
        AddReward(1f);
        //Done();
    }

    //public void GeneratePlayerInfo()
    //{
    //    //Add the players x and y co-ordinates (z never changes so it wont be necessary)
        
    //    AddVectorObs(Mathf.Clamp01(gameObject.transform.position.x/60.0f));
    //    AddVectorObs(Mathf.Clamp01(gameObject.transform.position.y/50.0f));

    //    //Add the players up direction in x and y (Z never changes for this vector either)
    //    AddVectorObs(Mathf.Clamp01(gameObject.transform.up.x));
    //    AddVectorObs(Mathf.Clamp01(gameObject.transform.up.y));

    //    //Clear the previous frames list of visible players
    //    visiblePlayers.Clear();

    //    //Get list of players
    //    GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player1");

    //    //Check if the player is visible to the AI
    //    foreach (GameObject g1 in allPlayers)
    //    {
    //        //If looking at itself, ignore it
    //        if (g1.transform.position == gameObject.transform.position)
    //            continue;

    //        //Check if enemy is visible within the agents camera
    //        Vector3 screenPoint = personalCamera.WorldToViewportPoint(g1.transform.position);
    //        bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

    //        //If so, calculate the angle between them and add it to the vector of observations
    //        if (onScreen)
    //        {                
    //            visiblePlayers.Add(g1);
    //            Vector3 directionToAim = gameObject.transform.position - g1.transform.position;
    //            float angleBetweenAgents = Vector3.Angle(gameObject.transform.up, -directionToAim);

    //            AddVectorObs(angleBetweenAgents/360.0f);

    //            //if (Vector3.Angle(gameObject.transform.up, -directionToAim) < 2)
    //            //{
    //            //    //AddReward(0.01f);
    //            //    AddVectorObs(1);
    //            //}
    //            //else
    //            //    AddVectorObs(0);

    //            break;
    //        }

    //    }

    //    if(visiblePlayers.Count > 0)
    //    {
    //        //Indicate that a player is visible
    //        //AddVectorObs(1);

    //        //Add the enemies position in x and y
    //        AddVectorObs(visiblePlayers[0].transform.position.x/60.0f);
    //        AddVectorObs(visiblePlayers[0].transform.position.y/50.0f);

    //        //Check if the enemy is looking at the agent
    //        Vector3 directionEnemyAim = visiblePlayers[0].transform.position - gameObject.transform.position;
    //        float angleBetweenAgents = Vector3.Angle(gameObject.transform.up, -directionEnemyAim);

    //        //if (Vector3.Angle(gameObject.transform.up, -directionEnemyAim) < 2)
    //        //{
    //        //    AddReward(-0.0001f);
    //        //    AddVectorObs(1);
    //        //}
    //        //else
    //        //    AddVectorObs(0);



    //    }
    //    else
    //    {
    //        //Otherwise, add -1 to indicate no angle between agents
    //        //and 0 to indicate that it cant see any agents
    //        //then three more -1's to indicate it cannot add this information
    //        //AddVectorObs(-1);
    //        //AddVectorObs(-1);
    //        //AddVectorObs(-1);
    //        AddVectorObs(-1);
    //        AddVectorObs(-1);
    //        AddVectorObs(-1);
    //    }

    //   // int wallCounter = 0;

    //    ////Finally, add walls to the vector of observations
    //    //foreach(GameObject w in walls)
    //    //{
    //    //    //Min X
    //    //    if (wallCounter == 0)
    //    //        AddVectorObs(Mathf.Clamp01(w.transform.position.x));
    //    //    //Min Y
    //    //    if (wallCounter == 1)
    //    //        AddVectorObs(Mathf.Clamp01(w.transform.position.y));
    //    //    //Max X
    //    //    if (wallCounter == 2)
    //    //        AddVectorObs(Mathf.Clamp01(w.transform.position.x));
    //    //    //Max Y
    //    //    if (wallCounter == 3)
    //    //        AddVectorObs(Mathf.Clamp01(w.transform.position.y));

    //    //    wallCounter++;
    //    //}
    //}

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
        gameObject.transform.position = new Vector3(Random.Range(-resetParams["x-position"], resetParams["x-position"]) + worldPosition.transform.position.x, Random.Range(-resetParams["y-position"], resetParams["y-position"]) + worldPosition.transform.position.y, -10);

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
