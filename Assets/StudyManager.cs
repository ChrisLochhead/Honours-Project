using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class StudyManager : MonoBehaviour
{

    public List<int> Enemies;

    [SerializeField]
    float respawnTimer = 15.0f;

    bool isCompleted = false;

    public GameObject CurrentEnemy;
    GameObject[] spawnPoints;

    public GameObject[] enemyPrefabs;
    //0 : NMLAI
    //1 : DRL
    //2 : CL
    //3 : IL
    //4 : EL
    //5 : Real Player

        [SerializeField]
    int currentEnemyIndex = 0;

    // Use this for initialization
    void Start()
    {
        //List describes adversaries subject will face, hard coded for purposes of this
        //investigation, each number represents which type of enemy
        // 0 for NMLAI, 1 for DRL, 2 for CL, 3 for IL, 4 for EL and 5 for a real player

        Enemies = new List<int>() { 2, 1, 3, 4, 5};
        RespawnTimer();
    }

    void RespawnTimer()
    {
        respawnTimer -= Time.deltaTime;
    }

    void SpawnEnemy()
    {
        //spawn enemy
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint2");
        int rand = Random.Range(0, spawnPoints.Length);
        Vector3 spawn = spawnPoints[rand].transform.position;

        if (Enemies[currentEnemyIndex] != 5)
        {
            GameObject spawnObj = (GameObject)Instantiate(enemyPrefabs[currentEnemyIndex], new Vector3(spawn.x, spawn.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, Random.Range(0, 360)));
            NetworkServer.Spawn(spawnObj);
            CurrentEnemy = spawnObj;
        }
        else
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
            {
                if (g.GetComponent<Client>().isOpponent)
                {
                    CurrentEnemy = g;
                    break;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //deactivate self if not in study mode
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            if (!g.GetComponent<Client>().isStudy)
            {
                Destroy(this.gameObject);
            }
        }

        if (respawnTimer <= 0.0f && !CurrentEnemy && currentEnemyIndex <= Enemies.Count)
        {
            SpawnEnemy();
            respawnTimer = 15.0f;
        }

        if (CurrentEnemy)
        {
            if (Enemies[currentEnemyIndex] == 5)
            {
                if (CurrentEnemy.GetComponent<Client>().isDead)
                {
                    Destroy(CurrentEnemy);
                    currentEnemyIndex++;

                    if (currentEnemyIndex >= Enemies.Count)
                    {
                        isCompleted = true;
                    }

                    if (!isCompleted)
                    {
                        CurrentEnemy = null;
                        RespawnTimer();
                    }
                }
            }
            else
            if (CurrentEnemy.GetComponentInChildren<EnemyAgentController>().isAlive == false)
            {
                Destroy(CurrentEnemy);
                currentEnemyIndex++;

                if (currentEnemyIndex >= Enemies.Count)
                {
                    isCompleted = true;
                }

                if (!isCompleted)
                {
                    CurrentEnemy = null;
                    RespawnTimer();
                }
            }
        }
        else
        {
            if (isCompleted)
            {
                //game is over
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
                {
                    if (!g.GetComponent<Client>().isOpponent)
                    {
                        g.GetComponent<Client>().clientScoreBoard.ClientWon();
                    }
                }
            }
            else
            {
                RespawnTimer();
            }
        }
    }
}
