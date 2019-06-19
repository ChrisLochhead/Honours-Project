using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : MonoBehaviour {

    //Record all players in server
    List<GameObject> players = new List<GameObject>();

    //For recording score
    public int team1Score = 24;
    public int team2Score = 132;

    //To be made modifiable
    public int timeLimit = 15;
    public int killLimit = 25;

    public void AddPlayer(GameObject p)
    {
       players.Add(p);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnKillRegistered(GameObject killer, GameObject killed)
    {
        if (killer.transform.parent.GetComponent<Client>().team == 0)
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
        Debug.Log("No of players" + players.Count);
        CheckVictory();
	}

    private void CheckVictory()
    {
    }
}
