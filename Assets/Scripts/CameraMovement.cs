using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    private float panSpeed = 20.0f;
    private float scrollRate = 20.0f;
    public Vector2 panLimit;

    public bool canMove;

	// Use this for initialization
	void Start () {
        canMove = true;
	}
	
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
        }
        float scrollButton = Input.GetAxis("Mouse ScrollWheel");

        pos.z += scrollButton * scrollRate * 100.0f * Time.deltaTime;

        transform.position = pos;
        transform.rotation = rot;
    }
}
