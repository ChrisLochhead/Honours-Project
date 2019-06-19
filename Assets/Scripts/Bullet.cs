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

    private void Update()
    {
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
        if (collision.gameObject.tag == "Bullet" || collision.gameObject.transform.parent.GetComponent<Client>().team == shooter.transform.parent.GetComponent<Client>().team)
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            Destroy(this.gameObject);
            return;
        }

        //Check if it has hit an enemyplayer
        if (collision.transform.parent.GetComponent<Client>().team != shooter.transform.parent.GetComponent<Client>().team)
        {
            CheckEnemyCollision(collision);
            Destroy(this.gameObject);
            return;
        }
    }

    void CheckEnemyCollision(Collision collision)
    {
        //Apply damage
        collision.gameObject.transform.parent.GetComponent<Client>().Hit(15);

        //If this shot killed the player, register it
        if (collision.transform.parent.GetComponent<Client>().isDead)
        {
            GameObject.Find("GameManager").GetComponent<Game>().OnKillRegistered(shooter, collision.gameObject);
            shooter.transform.parent.GetComponent<Client>().kills++;
            shooter.transform.parent.GetComponent<Client>().UpdateScore(100);
            collision.transform.parent.GetComponent<Client>().deaths++;
        }
        else
        {
            shooter.transform.parent.GetComponent<Client>().UpdateScore(10);
            Debug.Log("shooter score : " + shooter.transform.parent.GetComponent<Client>().score);
        }

    }
}
