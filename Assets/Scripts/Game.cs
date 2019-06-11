using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour {

    //Record all players in server
    List<GameObject> players = new List<GameObject>();

	// Use this for initialization
	void Start () {

		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            players.Add(g);
        }
	}

    private void OnPlayerConnected(NetworkPlayer player)
    {
        players.Clear();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            players.Add(g);
        }

    }

    public void onKillRegistered(GameObject killer, GameObject killed)
    {

    }
    // Update is called once per frame
    void Update () {
        Debug.Log("No of players" + players.Count);
	}
}
