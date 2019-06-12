using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

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
    int health = 150;
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
    public Button respawn;

    //To record kills and deaths in-game
    public int kills = 0;
    public int deaths = 0;

    //Recording whether character is currently alive
    public bool isDead = false;

    // Use this for initialization
    void Start () {

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
    
       for(int i = 0; i < muzzleFlashes.Length; i++)
       muzzleFlashes[i].SetActive(false);
    }

    // Update is called once per frame
    void Update () {


        //Check for life
        if(isDead)
        {
            Death();
            return;
        }

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

        //Initialisation for the camera
        if(playerCam.GetComponent<CameraMovement>().canMove == true)
        {
            playerCam.GetComponent<CameraMovement>().canMove = false;
        }

        //Reload sequence
        if(isReloading)
        {
            if(reloadStartTime >= reloadTargetTime)
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
        if (Input.GetMouseButtonDown(0) && GetCurrentAmmo(GetCurrentWeapon()) > 0)
        {
            SetCurrentAmmo(GetCurrentWeapon());
        }

        //Reloading
        if (Input.GetKey("r") && initialReload == true || GetCurrentAmmo(GetCurrentWeapon()) == 0 && initialReload == true || Input.GetKey("r") && isReloading == false)
        {
            reloadStartTime = Time.time;
            reloadTargetTime = reloadStartTime + reloadTimer[GetCurrentWeapon()];
            isReloading = true;
            initialReload = false;
        }

    }

    private void FixedUpdate()
    {

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
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentMPos.x, currentMPos.y, -10), GetVelocity());
                floatingHealthBar.transform.position = new Vector3(transform.position.x, transform.position.y + 7.5f, transform.position.z);
                floatingRankIcon.transform.position = new Vector3(transform.position.x -5.8f, transform.position.y + 7.75f, transform.position.z);
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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("registered hit");
        
        //Check it is a bullet
        if (collision.gameObject.tag == "bullet")
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();

            //check its an enemy players collision
            if (bullet.shooter != GetInstanceID())
            {
                //Take damage
                TakeDamage(bullet.damageAmount);

                //Tell the network about the damage taken
                this.transform.parent.GetComponent<ClientSetup>().CmdRegisterDamage(bullet.shooter);

            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage + armour;

        if(health <= 0)
        {
            isDead = true;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
        }
    }

    public void Death()
    {
        if (transform.rotation.eulerAngles.x < 90)
        {
            Quaternion tempRot = transform.rotation;
            tempRot *= Quaternion.Euler(Time.deltaTime, 0, 0);
            transform.rotation = tempRot;
        }
        else
        {

            Color tmp = transform.GetComponentInChildren<SkinnedMeshRenderer>().material.color;
            tmp.a -= Time.deltaTime / 4;
            transform.GetComponentInChildren<SkinnedMeshRenderer>().material.color = tmp;

            if (tmp.a <= 0.0f)
                respawn.gameObject.SetActive(true);
        }
    }

    public void Respawn()
    {
        //Reset health and ranks

        //Spawn in random position

        //Hide respawn button

    }

    public Vector3 getDirection()
    {
        return currentDirection;
    }

    public void setHealth(int damage)
    {
        health -= damage;
    }

    public int GetPlayerNo()
    {
        return playerNo;
    }

    public float GetVelocity()
    {
        return velocity;
    }

    public int GetCurrentAmmo(int i)
    {
        return currentAmmo[i];
    }

    public void SetCurrentAmmo(int i)
    {
        currentAmmo[i]--;
    }

    public int GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public void SetCurrentWeapon(int n)
    {
        currentWeapon = n;
    }

    public GameObject GetBullet()
    {
        return bullet;
    }

    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return totalHealth;
    }
}
