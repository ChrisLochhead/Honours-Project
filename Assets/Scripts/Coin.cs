using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour {

    public Vector2 pos;
    public int type;

    public int Points;
    public bool isActive = true;

    public float reactivationTime = 45.0f;

	// Use this for initialization
	void Start () {
		if(type == 0)
        {
            Points = 50;
        }else 
        if(type == 1)
        {
            Points = 30;
        }
        else
        {
            Points = 15;
        }
	}
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //check if a player is touching it
        if(collision.gameObject.transform.parent.GetComponent<Client>() && isActive == true)
        {
            collision.gameObject.transform.parent.GetComponent<Client>().score += Points;
            GetComponent<SphereCollider>().enabled = false;
            isActive = false;
        }
    }

    // Update is called once per frame
    void Update () {
        if (isActive == false)
        {
            GetComponent<MeshRenderer>().enabled = false;
            reactivationTime -= Time.deltaTime;

            if(reactivationTime <= 0)
            {
                reactivationTime = 45.0f;
                isActive = true;
                GetComponent<SphereCollider>().enabled = true;
            }
        }
        else
            GetComponent<MeshRenderer>().enabled = true;
    }
}
