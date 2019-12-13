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
        //ignore collisions from other bullets
        Physics.IgnoreLayerCollision(10, 10);

        if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 30 && shooter.GetComponent<AIController>())
        {
            Destroy(this.gameObject);
        }
        else if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 150)
            Destroy(this.gameObject);

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

        //Training function for AI agents, commented out during study
        //Check if it has hit an enemyplayer (DRL or NMLA), this function is specific to DRL training
        if (collision.transform.GetComponent<EnemyAgentController>() || collision.transform.GetComponent<NMLAgentTrainer>() && shooter.gameObject.GetComponent<AIController>() ||
            shooter.transform.GetComponent<NMLAgentTrainer>() && collision.gameObject.GetComponent<AIController>() || collision.transform.parent.GetComponent<Client>() || shooter.transform.parent.GetComponent<Client>() && collision.transform.GetComponent<AIController>())
        {
            CheckEnemyCollision(collision);
            Destroy(this.gameObject);
            return;
        }

        //Check if its hit a friendly, or another bullet
        if (collision.gameObject.GetComponent<Coin>())
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            if (!collision.gameObject.GetComponent<Coin>())
                Destroy(this.gameObject);
            return;
        }

        //It should be destroyed no matter what it hits other than another bullet
        //so destroy this for any edge cases like friendly fire
        Destroy(this.gameObject);

    }

    void CheckEnemyCollision(Collision collision)
    {

        //Bullet code for AI training (small arena)
        //For player hitting enemy
        if (collision.transform.GetComponent<NMLAgentTrainer>() && shooter.transform.GetComponent<AIController>())
        {
            //Apply damage
            collision.transform.GetComponent<NMLAgentTrainer>().health -= damageAmount;
            if (collision.transform.GetComponent<NMLAgentTrainer>().health <= 0)
            {
                collision.transform.GetComponent<NMLAgentTrainer>().isAlive = false;
                shooter.GetComponent<AIController>().GainedKill();
            }

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

        }

        //For study collision detection: enemy to player
        if(collision.transform.parent.GetComponent<Client>())
        {
            Client c = collision.transform.parent.GetComponent<Client>();
            c.health -= damageAmount;
            if (c.health <= 0)
                c.isDead = true;
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
        }

        //For study collision detection: enemy to player
        if (collision.transform.parent.GetComponent<Client>() && shooter.transform.parent.GetComponent<Client>())
        {
            Client c = collision.transform.parent.GetComponent<Client>();

            c.health -= 100;// damageAmount;
            if (c.health <= 0)
            {
                c.isDead = true;
            }
        }


        Destroy(this.gameObject);
    }
}

