using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

	// Use this for initialization
	void Start () {

        sceneCam = Camera.main;
        

        if(!isLocalPlayer)
        {
            isLocal = false;
            //Disable all components unique to the client
            for(int i = 0; i < clientComponents.Length; i++)
            {
                if(i != 1)
                clientComponents[i].enabled = false;
            }

            //Activate healthbar and rank icon
            player.floatingHealthBar.transform.parent.gameObject.SetActive(true);

            foreach (GameObject g in GameObject.Find("Network Manager").GetComponent<NetworkManager>().spawnPrefabs)
            {
                ClientScene.RegisterPrefab(g);
            }

        }
        else
        {
            isLocal = true;
            sceneCam.gameObject.SetActive(false);
            player.floatingHealthBar.transform.parent.gameObject.SetActive(false);
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

    [Command]
    public void CmdUpdateHealth()
    {
        RpcUpdateHealth();
    }

    [ClientRpc]
    public void RpcUpdateHealth()
    {
        float currentHealthPercentage = player.GetHealth() / player.GetMaxHealth();

        //set size
        Vector3 newScale = new Vector3(currentHealthPercentage, 0.1f, 1);

        //set colour
        if (currentHealthPercentage > 0.7f)
            player.floatingHealthBar.GetComponent<Image>().color = Color.green;
        else
        if (currentHealthPercentage < 0.7f && currentHealthPercentage > 0.25f)
            player.floatingHealthBar.GetComponent<Image>().color = Color.yellow;
        else
            player.floatingHealthBar.GetComponent<Image>().color = Color.red;

        //set it to the correct gameobject
        player.floatingHealthBar.transform.localScale = newScale;

        //finally set its position
        // player.floatingHealthBar.GetComponent<RectTransform>().position = (new Vector3(0, 60, 0));
    }
    public void Update()
    {

        //update health bar for clients
        if (!isLocalPlayer)
        {
            CmdUpdateHealth();
        }

        if (Input.GetMouseButtonDown(0) && player.GetCurrentAmmo(player.GetCurrentWeapon()) > 0 && isLocalPlayer)
        {
            CmdSpawnBullet();
        }
        else
        {
            CmdRemoveFlash();
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

        //calculate rotation
        Quaternion rot = b.transform.rotation;
        rot = player.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        b.transform.rotation = rot;

        //calculate trajectory
        b.GetComponent<Rigidbody>().velocity = b.transform.forward * 6.0f;

        NetworkServer.Spawn(b);

        MuzzleFlash(true);
        RpcMuzzleFlash(true);
    }

    [Command]
    public void CmdRemoveFlash()
    {
        MuzzleFlash(false);
        RpcMuzzleFlash(false);
    }

    public void MuzzleFlash(bool istrue)
    {
        player.muzzleFlashes[player.GetCurrentWeapon()].SetActive(istrue);
    }

    [ClientRpc]
    public void RpcMuzzleFlash(bool istrue)
    {
        player.muzzleFlashes[player.GetCurrentWeapon()].SetActive(istrue);
    }

    private void OnDisable()
    {
        if (sceneCam)
            sceneCam.gameObject.SetActive(true);
    }
}
