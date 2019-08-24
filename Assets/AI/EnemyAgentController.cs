﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgentController : MonoBehaviour {

    public Vector3 direction;
    public float velocity;
    public int rank;
    public float health;

    public bool isAlive = true;

    public int score;
    public EnemyAgentWeaponManager weaponManager;

    public int kills;
    public int deaths;

    public void move(Vector2 actions)
    {
        //Assign it to either 1.0 or 0.0: as players cannot control their exact speed, only whether they are moving
        //or not, then so must the AI
        if (actions.x >= 0.5)
            actions.x = 1.0f;
        else
            actions.x = 0.0f;

        //Move at a fixed velocity of 0.3
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, direction+gameObject.transform.position, actions.x/3.3333f);

        //Update the rotation
        actions.y = Mathf.Clamp(actions.y, -0.5f, 0.5f);
        gameObject.transform.rotation *= Quaternion.Euler(0, 0, actions.y);
    }

    public void reload(int action)
    {
        weaponManager.Reload(action);
    }

    public void changeWeapon(int action)
    {
        weaponManager.SwitchWeapon(action);
    }

    public void shoot(int action)
    {
        weaponManager.Shoot(action);
    }

}
