using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    float velocity;
    Vector3 direction;
    int player;
    public GameObject shooter;

    public bool isTemplate;

	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);

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

        if (isTemplate == false)
        {
            Vector3 pos;
            pos = transform.position;
            pos += velocity * direction * Time.deltaTime;
            pos.z = -10;
            transform.position = pos;
        }

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

            if(collision.gameObject.tag == "Bullet" || collision.gameObject.tag == shooter.gameObject.tag)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            }
        }
    }
}
