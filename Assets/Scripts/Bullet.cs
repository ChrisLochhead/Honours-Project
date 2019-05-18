using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    float velocity;
    Vector3 direction;
    int player;

	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
        //transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));

        velocity = 0.5f;

        GameObject tmp = GameObject.Find("Player1");

        if (tmp)
        {
            transform.rotation = tmp.transform.rotation;
            direction = tmp.GetComponent<Player>().getDirection();
        }
    }
	
	// Update is called once per frame
	void Update () {

            Vector3 pos;
            pos = transform.position;
            pos += velocity * direction * Time.deltaTime;
            transform.position = pos;

	}
}
