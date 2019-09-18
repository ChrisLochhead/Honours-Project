using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAgentController : MonoBehaviour {
    
    public float velocity;
    public int rank;
    public float health;

    public bool isAlive = true;
    public bool hittingWall = false;

    public int score;
    public EnemyAgentWeaponManager weaponManager;

    public int kills;
    public int deaths;

    private void Start()
    {
        score = 0;
    }

    private void Update()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        //Ground the objects z co-ordinate 
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    public void Move(Vector2 actions)
    {
        //Move at a fixed velocity
        if (actions.x == 1)
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, transform.up + gameObject.transform.position, 0.9f);

        //Update the rotation
        //rotate left
        if(actions.y == 1)
        transform.Rotate(new Vector3(0, 0, -8));
        //rotate right
        if(actions.y == 2)
        transform.Rotate(new Vector3(0, 0, 8));

    }

    public void Reload(float action)
    {
        weaponManager.Reload(action);
    }

    public void ChangeWeapon(float action)
    {
        weaponManager.SwitchWeapon(action);
    }

    public void Shoot(float action)
    {
        weaponManager.Shoot(action);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        hittingWall = true;

        Physics.IgnoreLayerCollision(9, 9);
    }

    private void OnCollisionExit(Collision collision)
    {
        hittingWall = false;
    }
}
