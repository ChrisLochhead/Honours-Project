﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    private float panSpeed = 20.0f;
    private float scrollRate = 20.0f;
    public Vector2 panLimit;
    public Vector2 zoomLimit;

    public bool canMove;

    public void SetMovement(bool m)
    {
        canMove = m;
    }

	// Update is called once per frame
	void Update () {

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        if (canMove)
        {
            if (Input.GetKey("p"))
            {
                rot *= Quaternion.Euler(0.1f, 0, 0);
            }
            if (Input.GetKey("w"))
            {
               pos.y += panSpeed * Time.deltaTime;
            }
            if (Input.GetKey("s"))
            {
                pos.y -= panSpeed * Time.deltaTime;
            }
            if (Input.GetKey("d"))
            {
                pos.x += panSpeed * Time.deltaTime;
            }
            if (Input.GetKey("a"))
            {
                pos.x -= panSpeed * Time.deltaTime;
            }

            //Check if camera has pan limits
            if(panLimit.x != 0 && panLimit.y != 0)
            {
                if (pos.x > panLimit.x)
                    pos.x = panLimit.x;
                if (pos.x < -panLimit.x)
                    pos.x = -panLimit.x;

                if (pos.y > panLimit.y)
                    pos.y = panLimit.y;
                if (pos.y < -panLimit.y)
                    pos.y = -panLimit.y;
            }
        }
        float scrollButton = Input.GetAxis("Mouse ScrollWheel");

        
        pos.z += scrollButton * scrollRate * 100.0f * Time.deltaTime;

        //Check if a zoom limit has been assigned
        if(zoomLimit.x != 0 && zoomLimit.y != 0)
        {
            if (pos.z > zoomLimit.x)
                pos.z = zoomLimit.x;

            if (pos.z < zoomLimit.y)
                pos.z = zoomLimit.y;
        }

        transform.position = pos;
        transform.rotation = rot;
    }
}
