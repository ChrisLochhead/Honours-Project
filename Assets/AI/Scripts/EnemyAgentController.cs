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
        //Physics.IgnoreLayerCollision(9, 9);

        //Ground the objects z co-ordinate 
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);

        //transform.rotation = Quaternion.Euler(0, 0, transform.rotation.z);
    }

    public void Move(Vector2 actions)
    {
        //Move at a fixed velocity of 0.3 or 0.0
        if (actions.x > 0)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, transform.up + gameObject.transform.position, 0.9f);
        }

        //Update the rotation
        actions.y = Mathf.Clamp(actions.y, -3.5f, 3.5f);
        transform.Rotate(new Vector3(0, 0, actions.y *2.5f));

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
    }

    private void OnCollisionExit(Collision collision)
    {
        hittingWall = false;
    }
}
