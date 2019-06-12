using System.Collections;
using System.IO;
using System.Collections.Generic;
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

    public GameObject gameManager;
    public List<GameObject> spawnPoints;

	// Use this for initialization
	void Start () {

        sceneCam = Camera.main;

        gameManager = GameObject.Find("MapManager");
        if(player.team == 1)
            spawnPoints = gameManager.GetComponent<GameMap>().team1Spawns;
        else
            spawnPoints = gameManager.GetComponent<GameMap>().team2Spawns;

        //set up spawnpoint
        int rand = Random.Range(0, spawnPoints.Count);
        player.transform.position = spawnPoints[rand].transform.position;

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

        //Set size
        Vector3 newScale = new Vector3(currentHealthPercentage, 0.1f, 1);

        //Set colour
        if (currentHealthPercentage > 0.7f)
            player.floatingHealthBar.GetComponent<Image>().color = Color.green;
        else
        if (currentHealthPercentage < 0.7f && currentHealthPercentage > 0.25f)
            player.floatingHealthBar.GetComponent<Image>().color = Color.yellow;
        else
            player.floatingHealthBar.GetComponent<Image>().color = Color.red;

        //Set it to the correct gameobject
        player.floatingHealthBar.transform.localScale = newScale;

        //And finally set it's position
        player.floatingHealthBar.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 7.5f, player.transform.position.z);
        player.floatingRankIcon.transform.position = new Vector3(player.transform.position.x - 5.8f, player.transform.position.y + 7.75f, player.transform.position.z);

    }
    public void Update()
    {

       CmdUpdateHealth();

        if (Input.GetMouseButtonDown(0) && player.GetCurrentAmmo(player.GetCurrentWeapon()) > 0 && isLocalPlayer)
        {
            CmdSpawnBullet();
            player.TakeDamage(15);
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
        GameObject b = (GameObject)Instantiate(bullet, new Vector3(player.crosshairMarker.transform.position.x, player.crosshairMarker.transform.position.y, -4.5f), Quaternion.identity);

        //calculate rotation
        Quaternion rot = b.transform.rotation;
        rot = player.transform.rotation;
        rot *= Quaternion.Euler(-90, 0, 0);
        b.transform.rotation = rot;

        //calculate trajectory
        b.GetComponent<Rigidbody>().velocity = b.transform.forward * 6.0f;

        //add tag indicating whose bullet it is
        b.GetComponent<Bullet>().shooter = player.gameObject;
        b.GetComponent<Bullet>().damageAmount = player.damageAmounts[player.GetCurrentWeapon()];

        NetworkServer.Spawn(b);

        MuzzleFlash(true);
        RpcMuzzleFlash(true);
    }

    [Command]
    public void CmdRegisterDamage(int shooterID)
    {

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

    public void Respawn()
    {
        //set up spawnpoint
        int rand = Random.Range(0, spawnPoints.Count);
        Debug.Log("current : " + player.transform.position  + "new : " + spawnPoints[rand].transform.position);
        player.transform.position = spawnPoints[rand].transform.position;
        player.playerCam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.playerCam.transform.position.z);
        
    }

    public void exitGame()
    {
        NetworkManager.singleton.StopClient();
    }
}
