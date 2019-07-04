using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Client : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] clientComponents;

    public Camera lobbyCam;

    public GameObject player;

    public bool isLocal;

    public GameObject[] spawnPoints;

    //Camera
    public Camera playerCam;

    //Physics
    float velocity;
    float movementSpeed;

    //Crosshair
    Vector3 currentDirection;
    Vector3 crosshairDirection;

    public GameObject crosshair;
    public GameObject crosshairMarker;

    //Animations
    public Animator anim;
    public Rigidbody body;

    //Armour
    public int[] armourRatings = { 0, 1, 2, 3, 5, 7, 8, 10, 15 };
    public int armour;

    //HUD stuff
    public int score = 0;
    public int health = 100;
    int totalHealth = 100;

    public int rank = 0;

    //For multiplayer
    int playerNo;

    //For interaction with the controller script
    public Vector3 currentMPos;

    //Respawn screen
    public GameObject respawnScreen;

    //To record kills and deaths in-game
    [SyncVar]
    public int kills = 0;
    [SyncVar]
    public int deaths = 0;

    //Recording whether character is currently alive
    [SyncVar(hook = "SetLiving")] public bool isDead = false;
    bool reset = false;
    float deathRotation = 0;


    //Registers which team the player is in
    [SyncVar]
    public int team = 0;

    //For win conditions
    public bool hasWon = false;
    public bool hasLost = false;

    //For visible name
    [SyncVar]
    public string playerName;

    //For recording kills and deaths over network
    public bool needsUpdate = false;

    //Scoreboard
    public ClientScoreBoard clientScoreBoard;

    //Name selector
    public ClientNameMenu clientNameMenu;

    //HUD
    public ClientHUD clientHUD;

    //Floating healthbar 
    public ClientHealthBar clientHealthBar;

    //Weapon controller
    public ClientWeaponManager clientWeaponManager;

    //Model
    public GameObject characterModel;

    //Accompanying textures
    public Material BlueTeamMaterial;
    public Material RedTeamMaterial;

    //For pausing at the start of games
    public bool isPaused = true;

    // Use this for initialization
    void Start()
    {

        if (!isLocalPlayer)
        {
            isLocal = false;
            //Disable all components unique to the client
            for (int i = 0; i < clientComponents.Length; i++)
            {
                clientComponents[i].enabled = false;
            }

            foreach (GameObject g in GameObject.Find("Network Manager").GetComponent<NetworkManager>().spawnPrefabs)
            {
                ClientScene.RegisterPrefab(g);
            }

        }
        else
        {
            isLocal = true;
            //sceneCam.gameObject.SetActive(false);

            //Hide UI elements that player shouldn't see
            //floatingHealthBar.GetComponent<CanvasRenderer>().SetAlpha(0);
            //floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(0);
        }

        //Find team numbers
        int temp1 = -1;
        int temp2 = 0;

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            if (g.GetComponent<Client>().team == 0)
                temp1++;
            else
                temp2++;
        }

        if (temp1 == temp2)
            CmdSetTeam(Random.Range(0, 2));
        else if (temp1 > temp2)
            CmdSetTeam(1);
        else
            CmdSetTeam(0);

        //Get spawnpoints from team
        //if (team == 0)
        //    spawnPoints = team1Spawns;
        //else
        //    spawnPoints = team2Spawns;

        //set up spawnpoint
        //Respawn();
    }

    [Command]
    public void CmdSetName(string name)
    {
        SetName(name);
        RpcSetName(name);
    }

    public void SetName(string name)
    {
        playerName = name;
    }

    [ClientRpc]
    public void RpcSetName(string name)
    {
        playerName = name;
    }
    public void InitialisePlayer()
    {
        //Disable the lobby now that the game has begun
        lobbyCam.gameObject.SetActive(false);
        isPaused = false;

        clientHUD.gameObject.SetActive(true);

        clientNameMenu.InitialisePlayerName(playerName);

        //ignore collisions between players
        Physics.IgnoreLayerCollision(9, 9);

        //initialise armour rating
        armour = 0;

        //Set up health and rank position so it doesnt jump on first movement
        clientHealthBar.InitialiseHealthbar();

        //get the number of players currently in game
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            //assign this instances player count for identification in other functions
            playerNo++;
        }

        clientWeaponManager.InitialiseWeapons();
        //currentWeapon = 0;
        velocity = 0.3f;
        movementSpeed = 0.3f;

        player.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
        anim.enabled = false;

    }

    public void Hit(int damage)
    {
        CmdTakeDamage(damage);
    }

    [Command]
    public void CmdTakeDamage(int damage)
    {
        TakeDamage(damage);
        RpcTakeDamage(damage);
    }

    [ClientRpc]
    public void RpcTakeDamage(int damage)
    {
        if (isLocalPlayer)
        {
            if (!isDead)
            {
                health -= damage;
                clientHealthBar.healthPercentage = (float)health / (float)totalHealth;

                if (health <= 0)
                {
                    isDead = true;
                    SetHealth(0);
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isLocalPlayer)
        {
            if (!isDead)
            {
                health -= damage;
                clientHealthBar.healthPercentage = (float)health / (float)totalHealth;

                if (health <= 0)
                {
                    isDead = true;
                    SetHealth(0);
                }
            }
        }
    }

    public void Update()
    {

        if (spawnPoints.Length == 0)
        {
            if(GameObject.FindGameObjectsWithTag("SpawnPoint1").Length > 0 && team == 0)
            {
                spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint1");
                Respawn();
                return;
            }

            if (GameObject.FindGameObjectsWithTag("SpawnPoint2").Length > 0 && team == 1)
            {
                spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint2");
                Respawn();
                return;
            }
        }

        //Check for victory by updating the scoreboard
        clientScoreBoard.UpdateScores();
        clientScoreBoard.CheckVictory();

        if (hasWon == true || hasLost == true)
            clientScoreBoard.scoreBoardActive = true;

        //Ignore collisions from players running into eachother
        Physics.IgnoreLayerCollision(9, 9);

        //Check for life
        if (isDead)
        {
            Death();
        }

        if (hasLost == true || hasWon == true)
        {
            //Make sure respawn screen is deactivated
            respawnScreen.SetActive(false);

            //HealthHUD
            clientHUD.healthBar.SetActive(false);
            clientHUD.healthText.enabled = false;

            //ammo score and rank icon
            clientHUD.ammoText.enabled = false;
            clientHUD.scoreText.enabled = false;
            clientHUD.rankImage.SetActive(false);

            clientScoreBoard.gameObject.SetActive(true);
            return;
        }

        if (isDead) return;

        //Update rank
        if (score <= 49) rank = 0;
        else if (score > 49 && score <= 99) rank = 1;
        else if (score > 100 && score <= 149) rank = 2;
        else if (score > 150 && score <= 199) rank = 3;
        else if (score > 200 && score <= 249) rank = 4;
        else if (score > 250 && score <= 299) rank = 5;
        else if (score > 300 && score <= 349) rank = 6;
        else if (score > 350 && score <= 399) rank = 7;
        else if (score > 400 && score <= 449) rank = 8;
        else if (score > 450 && score <= 499) rank = 9;

        //Toggle scoreboard
        if (Input.GetKeyDown("t") && playerName != "")
        {
            if (!isDead && isLocalPlayer)
            {
                clientScoreBoard.ToggleScoreBoard();
            }
        }

        //Initialisation for the camera
        if (playerCam.GetComponent<CameraMovement>().canMove == true)
        {
            playerCam.GetComponent<CameraMovement>().canMove = false;
        }

    }

    private void OnDisable()
    {
        if (lobbyCam)
            lobbyCam.gameObject.SetActive(true);
    }

    void SetLiving(bool b)
    {
        isDead = b;
    }

    public void OnRespawnClicked()
    {
        CmdRespawn();
        clientHealthBar.CmdRespawn();
    }

    [Command]
    public void CmdRespawn()
    {
        Respawn();
        RpcRespawn();
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        if (!isServer)
        {

            //Spawn in random position
            int rand = Random.Range(0, spawnPoints.Length);
            Debug.Log(rand);
            player.transform.position = spawnPoints[rand].transform.position;
            playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);

            //Reset health and ranks
            health = 100;
            clientHealthBar.healthPercentage = 1;
            rank = 0;
            score = 0;

            //Make visible after in position 
            SkinnedMeshRenderer[] r = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in r)
            {
                smr.enabled = true;
            }

            //Let the game loop re-commence
            isDead = false;

            //Stand the player back up
            player.transform.rotation *= Quaternion.Euler(-90, 0, 0);

            //Hide respawn button
            respawnScreen.SetActive(false);
            reset = false;
        }
    }

    public void Respawn()
    {
        if (isServer)
        {

            //Spawn in random position
            int rand = Random.Range(0, spawnPoints.Length);
            Debug.Log(rand);
            player.transform.position = spawnPoints[rand].transform.position;
            playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);

            //Reset health and ranks
            health = 100;
            clientHealthBar.healthPercentage = 1;
            rank = 0;
            score = 0;

            //Make visible after in position 
            SkinnedMeshRenderer[] r = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in r)
            {
                smr.enabled = true;
            }

            //Let the game loop re-commence
            isDead = false;

            //Stand the player back up
            player.transform.rotation *= Quaternion.Euler(-90, 0, 0);

            //disable colliders
            player.GetComponent<Rigidbody>().detectCollisions = true;
            player.GetComponent<BoxCollider>().enabled = true;

            //Hide respawn button
            respawnScreen.SetActive(false);
            reset = false;
        }
    }

    public void exitGame()
    {
        NetworkManager.singleton.StopClient();
    }


    private void FixedUpdate()
    {
        if (hasLost || hasWon || isPaused)
            return;

        if (isDead)
        {
            clientScoreBoard.DeactivateScoreBoard();
            return;
        }

        //Set all players to the correct z plane
        Vector3 p = player.transform.position;
        p.z = -10;
        player.transform.position = p;

        //first do crosshair position
        crosshair.transform.position = Input.mousePosition;

        Ray cameraRay = playerCam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, Vector3.zero);
        float rayLength;

        //get mouse position and apply transformation on z-coordinate to make it level with the player
        Vector3 mPos = Input.mousePosition;
        mPos.z = 140.0f;

        //get its position in world space
        mPos = playerCam.ScreenToWorldPoint(mPos);

        currentMPos = mPos;

        if (isLocal)
        {
            if (ground.Raycast(cameraRay, out rayLength))
            {
                Vector3 target = cameraRay.GetPoint(rayLength);
                Vector3 direction = target - player.transform.position;
                currentDirection = direction;
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                player.transform.rotation = Quaternion.Euler(0, 0, -rotation);

            }


            if (Input.GetKey("w"))
            {
                //apply the move toward function using this position
                player.transform.position = Vector3.MoveTowards(player.transform.position, new Vector3(currentMPos.x, currentMPos.y, -10), velocity);
                clientHealthBar.UpdatePosition();
                playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);
                anim.enabled = true;
            }
            else
            {
                anim.enabled = false;
            }
        }
        crosshair.transform.position = playerCam.WorldToScreenPoint(crosshairMarker.transform.position) + currentDirection.normalized * 200;

        body.velocity = new Vector3(0, 0, 0);
        body.angularVelocity = new Vector3(0, 0, 0);

    }

    public void Death()
    {
        if (!reset)
        {
            //Player falls over
            SkinnedMeshRenderer[] r = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in r)
            {
                smr.enabled = false;
            }

            //Spawn in random position
            int rand = Random.Range(0, spawnPoints.Length);
            player.transform.position = spawnPoints[rand].transform.position;
            playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);

            //disable colliders
            player.GetComponent<Rigidbody>().detectCollisions = false;
            player.GetComponent<BoxCollider>().enabled = false;

            //Respawn screen shows
            respawnScreen.SetActive(true);
            if (isLocalPlayer)
                CmdSetDeath();

            reset = true;

        }
    }

    public void SetHealth(int damage)
    {
        if (damage == 0)
            health = 0;
        else
            health -= damage;
    }

    public void UpdateScore(int s)
    {
        CmdGainScore(s);
    }

    public void UpdateKills(int k)
    {
        CmdUpdateKills(k);
    }

    public void GainScore(int s)
    {
        if (isServer)
        {
            score += s;

            int tmpRank = rank;

            if (score <= 49) rank = 0;
            else if (score > 49 && score <= 99) rank = 1;
            else if (score > 100 && score <= 149) rank = 2;
            else if (score > 150 && score <= 199) rank = 3;
            else if (score > 200 && score <= 249) rank = 4;
            else if (score > 250 && score <= 299) rank = 5;
            else if (score > 300 && score <= 349) rank = 6;
            else if (score > 350 && score <= 399) rank = 7;
            else if (score > 400 && score <= 449) rank = 8;
            else if (score > 450 && score <= 499) rank = 9;

            //Player has levelled up
            if (tmpRank != rank)
            {
                health = health = clientHUD.rankHealthValues[rank];
                totalHealth = clientHUD.rankHealthValues[rank];
                CmdTakeDamage(0);
            }



        }
    }

    [Command]
    public void CmdGainScore(int s)
    {
        GainScore(s);
        RpcGainScore(s);
    }

    [Command]
    public void CmdUpdateKills(int k)
    {
        RpcAddKills(k);
    }

    [ClientRpc]
    public void RpcGainScore(int s)
    {
        if (!isServer)
        {
            score += s;


            int tmpRank = rank;

            if (score <= 49) rank = 0;
            else if (score > 49 && score <= 99) rank = 1;
            else if (score > 100 && score <= 149) rank = 2;
            else if (score > 150 && score <= 199) rank = 3;
            else if (score > 200 && score <= 249) rank = 4;
            else if (score > 250 && score <= 299) rank = 5;
            else if (score > 300 && score <= 349) rank = 6;
            else if (score > 350 && score <= 399) rank = 7;
            else if (score > 400 && score <= 449) rank = 8;
            else if (score > 450 && score <= 499) rank = 9;

            //Player has levelled up, replenish back to full health
            if (tmpRank != rank)
            {
                health = health = clientHUD.rankHealthValues[rank];
                totalHealth = clientHUD.rankHealthValues[rank];
                CmdTakeDamage(0);
            }

        }
    }

    public int GetPlayerNo()
    {
        return playerNo;
    }

    [ClientRpc]
    public void RpcSetTeam(int t)
    {
        team = t;

        //Set the texture of the player
        if (team == 0)
            characterModel.GetComponent<SkinnedMeshRenderer>().material = RedTeamMaterial;
        else
            characterModel.GetComponent<SkinnedMeshRenderer>().material = BlueTeamMaterial;
    }

    public void SetTeam(int t)
    {
        team = t;

        //Set the texture of the player
        if (team == 0)
            characterModel.GetComponent<SkinnedMeshRenderer>().material = RedTeamMaterial;
        else
            characterModel.GetComponent<SkinnedMeshRenderer>().material = BlueTeamMaterial;
    }

    [Command]
    public void CmdSetTeam(int t)
    {
        SetTeam(t);
        RpcSetTeam(t);
    }

    [Command]
    public void CmdSetDeath()
    {
        RpcUpdateDeaths();
    }

    [ClientRpc]
    public void RpcUpdateDeaths()
    {
        deaths++;
    }

    [ClientRpc]
    public void RpcAddKills(int k)
    {
        kills += k;
    }

}



