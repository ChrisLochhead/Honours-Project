﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class EnemyAgentReinforcement : Agent {

    //Training module

    /*
    Camera is 78 wide by 22 X*Y
    */
    public EnemyAgentController controller;
    Vector2 cameraDimensions = new Vector2(39, 11);

    //array of visible players
    public  List<GameObject> visiblePlayers = new List<GameObject>();

    //AI health
    private float health;

    //AI players rank
    private float rank;

    //Direction the AI is facing
    private Vector3 direction;

    //Marked true if the AI would hit someone if they shot
    private bool lineOfSight = false;

    //Marked true if shooting a wall behind which there would be a player
    private bool weakLineOfSight = false;

    //Closest enemy to the AI agent
    private GameObject closestPlayer;

    //Weapon manager
    public EnemyAgentWeaponManager weaponManager;

    //Academy variables
    ResetParameters resetParams;
 
    //AI specific functionality
    public override void CollectObservations()
    {
        //Generate a list of visible players complete with attributes
        GeneratePlayerInfo();
        GenerateAIInfo();
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        /*Generates 5 actions in 6 actions
         * Move, (can only move in the direction it is facing, so no extra variables for moving left/right/backwards)
         * Rotate
         * Shoot (only available if canshoot is true, but can still be attempted)
         * Reload
         * Change Weapon (like shoot, can be called even if it physically cant change weapon)
         */
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            //Move agent using first two actions as movement and rotation amounts
            controller.Move(new Vector2(Mathf.RoundToInt(Mathf.Clamp01(vectorAction[0])), vectorAction[1]));
            
            //Decides whether agent should shoot (clamped to 0 or 1 for dont shoot and shoot)
            controller.Shoot(Mathf.RoundToInt(Mathf.Clamp01(vectorAction[2])));

            //Decides whether agent should reload (clamped to 0 or 1 for dont reload and reload)
            controller.Reload(Mathf.RoundToInt(Mathf.Clamp01(vectorAction[3])));

            //Decides to change weapon (0 for dont, 1 - 5 for which weapon to attempt to change to)
            controller.ChangeWeapon(Mathf.RoundToInt(Mathf.Clamp(vectorAction[4],0,5)));
        }
        //Trigger if the controller has died
        if (!controller.isAlive)
        {
            Done();
            SetReward(-1f);
            controller.deaths++;
        }

    }

    public void GainedKill()
    {
        SetReward(1f);
        controller.kills++;
    }

    public void GenerateAIInfo()
    {

        //Player information 
        //Current direction
        AddVectorObs(controller.transform.up.x);
        AddVectorObs(controller.transform.up.y);
        AddVectorObs(controller.transform.up.z);

        //Players 2D position (because it cant move in z axis space anyway
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.y);

        //Players weapon info like its max ammo, the ammo currently in clip and the weapon being used
        AddVectorObs(weaponManager.currentAmmo[weaponManager.currentWeapon]);
        AddVectorObs(weaponManager.currentWeapon);

        //The players health is also added
        AddVectorObs(controller.health);

        bool checkCanShoot = false;
        //If any of these return true, the AI can try to shoot
        foreach (GameObject g3 in visiblePlayers)
        {
            Vector3 aimDirection = g3.transform.position - gameObject.transform.position;
            if (aimDirection == controller.transform.up || aimDirection == -controller.transform.up)
            {
                AddVectorObs(1);
                checkCanShoot = true;
            }

        }
        //Otherwise, he cant
        if (checkCanShoot == false)
        {
            AddVectorObs(0);
        }
    }
    public void GeneratePlayerInfo()
    {
        //Clear the list
        visiblePlayers.Clear();

        //Get list of players
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player1");

        //Check if the player is visible to the AI
        foreach (GameObject g1 in allPlayers)
        {
            if (Mathf.Abs(g1.transform.position.x - gameObject.transform.position.x) <= cameraDimensions.x
                && Mathf.Abs(g1.transform.position.y - gameObject.transform.position.y) <= cameraDimensions.y)
            {
                visiblePlayers.Add(g1);
            }
        }

        if (visiblePlayers.Count > 0)
        {
            //Find the closest player and add its values to the observations
            GameObject closestPlayer = visiblePlayers[0];
            float smallestDistance = Vector3.Distance(closestPlayer.transform.position, gameObject.transform.position);
            foreach (GameObject g2 in visiblePlayers)
            {
                if (g2 == visiblePlayers[0])
                    continue;

                float currentDistance = Vector3.Distance(gameObject.transform.position, g2.transform.position);
                if (currentDistance < smallestDistance)
                {
                    smallestDistance = currentDistance;
                    closestPlayer = g2;
                }

            }

            /*Then add the closest players values
             * health
             * rank
             * weapon
             * distance from AI agent
             * */
            if (closestPlayer.GetComponent<Client>())
            {
                //If playing in a real game
                Client g2c = closestPlayer.GetComponent<Client>();
                AddVectorObs(g2c.health);
                AddVectorObs(g2c.rank);
                AddVectorObs(g2c.clientWeaponManager.currentWeapon);
                AddVectorObs(Vector3.Distance(g2c.player.transform.position, gameObject.transform.position));
            }
            else if (closestPlayer.GetComponent<EnemyAgentController>())
            {
                //If training against other agents
                EnemyAgentController g2c = closestPlayer.GetComponent<EnemyAgentController>();
                AddVectorObs(g2c.health);
                AddVectorObs(g2c.rank);
                AddVectorObs(g2c.weaponManager.currentWeapon);
                AddVectorObs(Vector3.Distance(g2c.transform.position, gameObject.transform.position));
            }
        }
        else
        {
            //No players in the vicinity
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);
            AddVectorObs(0);
        }

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
        gameObject.transform.position = new Vector3(Random.Range(-resetParams["x-position"], resetParams["x-position"]), Random.Range(-resetParams["y-position"], resetParams["y-position"]), -10);

        //Reset controller variables
        controller.health = resetParams["health"];
        weaponManager.currentWeapon = (int)resetParams["weapon"];
        weaponManager.currentAmmo = weaponManager.clipSize;
        controller.rank = (int)resetParams["rank"];

        //Reset Agents direction and rotation
        controller.transform.up = new Vector3(Random.Range(-resetParams["x-direction"], resetParams["x-direction"]), Random.Range(-resetParams["y-direction"], resetParams["y-direction"]), 0);
        float rotation = Mathf.Atan2(controller.transform.up.x, controller.transform.up.y) * Mathf.Rad2Deg;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, -rotation);
    }
}