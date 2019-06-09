using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyAI : MonoBehaviour {

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
    int[] reloadTimer = { 2, 5, 4, 3, 3 };

    //HUD stuff
    int score = 230;
    int health = 150;
    int rank = 3;

    //HUD objects
    public TextMeshPro healthText;

    public GameObject healthBar;
    public GameObject rankImage;

    public Sprite[] rankIcons;
    public int[] rankHealthValues;

    //reloading and timer
    bool isReloading = false;
    bool initialReload = true;
    float reloadStartTime = 0.0f;
    float reloadTargetTime = 0.0f;

    // Use this for initialization
    void Start()
    {

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
    }

    // Update is called once per frame
    void Update()
    {

        //Update the HUD
        //Health
        healthBar.GetComponent<Slider>().maxValue = rankHealthValues[rank];
        healthBar.GetComponent<Slider>().value = health;
        healthText.text = health.ToString() + "/" + rankHealthValues[rank];

        //Rank
        rankImage.GetComponent<Image>().sprite = rankIcons[rank];

        //Initialisation for the camera
        if (playerCam.GetComponent<CameraMovement>().canMove == true)
        {
            playerCam.GetComponent<CameraMovement>().canMove = false;
        }

        //Ignore player bullet collisions for now
        Physics.IgnoreLayerCollision(9, 10);

        //Weapon switching
        if (Input.GetKey("b")) SetWeapon(0);
        if (Input.GetKey("b")) SetWeapon(1);
        if (Input.GetKey("b")) SetWeapon(2);
        if (Input.GetKey("b")) SetWeapon(3);
        if (Input.GetKey("b")) SetWeapon(4);

        //Cancel reload if reloading mid-weapon switch
        if (Input.GetKey("b") || Input.GetKey("b") || Input.GetKey("b") || Input.GetKey("b") || Input.GetKey("b"))
        {
            isReloading = false;
            initialReload = true;
        }

        //Shooting
        if (Input.GetMouseButtonDown(0) && currentAmmo[currentWeapon] > 0)
        {
            GameObject b = Instantiate(bullet, crosshairMarker.transform.position, Quaternion.identity * Quaternion.Euler(new Vector3(-90, 0, 0)));
            muzzleFlashes[currentWeapon].SetActive(true);
            --currentAmmo[currentWeapon];
        }
        else
        {
            muzzleFlashes[currentWeapon].SetActive(false);
        }

        //Reloading
        if (Input.GetKey("b") && initialReload == true || currentAmmo[currentWeapon] == 0 && initialReload == true || Input.GetKey("r") && isReloading == false)
        {
            reloadStartTime = Time.time;
            reloadTargetTime = reloadStartTime + reloadTimer[currentWeapon];
            isReloading = true;
            initialReload = false;
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
        mPos = Camera.main.ScreenToWorldPoint(mPos);

        if (ground.Raycast(cameraRay, out rayLength))
        {
            Vector3 target = cameraRay.GetPoint(rayLength);
            Vector3 direction = target - transform.position;
            currentDirection = direction;
            float rotation = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, -rotation);

        }

        if (Input.GetKey("b"))
        {
            //apply the move toward function using this position             //was mpos
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(mPos.x, mPos.y, -10), velocity);
            playerCam.transform.position = new Vector3(transform.position.x, transform.position.y, playerCam.transform.position.z);
            Debug.Log(transform.position);
            anim.enabled = true;
        }
        else
        {
            anim.enabled = false;
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

        for (int i = 0; i < guns.Length; i++)
        {
            if (i == type)
                guns[i].SetActive(true);
            else
                guns[i].SetActive(false);
        }
    }

}
