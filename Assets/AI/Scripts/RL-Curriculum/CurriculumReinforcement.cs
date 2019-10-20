using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class CurriculumReinforcement : Agent {

    //Training module
    public EnemyAgentController controller;

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

    public List<GameObject> visiblePlayers = new List<GameObject>();
    public Vector3 previousPosition;

    public Camera personalCamera;

    public GameObject[] walls;

    public GameObject worldPosition;

    public bool isLearning;

    RayPerception3D rayPerception;
    float rayDistance = 50.0f;
    float[] rayAngles = new float[19];
    string[] detectableObjects = { "Obstacle", "AdversaryPlayer" };

    //Modifiable agent values in the start menu
    public float killReward;
    public float deathPenalty;
    public float collisionPenalty;

    private void Start()
    {
        RLSessionManager sessManager = GameObject.Find("RLSessionManager").GetComponent<RLSessionManager>();

        //Setup training stats from the session manager.
        killReward = sessManager.floatModelSettings[0];
        deathPenalty = sessManager.floatModelSettings[1];
        collisionPenalty = sessManager.floatModelSettings[2];

        //Activate or deactivate graphics rendering
        if (sessManager.floatModelSettings[3] == 0)
        {
            personalCamera.gameObject.SetActive(false);

            if(GameObject.Find("Main Camera"))
            GameObject.Find("Main Camera").gameObject.SetActive(false);
        }
        else
        {
            personalCamera.gameObject.SetActive(true);

            if (GameObject.Find("Main Camera"))
                GameObject.Find("Main Camera").gameObject.SetActive(true);
        }
    
        //debug values
        //killReward = 1.0f;
        //deathPenalty = -1.0f;
        //collisionPenalty = -0.1f;

        previousPosition = gameObject.transform.position;
        rayPerception = GetComponent<RayPerception3D>();

        for (int i = 1; i < 19; i++)
        {
            rayAngles[i] = i * 20;
        }

        if (!isLearning)
            detectableObjects[1] = "Player1";
    }

    //AI specific functionality
    public override void CollectObservations()
    {
        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, 0.0f, 0.0f));
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
            AddReward(deathPenalty);
            Done();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            AddReward(collisionPenalty);
            Done();
        }
    }

    public void GainedKill()
    {
        Debug.Log("gained kill");
        AddReward(killReward);
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

        //Reset the agents training this agent if using CL
        if (trainingAgents[0] != null)
        {
            if (trainingAgents.Length > 0 && trainingAgents[0].GetComponent<NMLAgent>())
            {
                for (int i = 0; i < trainingAgents.Length; i++)
                {
                    trainingAgents[i].GetComponent<NMLAgent>().Respawn();
                }
            }
        }
    }
}
