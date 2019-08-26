using UnityEngine;
//Gonna need to probably split this class into bullet and training-bullet
public class TrainingBullet : MonoBehaviour
{

    //Reference to the client who shot this bullet
    public GameObject shooter;

    //Amount of damage carried by this bullet
    public int damageAmount;


    void Start()
    {
        //Set the objects scale as prefab doesn't keep it static
        transform.localScale = new Vector3(8.1f, 8.1f, 23.1f);
    }

    private void Update()
    {
        if (Vector3.Distance(this.gameObject.transform.position, shooter.transform.position) > 40)
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

        //Training function for AI agents, commented out during study
        //Check if it has hit an enemyplayer
        if (collision.transform.GetComponent<EnemyAgentController>())
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

        Debug.Log("getting in here");
        //Bullet code for AI training
        if (collision.transform.GetComponent<EnemyAgentReinforcement>())
        {
            Debug.Log("hit");
            if (collision.transform.GetComponent<EnemyAgentController>().isAlive == false)
            {
                if (shooter.GetComponent<EnemyAgentReinforcement>())
                {
                    shooter.GetComponent<EnemyAgentReinforcement>().GainedKill();
                }
            }
        }
    }
}

