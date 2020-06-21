using UnityEngine;
using UnityEngine.Networking;
//Gonna need to probably split this class into bullet and training-bullet
public class Bullet : NetworkBehaviour
{

    //Reference to the client who shot this bullet
    public GameObject shooter;

    //Check if host, because this causes bullets to behave differently
    [SyncVar]
    public bool isHost;

    //Amount of damage carried by this bullet
    public int damageAmount;


    void Start()
    {
        //Set the objects scale as prefab doesn't keep it static
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
    }

    private void Update()
    {
        //Optional distance drop off for bullets (training-only)
        if (shooter)
        {
            if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 100)
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
        CheckEnemyCollision(collision);

        //Check if its hit a coin, or another bullet
        if (collision.gameObject.tag == "Bullet" || collision.gameObject.GetComponent<Coin>())
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            if (!collision.gameObject.GetComponent<Coin>())  
                Destroy(this.gameObject); 
            return;
        }

        //Check for friendly fire
        if (collision.gameObject.transform.parent.GetComponent<Client>() && shooter.transform.parent.GetComponent<Client>())
        {
            if (collision.gameObject.transform.parent.GetComponent<Client>().team == shooter.transform.parent.GetComponent<Client>().team)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                Destroy(this.gameObject);
                return;
            }
        }
    }

    void CheckEnemyCollision(Collision collision)
    {
        //AI Vs AI (Bot shooting)
        if (collision.gameObject.GetComponent<EnemyAgentController>() && shooter.transform.GetComponent<NMLAgentTrainer>())
        {
            if (collision.transform.GetComponent<EnemyAgentController>().health - damageAmount <= 0)
            {
                collision.gameObject.GetComponent<EnemyAgentController>().health -= damageAmount;
                collision.gameObject.GetComponent<EnemyAgentController>().isAlive = false;
                collision.transform.GetComponent<EnemyAgentController>().deaths++;
            }
            else
                collision.gameObject.GetComponent<EnemyAgentController>().health -= damageAmount;

            Destroy(this.gameObject);
        }

        //AI Vs AI (AI shooting)
        if (collision.gameObject.GetComponent<NMLAgentTrainer>() && shooter.transform.GetComponent<EnemyAgentController>())
        {
            if (collision.transform.GetComponent<NMLAgentTrainer>().health - damageAmount <= 0)
            {
                collision.gameObject.GetComponent<NMLAgentTrainer>().health -= damageAmount;
                collision.gameObject.GetComponent<NMLAgentTrainer>().isAlive = false;
                shooter.transform.GetComponent<AIController>().GainedKill();
            }
            else
            {
                collision.gameObject.GetComponent<NMLAgentTrainer>().health -= damageAmount;
                shooter.GetComponent<AIController>().InflictedDamage();
            }

            Destroy(this.gameObject);
        }


        // Player VS Player
        if (collision.transform.parent && shooter.transform.parent)
        {
            if (collision.gameObject.transform.parent.GetComponent<Client>() && shooter.transform.parent.GetComponent<Client>())
            {
                //Check if opponent is dead and apply points and kills appropriately
                if (collision.transform.parent.GetComponent<Client>().health <= damageAmount)
                {
                    collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);
                    shooter.transform.parent.GetComponent<Client>().UpdateScore(100);
                    shooter.transform.parent.GetComponent<Client>().UpdateKills(1);
                }
                else
                {
                    collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);
                    shooter.transform.parent.GetComponent<Client>().UpdateScore(10);
                }
                Destroy(this.gameObject);
            }
        }

        //NMLAI Vs Player
        if (collision.transform.parent)
        {
            if (collision.gameObject.transform.parent.GetComponent<Client>() && shooter.transform.GetComponent<EnemyAgentController>())
            {
                collision.gameObject.transform.parent.GetComponent<Client>().Hit(damageAmount);
                Destroy(this.gameObject);
            }
        }

        if (shooter.transform.parent)
        {
            //Player Vs NMLAI
            if (collision.gameObject.GetComponent<EnemyAgentController>() && shooter.transform.parent.GetComponent<Client>())
            {
                if (collision.transform.GetComponent<EnemyAgentController>().health - damageAmount <= 0)
                {
                    collision.gameObject.GetComponent<EnemyAgentController>().health -= damageAmount;
                    if (shooter.transform.parent.GetComponent<Client>())
                    {
                        shooter.transform.parent.GetComponent<Client>().UpdateScore(100);
                        collision.gameObject.GetComponent<EnemyAgentController>().isAlive = false;
                    }
                }
                else
                {
                    collision.gameObject.GetComponent<EnemyAgentController>().health -= damageAmount;
                    if (shooter.transform.parent.GetComponent<Client>())
                        shooter.transform.parent.GetComponent<Client>().UpdateScore(10);
                }
                Destroy(this.gameObject);
            }
        }
    }
}
