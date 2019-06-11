using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public int shooter;
    public int damageAmount;
	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        //if (isTemplate == false && collision.gameObject.GetComponent<Player>().GetPlayerNo() != shooter.gameObject.GetComponent<Player>().GetPlayerNo())
        //{
        //    Debug.Log("hit");
        //    if (collision.gameObject.tag == "Obstacle")
        //    {
        //        Debug.Log("orange");
        //        Destroy(this.gameObject);
        //    }

        //    CheckEnemyCollision(collision);

        //    if(collision.gameObject.tag == "Bullet" || collision.gameObject.tag == shooter.gameObject.tag)
        //    {
        //        Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        //    }
        //}
    }

    void CheckEnemyCollision(Collision collision)
    {
        Debug.Log("in enemy collision");
            Debug.Log("called damage");
            collision.gameObject.GetComponent<Player>().setHealth(15);
    }
}
