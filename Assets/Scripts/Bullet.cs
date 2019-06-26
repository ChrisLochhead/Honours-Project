using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bullet : NetworkBehaviour {

    public GameObject shooter;

    [SyncVar]
    public bool isHost;

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

        //If this shot killed the player, register it
        if (isServer && isHost) // this works for host
        {
            //Apply damage
            collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);

            if (collision.transform.parent.GetComponent<Client>().isDead)
            {
                shooter.transform.parent.GetComponent<Client>().UpdateScore(100);
                shooter.transform.parent.GetComponent<Client>().UpdateKills(1);
            }
            else
            {
                shooter.transform.parent.GetComponent<Client>().UpdateScore(10);
            }
        }
       else{

            //Apply damage
            collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);

            if (collision.transform.parent.GetComponent<Client>().health - damageAmount <= 0)
            {
                shooter.transform.parent.GetComponent<Client>().UpdateScore(100);
                shooter.transform.parent.GetComponent<Client>().UpdateKills(1);
            }
            else
            {
                shooter.transform.parent.GetComponent<Client>().UpdateScore(10);
            }
        }

    }
}
