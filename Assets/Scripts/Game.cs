using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : MonoBehaviour {

    //Record all players in server
    List<GameObject> players = new List<GameObject>();

    //For recording score
    public int team1Score = 0;
    public int team2Score = 0;

    //To be made modifiable
    public int timeLimit = 15;
    public int killLimit = 25;

    private void OnPlayerConnected(NetworkPlayer player)
    {
        players.Clear();
    }

    public void  addPlayer(GameObject p)
    {
            players.Add(p);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void OnKillRegistered(GameObject killer, GameObject killed)
    {
        if (killer.gameObject.GetComponent<Player>().team == 0)
        {
            team1Score++;
        }
        else
        {
            team2Score++;
        }
    }

    // Update is called once per frame
    void Update () {
       // Debug.Log("No of players" + players.Count);
        checkVictory();
	}

    private void checkVictory()
    {
    }
}
