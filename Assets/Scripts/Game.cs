using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour {

    //Record all players in server
    public List<GameObject> team1Players = new List<GameObject>();
    public List<GameObject> team2Players = new List<GameObject>();

    //For recording score
    public int team1Score = 0;
    public int team2Score = 0;

    //To be made modifiable
    public int timeLimit = 15;
    public int killLimit = 25;

    public void AddPlayer(GameObject p)
    {
        if (p.GetComponent<Client>().team == 0)
            team1Players.Add(p);
        else
            team2Players.Add(p);
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
        CheckVictory();
	}

    private void CheckVictory()
    {
        if(team1Score >= killLimit)
        {
            Victory(1);
        }
        else if( team2Score >= killLimit)
        {
            Victory(2);
        }
    }

    public void Victory(int t)
    {
        //foreach (GameObject p in players)
        //{
        //    //Players of this team have won
        //    if(p.GetComponent<Client>().team == t)
        //    {

        //    }
        //    else
        //    {

        //    }

        //}
    }
}
