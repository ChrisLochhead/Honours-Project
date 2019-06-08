using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ClientSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] clientComponents;

    Camera sceneCam;

    public Player player;

    [SerializeField]
    public GameObject bullet;
    public bool isLocal;

    [SyncVar(hook = "SetWeapon")]
    public int wepType;

    //[SyncVar(hook = "SpawnBullet")]
    //public GameObject[] bullets; 

	// Use this for initialization
	void Start () {

        sceneCam = Camera.main;

        if(!isLocalPlayer)
        {
            isLocal = false;
            //Disable all components unique to the client
            for(int i = 0; i < clientComponents.Length; i++)
            {
                clientComponents[i].enabled = false;
            }

            foreach (GameObject g in GameObject.Find("Network Manager").GetComponent<NetworkManager>().spawnPrefabs)
            {
                ClientScene.RegisterPrefab(g);
            }
        }
        else
        {
            isLocal = true;
            sceneCam.gameObject.SetActive(false);
        }
	}

    [Command]
    void CmdSetWeapon(int type)
    {
        //currently both weapons change on server, changes properly on client
        SetWeapon(type);
        RpcSetWeapon(type);
    }

    [ClientRpc]
    public void RpcSetWeapon(int type)
    {
        player.SetCurrentWeapon(type);

        for (int i = 0; i < player.guns.Length; i++)
        {
            if (i == type)
                player.guns[i].SetActive(true);
            else
                player.guns[i].SetActive(false);
        }
    }

    public void SetWeapon(int type)
    {
        player.SetCurrentWeapon(type);
        wepType = type;

        for (int i = 0; i < player.guns.Length; i++)
        {
            if (i == type)
                player.guns[i].SetActive(true);
            else
                player.guns[i].SetActive(false);
        }
    }


    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && player.GetCurrentAmmo(player.GetCurrentWeapon()) > 0 && isLocalPlayer)
        {
            CmdSpawnBullet();
        }

        //Weapon switching
        if (Input.GetKey("1") && isLocalPlayer) CmdSetWeapon(0);
        if (Input.GetKey("2") && isLocalPlayer) CmdSetWeapon(1);
        if (Input.GetKey("3") && isLocalPlayer) CmdSetWeapon(2);
        if (Input.GetKey("4") && isLocalPlayer) CmdSetWeapon(3);
        if (Input.GetKey("5") && isLocalPlayer) CmdSetWeapon(4);

    }

    [Command]
    public void CmdSpawnBullet()
    {
        GameObject b = (GameObject)Instantiate(bullet, player.crosshairMarker.transform.position, Quaternion.identity);
        b.GetComponent<Bullet>().isTemplate = false;
        b.GetComponent<Bullet>().shooter = player.gameObject;
        NetworkServer.Spawn(b);

       // SpawnBullet();
       // RpcSpawnBullet();
    }

    public void SpawnBullet()
    {
        player.muzzleFlashes[player.GetCurrentWeapon()].SetActive(true);
        player.SetCurrentAmmo(player.GetCurrentWeapon());
    }

    [ClientRpc]
    public void RpcSpawnBullet()
    {
        player.muzzleFlashes[player.GetCurrentWeapon()].SetActive(true);
        player.SetCurrentAmmo(player.GetCurrentWeapon());
    }

    private void OnDisable()
    {
        if (sceneCam)
            sceneCam.gameObject.SetActive(true);
    }
}
