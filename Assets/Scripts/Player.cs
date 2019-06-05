using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour {

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

    //weapon clips
    int[] clipSize = { 16, 10, 30, 50, 1 };
    int[] currentAmmo = { 16, 10, 30, 50, 1 };

    //reload timers (in seconds)
    public int[] reloadTimer = { 2, 5, 4, 3, 3 };

    //HUD stuff
    int score = 230;
    int health = 150;
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

    //for interaction with the controller script
    public Vector3 currentMPos;

    // Use this for initialization
    void Start () {


        //get the rigidbody
        body = GetComponent<Rigidbody>();

        if(body)
        { Debug.Log("great success"); }

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

        //Weapon switching
        if (Input.GetKey("1")) SetWeapon(0);
        if (Input.GetKey("2")) SetWeapon(1);
        if (Input.GetKey("3")) SetWeapon(2);
        if (Input.GetKey("4")) SetWeapon(3);
        if (Input.GetKey("5")) SetWeapon(4);

        //Cancel reload if reloading mid-weapon switch
        if (Input.GetKey("1") || Input.GetKey("2") || Input.GetKey("3") || Input.GetKey("4") || Input.GetKey("5"))
        {
            isReloading = false;
            initialReload = true;
        }

        //Shooting
        if (Input.GetMouseButtonDown(0) && GetCurrentAmmo(GetCurrentWeapon()) > 0)
        {
            //CmdSpawnBullet();
            muzzleFlashes[GetCurrentWeapon()].SetActive(true);
            SetCurrentAmmo(GetCurrentWeapon());
        }
        else
        {
            muzzleFlashes[GetCurrentWeapon()].SetActive(false);
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
            Debug.Log("update");
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
                //apply the move toward function using this position             //was mpos
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(currentMPos.x, currentMPos.y, -10), GetVelocity());
                playerCam.transform.position = new Vector3(transform.position.x, transform.position.y, playerCam.transform.position.z);
                Debug.Log(transform.position);
                anim.enabled = true;
            }
            else
            {
                anim.enabled = false;
            }
        }
        crosshair.transform.position = playerCam.WorldToScreenPoint(crosshairMarker.transform.position) + currentDirection.normalized * 200;//transform.position;

        body.velocity = new Vector3(0, 0, 0);
        body.angularVelocity = new Vector3(0, 0, 0);// movementSpeed * currentDirection;

    }

    public Vector3 getDirection()
    {
        return currentDirection;
    }

    public void setHealth(int damage)
    {
        health -= damage;
    }

    public void SetWeapon(int type)
    {
        currentWeapon = type;

        for(int i = 0; i < guns.Length; i++)
        {
            if (i == type)
                guns[i].SetActive(true);
            else
                guns[i].SetActive(false);
        }
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

    public GameObject GetBullet()
    {
        return bullet;
    }
}
