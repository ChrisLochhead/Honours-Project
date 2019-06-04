using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ClientSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] clientComponents;

    Camera sceneCam;

    public Player player;
    public GameObject bullet;

	// Use this for initialization
	void Start () {

        sceneCam = Camera.main;

        if(!isLocalPlayer)
        {
            //Disable all components unique to the client
            for(int i = 0; i < clientComponents.Length; i++)
            {
                clientComponents[i].enabled = false;
            }
        }
        else
        {
            sceneCam.gameObject.SetActive(false);
        }
	}

    public void Update()
    {
        if (Input.GetMouseButtonDown(0) && player.GetCurrentAmmo(player.GetCurrentWeapon()) > 0)
        {
           // CmdSpawnBullet();
        }
    }

    //[Command]
    //void CmdSpawnBullet()
    //{
    //    GameObject b = Instantiate(bullet, player.crosshairMarker.transform.position, Quaternion.identity * Quaternion.Euler(new Vector3(-90, 0, 0)));
    //    b.GetComponent<Bullet>().isTemplate = false;
    //    NetworkServer.Spawn(b);
    //}

    private void OnDisable()
    {
        if (sceneCam)
            sceneCam.gameObject.SetActive(true);
    }
}
