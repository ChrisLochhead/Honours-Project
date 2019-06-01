using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ClientSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] clientComponents;

    Camera sceneCam;

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

    private void OnDisable()
    {
        if (sceneCam)
            sceneCam.gameObject.SetActive(true);
    }
}
