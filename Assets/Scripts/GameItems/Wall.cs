using UnityEngine.Networking;
using UnityEngine;

public class Wall : NetworkBehaviour {

    //For holding there position in space from file
    public Vector2 pos;
    //Holds wall rotation
    public float rot;
    //Represents the type of wall
    public int type;

    public int wallScaleX = 1;
    public int wallScaleY = 1;

    //reference to texture for changing alpha
    Material wallMaterial;

    //Syncs the health percentage
    //of each wall for every client in the network
    [SyncVar]
    public float Health;
    [SyncVar]
    public float totalHealth;

    //Distinguishes build walls from game walls
    public bool isNetworked;

    private void Start()
    {
        //Initialise health based on type
        SetHealth();
    }

    public void SetHealth()
    {
        //Check type and assign health accordingly
        if (type == 0)
        {
            //Red wall
            totalHealth = 80;
        }
        else if (type == 1)
        {
            //Orange wall
            totalHealth = 125;
        }
        else if (type == 2)
        {
            //Green wall
            totalHealth = 175;
        }
        else
        {
            //Impermeable wall
            totalHealth = 1;
        }
        //Set the health to full
        Health = totalHealth;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If wall is inpermeable ignore collision
        if (type == 3)
        {
            return;
        }
        else
        {
            //Check if colliding with a bullet
            if (collision.gameObject.GetComponent<Bullet>())
            {
                //Reduce health
                Health -= collision.gameObject.GetComponent<Bullet>().damageAmount / 2;
            }
        }
    }

    [Command]
    public void CmdUpdateAlpha()
    {
        //Update the alpha according to current health
        //and then transmit it to the rest of the clients.
        UpdateAlpha();
        RpcUpdateAlpha();
    }

    public void UpdateAlpha()
    {

        if (wallMaterial)
        {
            Color tmp = wallMaterial.color;
            tmp.a = ((Health / 100) * 75) / totalHealth;
            wallMaterial.color = tmp;
        }

        if (Health <= 0)
        {
            GetComponent<BoxCollider>().enabled = false;
        }

    }

    [ClientRpc]
    public void RpcUpdateAlpha()
    {

        if (wallMaterial)
        {
            Color tmp = wallMaterial.color;
            tmp.a = ((Health / 100) * 75) / totalHealth;
            wallMaterial.color = tmp;
        }

        if (Health <= 0)
        {
            GetComponent<BoxCollider>().enabled = false;
        }


    }

    void Update () {

        //Assign the wall material if applicable
        if (GetComponent<MeshRenderer>() && wallMaterial == null)
            wallMaterial = GetComponent<MeshRenderer>().material;

        //If this is a game wall and has a material, update its alpha
        //channel.
        if (wallMaterial && isNetworked)
        {
            CmdUpdateAlpha();
        }

	}
}
