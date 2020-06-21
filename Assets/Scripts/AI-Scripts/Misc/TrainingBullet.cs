using UnityEngine;
//Gonna need to probably split this class into bullet and training-bullet
public class TrainingBullet : MonoBehaviour
{

    //Reference to the client who shot this bullet
    public GameObject shooter;

    //Amount of damage carried by this bullet
    public int damageAmount;

    private void Update()
    {
        //Ignore collisions from other bullets
        Physics.IgnoreLayerCollision(10, 10);

        //Maximum shot distance before bullet dissapears
        if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 50)
        {
            //Destroy(this.gameObject);
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

        if (collision.transform.gameObject == shooter)
            return;

        //Check if its hit a friendly, or another bullet
        if (collision.gameObject.GetComponent<Coin>())
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            if (!collision.gameObject.GetComponent<Coin>())
                Destroy(this.gameObject);
            return;
        }

        //Training function for AI agents, commented out during study
        //Check if it has hit an enemyplayer (DRL or NMLA), this function is specific to DRL training
        CheckEnemyCollision(collision);

        //It should be destroyed no matter what it hits other than another bullet
        //so destroy this for any edge cases like friendly fire
        Destroy(this.gameObject);

    }

    void CheckEnemyCollision(Collision collision)
    {

        //Study -- NMLAI Vs Player
        if (collision.transform.GetComponent<Client>() && shooter.transform.GetComponent<NMLAgent>())
        {
            //Apply damage
            collision.transform.GetComponent<Client>().health -= damageAmount;
            if (collision.transform.GetComponent<Client>().health <= 0)
            {
                collision.transform.GetComponent<Client>().isDead = true;
            }
            Destroy(this.gameObject);
        }

        //Study -- Player Vs NMLAI
        if (collision.transform.GetComponent<NMLAgent>() && shooter.transform.GetComponent<Client>())
        {
            //Apply damage
            collision.transform.GetComponent<NMLAgent>().health -= damageAmount;
            if (collision.transform.GetComponent<NMLAgent>().health <= 0)
            {
                collision.transform.GetComponent<NMLAgent>().isAlive = false;
            }
            Destroy(this.gameObject);
        }

        //Bullet code for AI training
        //For player hitting enemy
        if (collision.transform.GetComponent<NMLAgentTrainer>() && shooter.transform.GetComponent<AIController>())
        {
            //Apply damage
            collision.transform.GetComponent<NMLAgentTrainer>().health -= damageAmount;
            if (collision.transform.GetComponent<NMLAgentTrainer>().health <= 0)
            {
                collision.transform.GetComponent<NMLAgentTrainer>().isAlive = false;
                shooter.GetComponent<AIController>().GainedKill();
            }else
            {
                shooter.GetComponent<AIController>().InflictedDamage();
            }
            Destroy(this.gameObject);
        }

        //For enemy hitting player
        if (shooter.transform.GetComponent<NMLAgentTrainer>() && collision.transform.GetComponent<AIController>())
        {
            //Apply damage
            collision.transform.GetComponent<EnemyAgentController>().health -= damageAmount;
            if (collision.transform.GetComponent<EnemyAgentController>().health <= 0)
            {
                collision.transform.GetComponent<EnemyAgentController>().isAlive = false;
            }
            Destroy(this.gameObject);
        }

        //For in test enemy hitting player or vice versa
        if (shooter.transform.GetComponent<EnemyAgentController>() && collision.transform.GetComponent<EnemyAgentController>())
        {
            //Apply damage
            collision.transform.GetComponent<EnemyAgentController>().health -= damageAmount;
            if (collision.transform.GetComponent<EnemyAgentController>().health <= 0)
            {
                collision.transform.GetComponent<EnemyAgentController>().isAlive = false;
            }
            Destroy(this.gameObject);
        }

        //For study collision detection: enemy to player
        if (collision.transform.parent.GetComponent<Client>())
        {
            Client c = collision.transform.parent.GetComponent<Client>();
            c.health -= damageAmount;
            if (c.health <= 0)
                c.isDead = true;

            Destroy(this.gameObject);
        }

        //For study collision detection: enemy to player
        if (collision.transform.GetComponent<EnemyAgentController>() && shooter.transform.parent.GetComponent<Client>())
        {
            EnemyAgentController c = collision.transform.GetComponent<EnemyAgentController>();

            c.health -= 100;// damageAmount;
            if (c.health <= 0)
            {
                c.isAlive = false;
            }
            Destroy(this.gameObject);
        }

        //For study collision detection: enemy to player
        if (collision.transform.parent.GetComponent<Client>() && shooter.transform.parent.GetComponent<Client>())
        {
            Client c = collision.transform.parent.GetComponent<Client>();

            c.health -= damageAmount;
            if (c.health <= 0)
            {
                c.isDead = true;
            }
            Destroy(this.gameObject);
        }
    }
}

