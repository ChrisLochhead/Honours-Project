using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgentController : MonoBehaviour {

    public Vector3 direction;
    public float velocity;
    public int rank;
    public int ammo;
    public int weapon;
    public float health;
    public int clip;

    public bool isAlive = true;

    public int score;
    public EnemyAgentWeaponManager weaponManager;

    public int kills;
    public int deaths;

    public void move(Vector2 actions)
    {
        //Prevents agent from moving faster than the player or moving backwards
        actions.x = Mathf.Clamp(actions.x, 0.0f, 0.3f);
        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, direction+gameObject.transform.position, actions.x);

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

    // Use this for initialization
    void Start () {
        velocity = 0.3f;
	}

}
