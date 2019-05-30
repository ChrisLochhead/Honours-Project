﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    float velocity;
    Vector3 direction;
    public GameObject shooter;

    int type;

    public bool isTemplate;
    bool templateSet;

	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
        velocity = 3.5f;
        templateSet = false;

       
        if(shooter)
        {

            //find out who shot this bullet
            if (shooter.name == "Adversary")
                type = 0;
            else
                type = 1;

            //calculate rotation
            Quaternion rot = transform.rotation;
            rot = shooter.transform.rotation;
            rot *= Quaternion.Euler(-90, 0, 0);
            transform.rotation = rot;

            //calculate trajectory
            direction = transform.position - Camera.main.ScreenToWorldPoint(shooter.GetComponent<Player>().crosshair.transform.position);

        }

    }
	
	// Update is called once per frame
	void Update () {

        //make players immune to their own respective bullets
        if (type == 1)
            Physics.IgnoreLayerCollision(10, 10);
        else
            Physics.IgnoreLayerCollision(11, 12);

        if (isTemplate == false)
        {

            if (!templateSet)
                this.gameObject.AddComponent<MeshRenderer>();
            // Debug.Log(transform.position);
            //transform.position = Vector3.MoveTowards(transform.position, direction, velocity);

            Vector3 pos;
            pos = transform.position;
            pos += velocity * -direction * Time.deltaTime;
           // pos.z = -10;
            transform.position = pos;
        }

    }

    private void FixedUpdate()
    {
        //transform.position = Vector3.MoveTowards(transform.position, direction, 1f);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (isTemplate == false)
        {
            Debug.Log("hit");
            if (collision.gameObject.tag == "Obstacle")
            {
                Debug.Log("orange");
                Destroy(this.gameObject);
            }

            CheckEnemyCollision(collision);

            if(collision.gameObject.tag == "Bullet" || collision.gameObject.tag == shooter.gameObject.tag)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            }
        }
    }

    void CheckEnemyCollision(Collision collision)
    {
        Debug.Log("in enemy collision");

        if(type == 0 && collision.gameObject.tag == "Player1" )
        {
            Debug.Log("called damage");
            collision.gameObject.GetComponent<Player>().setHealth(15);
        }

        if (type == 1 && collision.gameObject.tag == "Adversary")
        {

        }
    }
}
