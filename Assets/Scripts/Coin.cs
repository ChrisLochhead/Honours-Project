using UnityEngine.Networking;
using UnityEngine;

public class Coin : NetworkBehaviour {

    public Vector2 pos;
    public int type;

    public int Points;

    [SyncVar]
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

    [Command]
    public void CmdUpdateActivity()
    {
        UpdateActivity();
        RpcUpdateActivity();
    }

    public void UpdateActivity()
    {
        if (isActive == false)
        {
            GetComponent<MeshRenderer>().enabled = false;
            reactivationTime -= Time.deltaTime;

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
        if (isActive == false)
        {
            GetComponent<MeshRenderer>().enabled = false;
            reactivationTime -= Time.deltaTime;

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
    // Update is called once per frame
    void Update () {

        if(GetComponent<MeshRenderer>())
        CmdUpdateActivity();

    }
}
