using UnityEngine.Networking;
using UnityEngine;

public class Coin : NetworkBehaviour {

    //Record position and type
    public Vector2 pos;
    public int type;

    //The amount of points this coin gives when picked up
    public int Points;

    //Networked variables to check if and when the coin will spawn
    [SyncVar]
    public bool isActive = true;
    [SyncVar]
    public float reactivationTime = 45.0f;

	void Start () {

        //Initialise the points based on the coin type
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
            //Give the player points, disable the coins collider
            collision.gameObject.transform.parent.GetComponent<Client>().score += Points;
            GetComponent<SphereCollider>().enabled = false;
            isActive = false;
        }
    }

    [Command]
    public void CmdUpdateActivity()
    {
        UpdateActivity();
        RpcUpdateActivity();
    }

    public void UpdateActivity()
    {
        //If is not visible
        if (isActive == false)
        {
            //keep it hidden and iterate the time until it is supposed to respawn.
            GetComponent<MeshRenderer>().enabled = false;
            reactivationTime -= Time.deltaTime;

            //If it is now supposed to respawn, reset everything
            if (reactivationTime <= 0)
            {
                reactivationTime = 45.0f;
                isActive = true;
                GetComponent<SphereCollider>().enabled = true;
            }
        }
        else
            GetComponent<MeshRenderer>().enabled = true;
    }

    [ClientRpc]
    public void RpcUpdateActivity()
    {
        //If is not visible
        if (isActive == false)
        {
            //keep it hidden and iterate the time until it is supposed to respawn.
            GetComponent<MeshRenderer>().enabled = false;
            reactivationTime -= Time.deltaTime;

            //If it is now supposed to respawn, reset everything
            if (reactivationTime <= 0)
            {
                reactivationTime = 45.0f;
                isActive = true;
                GetComponent<SphereCollider>().enabled = true;
            }
        }
        else
            GetComponent<MeshRenderer>().enabled = true;
    }


    void Update () {

        if(GetComponent<MeshRenderer>())
        CmdUpdateActivity();

    }
}
