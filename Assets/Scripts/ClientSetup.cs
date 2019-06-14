using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class ClientSetup : NetworkBehaviour {


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
    Rigidbody body;

    //Weapons
    int currentWeapon;

    public GameObject[] guns;

    public GameObject[] muzzleFlashes;
    public GameObject bullet;

    //Armour
    public int[] armourRatings = { 0, 1, 2, 3, 5, 7, 8, 10, 15 };
    public int armour;

    //weapon clips
    int[] clipSize = { 16, 10, 30, 50, 1 };
    int[] currentAmmo = { 16, 10, 30, 50, 1 };

    //weapon damage
    public int[] damageAmounts = { 12, 15, 8, 6, 40 };

    //reload timers (in seconds)
    public int[] reloadTimer = { 2, 5, 4, 3, 3 };

    //HUD stuff
    int score = 230;
    [SyncVar(hook = "TakeDamage")]  public int health = 150;
    int totalHealth = 150;
    int rank = 3;

    //HUD objects
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI healthText;

    public GameObject healthBar;
    public GameObject rankImage;

    public Sprite[] rankIcons;
    public int[] rankHealthValues;

    //Reloading and timer
    public bool isReloading = false;
    public bool initialReload = true;
    public float reloadStartTime = 0.0f;
    public float reloadTargetTime = 0.0f;

    //For multiplayer
    int playerNo;

    //For interaction with the controller script
    public Vector3 currentMPos;

    //For manipulating the health bar
    public GameObject floatingHealthBar;
    public GameObject floatingRankIcon;

    //Respawn screen
    public GameObject respawnScreen;

    //To record kills and deaths in-game
    public int kills = 0;
    public int deaths = 0;

    //Recording whether character is currently alive
    public bool isDead = false;
    float deathRotation = 0;
    public Quaternion rotationAtDeath;

    //Registers which team the player is in
    public int team;

    //For maintaining score in the game
    public GameObject gameManager;

    [SerializeField]
    Behaviour[] clientComponents;

    Camera sceneCam;

    public GameObject player;

    public bool isLocal;

    public List<GameObject> spawnPoints;

	// Use this for initialization
	void Start () {

        //Join a random team
        team = Random.Range(1, 200);
        Respawn();

        //Notify the GameManager
        gameManager = GameObject.Find("GameManager");
        gameManager.GetComponent<Game>().addPlayer(this.gameObject);

        //initialise armour rating
        armour = 0;

        //Set up health and rank position so it doesnt jump on first movement
        floatingHealthBar.transform.position = new Vector3(transform.position.x, transform.position.y + 7.5f, transform.position.z);
        floatingRankIcon.transform.position = new Vector3(transform.position.x - 5.8f, transform.position.y + 7.75f, transform.position.z);

        //get the rigidbody
        body = GetComponent<Rigidbody>();

        //get the number of players currently in game
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            //assign this instances player count for identification in other functions
            playerNo++;
        }

        currentWeapon = 0;
        velocity = 0.3f;
        movementSpeed = 0.3f;

        body = GetComponent<Rigidbody>();

        transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
        anim.enabled = false;
        guns[currentWeapon].transform.position = this.transform.position;
        guns[currentWeapon].transform.rotation = this.transform.rotation;

        weaponAnim = guns[currentWeapon].GetComponent<Animator>();
        weaponAnim.enabled = false;

        for (int i = 0; i < muzzleFlashes.Length; i++)
            muzzleFlashes[i].SetActive(false);

        sceneCam = Camera.main;

        gameManager = GameObject.Find("MapManager");
        if(team == 1)
            spawnPoints = gameManager.GetComponent<GameMap>().team1Spawns;
        else
            spawnPoints = gameManager.GetComponent<GameMap>().team2Spawns;

        //set up spawnpoint
        int rand = Random.Range(0, spawnPoints.Count);
        player.transform.position = spawnPoints[rand].transform.position;

        if(!isLocalPlayer)
        {
            isLocal = false;
            //Disable all components unique to the client
            for(int i = 0; i < clientComponents.Length; i++)
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
            floatingHealthBar.GetComponent<CanvasRenderer>().SetAlpha(0);
            floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(0);
        }

       
    }

    [Command]
    void CmdSetWeapon(int type)
    {
        //currently both weapons change on server, changes properly on client
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

    [Command]
    public void CmdUpdateHealth()
    {
        UpdateHealth();
        RpcUpdateHealth();
    }

    public void UpdateHealth()
    {

        float currentHealthPercentage = (float)health / (float)totalHealth;

        //Set the Fill Amount
        floatingHealthBar.GetComponent<Image>().fillAmount = currentHealthPercentage;

        //Set colour
        if (currentHealthPercentage > 0.7f)
            floatingHealthBar.GetComponent<Image>().color = Color.green;
        else
        if (currentHealthPercentage <= 0.7f && currentHealthPercentage > 0.25f)
            floatingHealthBar.GetComponent<Image>().color = Color.yellow;
        else
            floatingHealthBar.GetComponent<Image>().color = Color.red;

        //And finally set it's position
        floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);

    }

    [ClientRpc]
    public void RpcUpdateHealth()
    {
        float currentHealthPercentage = (float)health / (float)totalHealth;

        //Set the Fill Amount
        floatingHealthBar.GetComponent<Image>().fillAmount = currentHealthPercentage;

        //Set colour
        if (currentHealthPercentage > 0.7f)
            floatingHealthBar.GetComponent<Image>().color = Color.green;
        else
        if (currentHealthPercentage <= 0.7f && currentHealthPercentage > 0.25f)
            floatingHealthBar.GetComponent<Image>().color = Color.yellow;
        else
            floatingHealthBar.GetComponent<Image>().color = Color.red;

        //And finally set it's position
        floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);

    }

    public void TakeDamage(int damage)
    {
        health -= damage + armour;

        if (health <= 0)
        {
            isDead = true;
            setHealth(0);
            rotationAtDeath = player.transform.rotation;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    public void Update()
    {
        //Update the HUD
        //Health
        healthBar.GetComponent<Slider>().maxValue = rankHealthValues[rank];
        healthBar.GetComponent<Slider>().value = health;
        healthText.text = health.ToString() + "/" + rankHealthValues[rank];

        //Score
        scoreText.text = score.ToString();

        //Ammo
        ammoText.text = currentAmmo[currentWeapon].ToString() + "/" + clipSize[currentWeapon];

        //Rank
        rankImage.GetComponent<Image>().sprite = rankIcons[rank];

        //Check for life
        if (isDead)
        {
            Death();
            return;
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

        //Shooting
        if (Input.GetMouseButtonDown(0) && currentAmmo[currentWeapon] > 0)
        {
            currentAmmo[currentWeapon]--;
        }

        //Reloading
        if (Input.GetKey("r") && initialReload == true || currentAmmo[currentWeapon] == 0 && initialReload == true || Input.GetKey("r") && isReloading == false)
        {
            reloadStartTime = Time.time;
            reloadTargetTime = reloadStartTime + reloadTimer[currentWeapon];
            isReloading = true;
            initialReload = false;
        }

        CmdUpdateHealth();

        if (Input.GetMouseButtonDown(0) && currentAmmo[currentWeapon] > 0 && isLocalPlayer)
        {
            CmdSpawnBullet();
            UpdateHealth();
        }
        else
        {
            CmdRemoveFlash();
        }

        //Weapon switching
        if (Input.GetKey("1") && isLocalPlayer) CmdSetWeapon(0);
        if (Input.GetKey("2") && isLocalPlayer) CmdSetWeapon(1);
        if (Input.GetKey("3") && isLocalPlayer) CmdSetWeapon(2);
        if (Input.GetKey("4") && isLocalPlayer) CmdSetWeapon(3);
        if (Input.GetKey("5") && isLocalPlayer) CmdSetWeapon(4);

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
        b.GetComponent<Bullet>().shooter = player.gameObject;
        b.GetComponent<Bullet>().damageAmount = damageAmounts[currentWeapon];

        NetworkServer.Spawn(b);

        MuzzleFlash(true);
        RpcMuzzleFlash(true);
    }

    [Command]
    public void CmdRegisterDamage(int shooterID)
    {

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

    public void Respawn()
    {
        //Reset health and ranks
        health = 100;
        rank = 0;

        //Spawn in random position
        int rand = Random.Range(0, spawnPoints.Count);
        player.transform.position = spawnPoints[rand].transform.position;
        playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, playerCam.transform.position.z);

        //Let the game loop re-commence
        isDead = false;

        //Stand the player back up
        transform.rotation *= Quaternion.Euler(-90, 0, 0);

        //Reactivate colliders
        gameObject.GetComponent<BoxCollider>().enabled = true;
        gameObject.GetComponent<Rigidbody>().detectCollisions = true;

        //Hide respawn button
        respawnScreen.SetActive(false);

    }

    public void exitGame()
    {
        NetworkManager.singleton.StopClient();
    }

    private void FixedUpdate()
    {

        if (isDead)
            return;

        Vector3 p = transform.position;
        p.z = -10;
        transform.position = p;
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

        if (transform.parent.gameObject.GetComponent<ClientSetup>().isLocal)
        {
            if (ground.Raycast(cameraRay, out rayLength))
            {
                Vector3 target = cameraRay.GetPoint(rayLength);
                Vector3 direction = target - transform.position;
                currentDirection = direction;
                float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, -rotation);

            }

            if (Input.GetKey("w"))
            {
                //apply the move toward function using this position
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentMPos.x, currentMPos.y, -10), velocity);
                floatingHealthBar.transform.position = new Vector3(transform.position.x, transform.position.y + 7.5f, transform.position.z);
                floatingRankIcon.transform.position = new Vector3(transform.position.x - 5.8f, transform.position.y + 7.75f, transform.position.z);
                playerCam.transform.position = new Vector3(transform.position.x, transform.position.y, playerCam.transform.position.z);
                anim.enabled = true;
            }
            else
            {
                anim.enabled = false;
            }
        }
        crosshair.transform.position = playerCam.WorldToScreenPoint(crosshairMarker.transform.position) + currentDirection.normalized * 200;

        body.velocity = new Vector3(0, 0, 0);
        body.angularVelocity = new Vector3(0, 0, 0);// movementSpeed * currentDirection;

    }

    public void Death()
    {
        //Player falls over
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(90, 0, 0) * rotationAtDeath, Time.deltaTime * 0.5f);

        //Respawn screen shows
        respawnScreen.SetActive(true);
    }


    public void setHealth(int damage)
    {
        if (damage == 0)
            health = 0;
        else
            health -= damage;
    }
}
