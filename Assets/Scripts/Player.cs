using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Camera playerCam;
    public Animator anim;

    public GameObject [] guns;

    public GameObject [] muzzleFlashes;
    public GameObject bullet;


    int currentWeapon;

    float velocity;
    Vector3 currentDirection;

    Animator weaponAnim;

    // Use this for initialization
    void Start () {

        currentWeapon = 0;
        velocity = 0.2f; 

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

        Ray cameraRay = playerCam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.forward, Vector3.zero);
        float rayLength;

        //get mouse position and apply transformation on z-coordinate to make it level with the player
        Vector3 mPos = Input.mousePosition;
        mPos.z = 190.0f;

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

        if (Input.GetKey("1")) SetWeapon(0);
        if (Input.GetKey("2")) SetWeapon(1);
        if (Input.GetKey("3")) SetWeapon(2);
        if (Input.GetKey("4")) SetWeapon(3);
        if (Input.GetKey("5")) SetWeapon(4);


        if (Input.GetKey("w"))
        {
            //apply the move toward function using this position
            transform.position = Vector3.MoveTowards(transform.position, mPos, velocity);
            anim.enabled = true;
        }
        else
        {
            anim.enabled = false;
        }


        if(Input.GetMouseButtonDown(0))
        {
            Instantiate(bullet, transform.position, Quaternion.identity * Quaternion.Euler(new Vector3(-90,0,0)));
            muzzleFlashes[currentWeapon].SetActive(true);
        }
        else
        {
            muzzleFlashes[currentWeapon].SetActive(false);
        }

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
