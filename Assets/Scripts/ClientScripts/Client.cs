using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Client : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] clientComponents;

    Camera sceneCam;

    public GameObject player;

    public bool isLocal;

    public List<GameObject> spawnPoints;

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
    Animator weaponAnim;
    public Animator anim;
    public Rigidbody body;

    //Weapons
    public int currentWeapon = 0;

    public GameObject[] guns;

    public GameObject[] muzzleFlashes;
    public GameObject bullet;

    //Armour
    public int[] armourRatings = { 0, 1, 2, 3, 5, 7, 8, 10, 15 };
    public int armour;

    //weapon clips
    public int[] clipSize = { 16, 10, 30, 50, 1 };
    public int[] currentAmmo = { 16, 10, 30, 50, 1 };

    //weapon damage and fire rates
    public int[] damageAmounts = { 12, 15, 8, 6, 40 };

    public float[] fireRates = { 0.75f, 1.8f, 0.25f, 0.35f, 2.0f };
    public float[] currentFireRates = { 0.75f, 1.8f, 0.25f, 0.35f, 2.0f };

    public int currentFired = 0;
    public bool hasFired = false;

    //reload timers (in seconds)
    public int[] reloadTimer = { 2, 5, 4, 3, 3 };

    //HUD stuff
    public int score = 0;
    //[SyncVar(hook = "ChangeHealth")] public float healthPercentage = 1;
    //[SyncVar(hook = "ChangeHealthColour")] public Color healthColour = Color.green;
    public int health = 100;
    int totalHealth = 100;


    //HUD objects
    //public TextMeshProUGUI scoreText;
    //public TextMeshProUGUI ammoText;
    //public TextMeshProUGUI healthText;

    //public GameObject healthBar;
    //public GameObject rankImage;

    //public Sprite[] rankIcons;
    //public int[] rankHealthValues;
    public int rank = 0;

    //Reloading and timer
    public bool isReloading = false;
    public bool initialReload = true;
    public float reloadStartTime = 0.0f;
    public float reloadTargetTime = 0.0f;

    //For multiplayer
    int playerNo;

    //For interaction with the controller script
    public Vector3 currentMPos;

    ////For manipulating the health bar
    //public GameObject floatingHealthBar;
    //public GameObject floatingRankIcon;

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

    //For muzzle flash timing
    float muzzleFlashTimer = 0.15f;
    bool muzzleShot = false;

    //For win conditions
    public bool hasWon = false;

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

    // Use this for initialization
    void Start()
    {

        sceneCam = Camera.main;

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
            sceneCam.gameObject.SetActive(false);

            //Hide UI elements that player shouldn't see
            //floatingHealthBar.GetComponent<CanvasRenderer>().SetAlpha(0);
            //floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
    }


    #region Name Setting region
    [ClientRpc]
    public void RpcSetName(string n)
    {
        playerName = n;
    }

    public void SetName(string n)
    {
        playerName = n;
    }

    [Command]
    public void CmdSetName(string n)
    {
        SetName(n);
        RpcSetName(n);
    }

    // Called from the naming menu
    public void InitialisePlayerName(string n)
    {
        CmdSetName(n);
    }
    #endregion

    public void InitialisePlayer()
    {
        //ignore collisions between players
        Physics.IgnoreLayerCollision(9, 9);

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
        if (team == 1)
            spawnPoints = GameObject.Find("MapManager").GetComponent<GameMap>().team1Spawns;
        else
            spawnPoints = GameObject.Find("MapManager").GetComponent<GameMap>().team2Spawns;

        //set up spawnpoint
        Respawn();

        //initialise armour rating
        armour = 0;

        ////Set up health and rank position so it doesnt jump on first movement
        clientHealthBar.InitialiseHealthbar();
        //floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
        //floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);

        //get the number of players currently in game
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            //assign this instances player count for identification in other functions
            playerNo++;
        }

        currentWeapon = 0;
        velocity = 0.3f;
        movementSpeed = 0.3f;

        player.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
        anim.enabled = false;
        guns[currentWeapon].transform.position = player.transform.position;
        guns[currentWeapon].transform.rotation = player.transform.rotation;

        weaponAnim = guns[currentWeapon].GetComponent<Animator>();
        weaponAnim.enabled = false;

        for (int i = 0; i < muzzleFlashes.Length; i++)
            muzzleFlashes[i].SetActive(false);

    }

    [Command]
    void CmdSetWeapon(int type)
    {
        SetWeapon(type);
        RpcSetWeapon(type);
    }

    [ClientRpc]
    public void RpcSetWeapon(int type)
    {
        currentWeapon = type;

        for (int i = 0; i < guns.Length; i++)
        {
            if (i == type)
                guns[i].SetActive(true);
            else
                guns[i].SetActive(false);
        }
    }

    public void SetWeapon(int type)
    {
        currentWeapon = type;

        for (int i = 0; i < guns.Length; i++)
        {
            if (i == type)
                guns[i].SetActive(true);
            else
                guns[i].SetActive(false);
        }
    }

    //[Command]
    //public void CmdUpdateHealth()
    //{
    //    UpdateHealth();
    //    RpcUpdateHealth();
    //}

    //public void UpdateHealth()
    //{
    //    //Set colour
    //    if (healthPercentage > 0.7f)
    //        floatingHealthBar.GetComponent<Image>().color = Color.green;
    //    else
    //    if (healthPercentage <= 0.7f && healthPercentage > 0.25f)
    //        floatingHealthBar.GetComponent<Image>().color = Color.yellow;
    //    else
    //        floatingHealthBar.GetComponent<Image>().color = Color.red;

    //    //update rank image also
    //    floatingRankIcon.GetComponent<Image>().sprite = clientHUD.rankIcons[rank];

    //    //And finally set it's position
    //    floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
    //    floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);
    //}

    //[ClientRpc]
    //public void RpcUpdateHealth()
    //{
    //    //update rank image
    //    floatingRankIcon.GetComponent<Image>().sprite = clientHUD.rankIcons[rank];

    //    //Set it's position
    //    floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
    //    floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);
    //}


    //void ChangeHealth(float h)
    //{
    //    //Set the Fill Amount
    //    floatingHealthBar.GetComponent<Image>().fillAmount = h;

    //    //Set colour
    //    if (h > 0.7f)
    //        healthColour = Color.green;
    //    else
    //    if (h <= 0.7f && h > 0.25f)
    //        healthColour = Color.yellow;
    //    else
    //        healthColour = Color.red;

    //}

    //void ChangeHealthColour(Color c)
    //{
    //    floatingHealthBar.GetComponent<Image>().color = c;
    //}

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
        //Ignore collisions from players running into eachother
        Physics.IgnoreLayerCollision(9, 9);

        ////Update the HUD
        ////Health
        //healthBar.GetComponent<Slider>().maxValue = rankHealthValues[rank];
        //healthBar.GetComponent<Slider>().value = health;
        //healthText.text = health.ToString() + "/" + rankHealthValues[rank];

        ////Score
        //scoreText.text = score.ToString();

        ////Ammo
        //ammoText.text = currentAmmo[currentWeapon].ToString() + "/" + clipSize[currentWeapon];

        ////Rank
        //rankImage.GetComponent<Image>().sprite = rankIcons[rank];

        //Check for life
        if (isDead)
        {
            Death();
            return;
        }

        //CmdUpdateHealth();

        if (Input.GetMouseButton(0) && currentAmmo[currentWeapon] > 0 && isLocalPlayer && fireRates[currentWeapon] == currentFireRates[currentWeapon])
        {
            CmdSpawnBullet();
            currentAmmo[currentWeapon]--;
            currentFired = currentWeapon;
            muzzleShot = true;
            hasFired = true;
        }

        if (hasFired)
        {
            if (currentFireRates[currentFired] > 0)
            {
                currentFireRates[currentFired] -= Time.deltaTime;
            }
            else
            {
                currentFireRates[currentFired] = fireRates[currentFired];
                hasFired = false;
            }

        }

        if (muzzleShot)
        {

            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0.0f)
            {
                CmdRemoveFlash();
                muzzleFlashTimer = 0.15f;
                muzzleShot = false;
            }
        }

        ////Weapon switching
        if (Input.GetKey("1") && isLocalPlayer) CmdSetWeapon(0);
        if (Input.GetKey("2") && isLocalPlayer && score > 100) CmdSetWeapon(1);
        if (Input.GetKey("3") && isLocalPlayer && score > 150) CmdSetWeapon(2);
        if (Input.GetKey("4") && isLocalPlayer && score > 200) CmdSetWeapon(3);
        if (Input.GetKey("5") && isLocalPlayer && score > 250) CmdSetWeapon(4);

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

        //Reload sequence
        if (isReloading)
        {
            if (reloadStartTime >= reloadTargetTime)
            {
                currentAmmo[currentWeapon] = clipSize[currentWeapon];
                reloadStartTime = 0.0f;
                isReloading = false;
                initialReload = true;
            }
            else
            {
                reloadStartTime = Time.time;
            }
        }

        //Cancel reload if reloading mid-weapon switch
        if (Input.GetKey("1") || Input.GetKey("2") || Input.GetKey("3") || Input.GetKey("4") || Input.GetKey("5"))
        {
            isReloading = false;
            initialReload = true;
        }

        //Reloading
        if (Input.GetKey("r") && initialReload == true || currentAmmo[currentWeapon] == 0 && initialReload == true || Input.GetKey("r") && isReloading == false)
        {
            reloadStartTime = Time.time;
            reloadTargetTime = reloadStartTime + reloadTimer[currentWeapon];
            isReloading = true;
            initialReload = false;
        }

    }

    [Command]
    public void CmdSpawnBullet()
    {
        GameObject b = (GameObject)Instantiate(bullet, new Vector3(crosshairMarker.transform.position.x, crosshairMarker.transform.position.y, -4.5f), Quaternion.identity);

        //calculate rotation
        Quaternion rot = b.transform.rotation;
        rot = player.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        b.transform.rotation = rot;

        //calculate trajectory
        b.GetComponent<Rigidbody>().velocity = b.transform.forward * 6.0f;

        //add tag indicating whose bullet it is
        b.GetComponent<Bullet>().shooter = player;
        b.GetComponent<Bullet>().isHost = isLocalPlayer;
        b.GetComponent<Bullet>().damageAmount = damageAmounts[currentWeapon];

        NetworkServer.Spawn(b);

        MuzzleFlash(true);
        RpcMuzzleFlash(true);
    }

    [Command]
    public void CmdRemoveFlash()
    {
        MuzzleFlash(false);
        RpcMuzzleFlash(false);
    }

    public void MuzzleFlash(bool istrue)
    {
        muzzleFlashes[currentWeapon].SetActive(istrue);
    }

    [ClientRpc]
    public void RpcMuzzleFlash(bool istrue)
    {
        muzzleFlashes[currentWeapon].SetActive(istrue);
    }

    private void OnDisable()
    {
        if (sceneCam)
            sceneCam.gameObject.SetActive(true);
    }

    void SetLiving(bool b)
    {
        isDead = b;
    }

    public void OnRespawnClicked()
    {
        CmdRespawn();
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
            //Reset health and ranks
            health = 100;
            clientHealthBar.healthPercentage = 1;
            rank = 0;
            score = 0;

            //floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            //healthColour = Color.green;

            //Make visible after in position 
            SkinnedMeshRenderer[] r = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in r)
            {
                smr.enabled = true;
            }

            //floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(255);

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
            //Reset health and ranks
            health = 100;
            clientHealthBar.healthPercentage = 1;
            rank = 0;
            score = 0;

          //  floatingHealthBar.GetComponent<Image>().fillAmount = 1;
           // healthColour = Color.green;

            //Make visible after in position 
            SkinnedMeshRenderer[] r = player.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer smr in r)
            {
                smr.enabled = true;
            }

            //floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(255);

            //Let the game loop re-commence
            isDead = false;

            //Stand the player back up
            player.transform.rotation *= Quaternion.Euler(-90, 0, 0);

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
               // floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
               // floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);
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

            clientHealthBar.floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(0);

            //Spawn in random position
            int rand = Random.Range(0, spawnPoints.Count);
            player.transform.position = spawnPoints[rand].transform.position;
            playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);

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
    }

    public void SetTeam(int t)
    {
        team = t;
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



