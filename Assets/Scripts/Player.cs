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

    //weapon ammunition
    const int fullPistolClip = 16;
    const int fullShotgunClip = 10;
    const int fullRifleClip = 30;
    const int fullSMGClip = 50;
    const int fullSniperClip = 1;

    //reload timers (in seconds)
    const int PistolReload = 2;
    const int ShotgunReload = 5;
    const int RifleReload = 4;
    const int SMGReload = 3;
    const int SniperReload = 3;

    //HUD stuff
    int score = 230;
    int ammo = 16;
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

    // Use this for initialization
    void Start () {

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
        ammoText.text = ammo.ToString() + "/" + "";

        //Rank
        rankImage.GetComponent<Image>().sprite = rankIcons[rank];

        //Initialisation for the camera
        if(playerCam.GetComponent<CameraMovement>().canMove == true)
        {
            playerCam.GetComponent<CameraMovement>().canMove = false;
        }

        //Ignore player bullet collisions for now
        Physics.IgnoreLayerCollision(9,10);

        //Weapon switching
        if (Input.GetKey("1")) SetWeapon(0);
        if (Input.GetKey("2")) SetWeapon(1);
        if (Input.GetKey("3")) SetWeapon(2);
        if (Input.GetKey("4")) SetWeapon(3);
        if (Input.GetKey("5")) SetWeapon(4);

        //Shooting
        if(Input.GetMouseButtonDown(0))
        {
            GameObject b = Instantiate(bullet, crosshairMarker.transform.position, Quaternion.identity * Quaternion.Euler(new Vector3(-90,0,0)));
            b.GetComponent<Bullet>().isTemplate = false;
            muzzleFlashes[currentWeapon].SetActive(true);
        }
        else
        {
            muzzleFlashes[currentWeapon].SetActive(false);
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

        if (Input.GetKey("w"))
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

}
