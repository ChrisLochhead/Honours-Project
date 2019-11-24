using UnityEngine;
using UnityEngine.Networking;
//Gonna need to probably split this class into bullet and training-bullet
public class Bullet : NetworkBehaviour {

    //Reference to the client who shot this bullet
    public GameObject shooter;

    //Check if host, because this causes bullets to behave differently
    [SyncVar]
    public bool isHost;

    //Amount of damage carried by this bullet
    public int damageAmount;


	void Start () {
        //Set the objects scale as prefab doesn't keep it static
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
    }

    private void Update()
    {
        //Optional distance drop off for bullets (training-only)
        if (shooter)
        {
            if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 40)
            {
                Destroy(this.gameObject);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {

        //Check if its hit an obstacle
        if (collision.gameObject.tag == "Obstacle")
        {
            Destroy(this.gameObject);
            return;
        }

        //Training function for AI agents, commented out during study
        //Check if it has hit an enemyplayer
        if (collision.transform.GetComponent<EnemyAgentController>())
        {
            CheckEnemyCollision(collision);
            Destroy(this.gameObject);
            return;
        }

        //Check if its hit a friendly, or another bullet
        if (collision.gameObject.tag == "Bullet" || collision.gameObject.transform.parent.GetComponent<Client>().team == shooter.transform.parent.GetComponent<Client>().team || collision.gameObject.GetComponent<Coin>())
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            if(!collision.gameObject.GetComponent<Coin>())
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

        //This applies damage differently depending on whose bullet fired it
        //because of network latency. If a client shoots a kill shot it will not
        //register until the next shot, so it calculates it before the damage is applied
        //to circumvent this.
        if (isServer && isHost)
        {
            //Apply damage
            collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);

            //Check if opponent is dead and apply points and kills appropriately
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
        else
        {

            //Bullet code for AI training
            if (collision.transform.GetComponent<CurriculumReinforcement>())
            {
                if (collision.transform.GetComponent<EnemyAgentController>().isAlive == false)
                {
                    if (shooter.GetComponent<CurriculumReinforcement>())
                    {
                        shooter.GetComponent<CurriculumReinforcement>().GainedKill();
                    }
                }
            }
            else
            {
                //Apply damage
                collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);

                //Check (if client) if the opponent WILL die before the shot has hit
                //then update kills and score as normal
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
}
