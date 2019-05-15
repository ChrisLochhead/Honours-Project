using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Camera playerCam;

	// Use this for initialization
	void Start () {
        transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, -10);
    }
	
	// Update is called once per frame
	void Update () {

        Ray cameraRay = playerCam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        //if(ground.Raycast(cameraRay, out rayLength))
        //{
        //    transform.LookAt(new Vector3(cameraRay.GetPoint(rayLength).x, transform.position.y, -10));
        //}
        Vector3 mPos = Input.mousePosition;
        mPos.z = 190.0f;

        mPos = Camera.main.ScreenToWorldPoint(mPos);

        if (Input.GetKey("w"))
        {

           // Vector3 movementTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);// playerCam.ScreenToWorldPoint(Input.mousePosition);
            transform.position = Vector3.MoveTowards(transform.position, mPos, 2);
           // Vector3 pos = transform.position;
            //pos.z = -10.0f;
           // transform.position = pos;
        }

        
		
	}
}
