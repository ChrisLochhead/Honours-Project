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
        if (collision.transform.GetComponent<EnemyAgentController>() || collision.transform.GetComponent<NMLAgentTrainer>() && shooter.gameObject.GetComponent<CurriculumReinforcement>())
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

                //Check the shooter isn't an NMLAgent
                if(shooter.GetComponent<CurriculumReinforcement>())
                shooter.GetComponent<CurriculumReinforcement>().GainedKill();
            }

        }

        //Bullet code for AI training
        if (collision.transform.GetComponent<NMLAgentTrainer>())
        {
            //Apply damage
            collision.transform.GetComponent<NMLAgentTrainer>().health -= damageAmount;
            if (collision.transform.GetComponent<NMLAgentTrainer>().health <= 0)
            {
                collision.transform.GetComponent<NMLAgentTrainer>().isAlive = false;
                shooter.GetComponent<CurriculumReinforcement>().GainedKill();
            }

        }

        Destroy(this.gameObject);
    }
}

