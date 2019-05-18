using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Camera playerCam;
    public Animator anim;
    public GameObject weapon;
    public GameObject muzzleFlash;
    public GameObject bullet;

    Vector3 currentDirection;

    Animator weaponAnim;

    // Use this for initialization
    void Start () {
        transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
        anim.enabled = false;
        weapon.transform.position = this.transform.position;
        weapon.transform.rotation = this.transform.rotation;

       weaponAnim = weapon.GetComponent<Animator>();
       weaponAnim.enabled = false;

        muzzleFlash.SetActive(false);
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

        if (Input.GetKey("w"))
        {
            //apply the move toward function using this position
            transform.position = Vector3.MoveTowards(transform.position, mPos, 0.2f);
            anim.enabled = true;
        }
        else
        {
            anim.enabled = false;
        }

        if(Input.GetMouseButtonDown(0))
        {
            Instantiate(bullet, transform.position, Quaternion.identity);
            muzzleFlash.SetActive(true);
        }
        else
        {
            muzzleFlash.SetActive(false);
        }

    }

    public Vector3 getDirection()
    {
        return currentDirection;
    }
}
