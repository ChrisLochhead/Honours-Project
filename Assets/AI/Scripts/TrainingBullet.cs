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
        if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 150)
        {
            Destroy(this.gameObject);
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

        //Training function for AI agents, commented out during study
        //Check if it has hit an enemyplayer (DRL or NMLA), this function is specific to DRL training
        if (collision.transform.GetComponent<EnemyAgentController>() || collision.transform.GetComponent<NMLAgent>() && shooter.gameObject.GetComponent<CurriculumReinforcement>())
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
        
    }

    void CheckEnemyCollision(Collision collision)
    {
        CurriculumReinforcement enemy = collision.transform.GetComponent<CurriculumReinforcement>();
        //Bullet code for AI training
        if (enemy)
        {
            EnemyAgentController enemyController = collision.transform.GetComponent<EnemyAgentController>();
            //Apply damage

            enemyController.health -= damageAmount;
            if(enemyController.health <= 0)
            {
                enemyController.isAlive = false;
                shooter.GetComponent<CurriculumReinforcement>().GainedKill();
            }

        }

        //Bullet code for AI training
        if (collision.transform.GetComponent<NMLAgent>())
        {
            //Apply damage
            collision.transform.GetComponent<NMLAgent>().health -= damageAmount;
            if (collision.transform.GetComponent<NMLAgent>().health <= 0)
            {
                collision.transform.GetComponent<NMLAgent>().isAlive = false;
                shooter.GetComponent<CurriculumReinforcement>().GainedKill();
            }

        }
    }
}

