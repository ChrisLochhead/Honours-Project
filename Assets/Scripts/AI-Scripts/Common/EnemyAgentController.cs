using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    public TextMeshProUGUI recordingKillCounter;
    public TextMeshProUGUI recordingDeathCounter;

    float moveLimiter = 0.1f;
    int lastRot = 0;
    public bool isRecording = false;

    private void Start()
    {
        score = 0;

        if (recordingKillCounter != null)
            isRecording = true;
    }

    void exitDemo()
    {
        GameObject g = GameObject.Find("sessionManager");

        if (g)
            g.GetComponent<ImitationManager>().ExitDemonstration();
    }

    private void Update()
    {
        //Stop model physics from making the model fly away
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        //transform.rotation = Quaternion.Euler(new Vector3(0,0,transform.rotation.z));

        //Ground the objects z co-ordinate 
        transform.position = new Vector3(transform.position.x, transform.position.y, -10);

    }

    private void FixedUpdate()
    {
        if (moveLimiter > 0.0f)
            moveLimiter -= Time.deltaTime;
        else
            moveLimiter = 1.0f;

        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
    }

    public void Move(Vector2 actions)
    {

        //Move at a fixed velocity, slightly faster than human controlled model to compensate for AI jittering
        if (actions.x == 1)
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, transform.up + gameObject.transform.position, 0.25f);


        //Update the rotation
        //rotate left
        if (actions.y == 1)
        {
            if (lastRot == 0)
            {
                transform.Rotate(new Vector3(0, 0, -3));
                lastRot = 0;
            }
            else if (moveLimiter == 1.0f)
            {
                transform.Rotate(new Vector3(0, 0, -3));
                lastRot = 0;
            }
        }
        //rotate right
        else if (actions.y == 2)
        {
            if (lastRot == 1)
            {
                transform.Rotate(new Vector3(0, 0, 3));
                lastRot = 1;
            }
            else if (moveLimiter == 1.0f)
            {
                transform.Rotate(new Vector3(0, 0, 3));
                lastRot = 1;
            }
        }
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
