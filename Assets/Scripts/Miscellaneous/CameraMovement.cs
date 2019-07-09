using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    //variable limits for camera movement
    private float panSpeed = 20.0f;
    private float scrollRate = 20.0f;
    public Vector2 panLimit;
    public Vector2 zoomLimit;

    //Effectively a pause controller
    public bool canMove;

    //For access from button clicks
    public void SetMovement(bool m)
    {
        canMove = m;
    }

    void Update()
    {
        if (canMove)
        {
            //Put position and rotation into temporary vectors for modification
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            //Directional movement
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

            //Check if camera has pan limits, and if so apply them
            if (panLimit.x != 0 && panLimit.y != 0)
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

            //Get zoom level via scrollwheel
            float scrollButton = Input.GetAxis("Mouse ScrollWheel");
            pos.z += scrollButton * scrollRate * 100.0f * Time.deltaTime;

            //Check if a zoom limit has been assigned and apply it
            if (zoomLimit.x != 0 && zoomLimit.y != 0)
            {
                if (pos.z > zoomLimit.x)
                    pos.z = zoomLimit.x;

                if (pos.z < zoomLimit.y)
                    pos.z = zoomLimit.y;
            }

            //Apply the changes on the camera
            transform.position = pos;
            transform.rotation = rot;
        }
    }
}
