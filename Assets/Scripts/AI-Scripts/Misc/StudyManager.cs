using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StudyManager : NetworkBehaviour
{
    public List<int> Enemies;

    [SyncVar]
    public float respawnTimer = 5.0f;

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

    public int currentEnemyIndex = 0;

    //Initialiser variables
    public bool gameInitialised = false;
    bool mapShrunk = false;
    bool isOver = false;

    // Use this for initialization
    void Start()
    {
        
        //List describes adversaries subject will face, hard coded for purposes of this
        //investigation, each number represents which type of enemy
        // 0 for NMLAI, 1 for DRL, 2 for CL, 3 for IL, 4 for EL and 5 for a real player
        //Change grid dimensions to accomodate smaller study map
        PathGrid p = GameObject.Find("PathGrid").GetComponent<PathGrid>();
        p.gridDimensions.x = 200;
        p.gridDimensions.y = 175;
        p.transform.SetPositionAndRotation(new Vector3(-20, -55, 0), p.transform.rotation);
        p.GenerateGrid();

        Enemies = new List<int>() { 0, 1, 5};
    }

    void ShrinkMap(GameObject wallParent)
    {
        //Get references to the boundary walls
        GameObject leftWall = wallParent.transform.Find("GreyWall - Left").gameObject;
        GameObject rightWall = wallParent.transform.Find("GreyWall - Right").gameObject;
        GameObject topWall = wallParent.transform.Find("GreyWall - Top").gameObject;
        GameObject bottomWall = wallParent.transform.Find("GreyWall - Bottom").gameObject;

        //Tighten them up for purposes of the study 
        Vector3 temp = leftWall.transform.localPosition;
        temp.x = -0.75f;
        leftWall.transform.localPosition = temp;

        temp = rightWall.transform.localPosition;
        temp.x = -1.55f;
        rightWall.transform.localPosition = temp;

        temp = topWall.transform.localPosition;
        temp.y = 0.001f;
        topWall.transform.localPosition = temp;

        temp = bottomWall.transform.localPosition;
        temp.y = -0.005f;
        bottomWall.transform.localPosition = temp;

        mapShrunk = true;
    }
    public void RespawnTimer()
    {
        //Timer divided by three to account for the potential three times the respawn timer can
        //be called per frame, also only iterated on host so it is synced among clients.
        if(gameInitialised && isServer)
        respawnTimer -= Time.deltaTime / 3.0f;
        else if (gameInitialised && !isServer)
        {
            respawnTimer = GameObject.Find("StudyManager").GetComponent<StudyManager>().respawnTimer;
        }
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

            //Assign the player to the pathgrid incase it is an NMLAI
            GameObject.Find("PathGrid").GetComponent<PathGrid>().player = CurrentEnemy;
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

    void StudyOver()
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
    // Update is called once per frame
    void Update()
    {
        if (!isOver) { 
        if(GameObject.Find("Border Wall(Clone)") && !mapShrunk)
        {
            ShrinkMap(GameObject.Find("Border Wall(Clone)"));
        }
        //deactivate self if not in study mode
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            if (g.GetComponent<Client>())
            {
                if (!g.GetComponent<Client>().isStudy)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        if (respawnTimer <= 0.0f && !CurrentEnemy && currentEnemyIndex <= Enemies.Count)
        {
            SpawnEnemy();
            respawnTimer = 5.0f;
        }

            if (CurrentEnemy)
            {
                if (Enemies[currentEnemyIndex] == 5)
                {
                    if (CurrentEnemy.GetComponent<Client>().isDead)
                    {
                        currentEnemyIndex++;

                        if (currentEnemyIndex >= Enemies.Count)
                        {
                            isCompleted = true;
                            StudyOver();
                            isOver = true;
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
                    StudyOver();
                    isOver = true;
                }
                else
                {
                    RespawnTimer();
                }
            }
        }
    }
}
