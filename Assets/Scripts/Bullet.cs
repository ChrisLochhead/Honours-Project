using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public GameObject shooter;
    public int damageAmount;
	// Use this for initialization
	void Start () {
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
    }


    private void OnCollisionEnter(Collision collision)
    {

        //Check if its hit an obstacle
        if (collision.gameObject.tag == "Obstacle")
        {
            Destroy(this.gameObject);
            return;
        }

        //Check if its hit a friendly, or another bullet
        if (collision.gameObject.tag == "Bullet" || collision.gameObject.GetComponent<Client>().team == shooter.gameObject.GetComponent<Client>().team)
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            return;
        }

        //Check if it has hit an enemy player
        if (collision.gameObject.GetComponent<Client>().team != shooter.gameObject.GetComponent<Client>().team)
        {
            CheckEnemyCollision(collision);
        }

      
    }

    void CheckEnemyCollision(Collision collision)
    {
        //Apply damage
        collision.gameObject.transform.parent.GetComponent<Client>().TakeDamage(15);

        //If this shot killed the player, register it
        if(collision.gameObject.GetComponent<Client>().isDead)
        {
            GameObject.Find("gameManager").GetComponent<Game>().OnKillRegistered(shooter, collision.gameObject);
            shooter.GetComponent<Client>().kills++;
            collision.gameObject.GetComponent<Client>().deaths++;
        }

        //Destroy the bullet
        Destroy(this.gameObject);
    }
}
