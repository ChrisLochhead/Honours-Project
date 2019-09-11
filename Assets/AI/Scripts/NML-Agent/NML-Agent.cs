using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NMLAgent : MonoBehaviour {

    int health;
    int ammo;
    public Camera personalCamera;

    bool actionMode;

    public GameObject player;

    public PathGrid pathGrid;

	// Use this for initialization
	void Start () {
        actionMode = false;
        health = 100;
        ammo = 16;
	}

    void SearchArea()
    {
        //Check if player is within the agents camera
        Vector3 screenPoint = personalCamera.WorldToViewportPoint(player.transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        onScreen = actionMode;
    }

    void Idle()
    {
        //pick a node if starting to idle

        //if timer isnt up

        //if its not clear, pick another one


        //otherwise, restart the timer and walk in this direction
    }

    void Attack()
    {
        //get players direction

        //rotate towards this direction (determined by difficulty setting)

        //if facing the right direction, shoot

        //depending on how close to the right direction, start moving

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
