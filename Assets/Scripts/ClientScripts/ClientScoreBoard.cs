using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class ClientScoreBoard : NetworkBehaviour {

    //Represents if scoreboard is currently visible
    public bool scoreBoardActive = false;

    //For counting player numbers
    private int team1Count = 0;
    private int team2Count = 0;

    //For calculating updated score for each team
    private int team1ScoreNo = 0;
    private int team2ScoreNo = 0;

    //GUI
    //Reference for the scoreboard itself
    public GameObject scoreBoard;

    //Represent each total score
    public TextMeshProUGUI team1Score;
    public TextMeshProUGUI team2Score;

    //Array of all the players names on each team
    public TextMeshProUGUI[] team1Names;
    public TextMeshProUGUI[] team2Names;

    //Int representation of how many players on each team
    public TextMeshProUGUI team1PlayerTotal;
    public TextMeshProUGUI team2PlayerTotal;

	
	// Update is called once per frame
	void Update () {
        if (scoreBoardActive)
            UpdateScoreBoard();
    }

    void UpdateScoreBoard()
    {


        GameObject[] players = GameObject.FindGameObjectsWithTag("Client");

        //Reset counters
        team1Count = 0;
        team2Count = 0;

        //Reset score
        team1ScoreNo = 0;
        team2ScoreNo = 0;

        foreach (GameObject g in players)
        {
            if (g.GetComponent<Client>().team == 0)
            {
                team1Names[team1Count].gameObject.SetActive(true);
                team1Names[team1Count].text = g.GetComponent<Client>().playerName + "     " + g.GetComponent<Client>().kills + "     " + g.GetComponent<Client>().deaths;
                team1Count++;
                team1ScoreNo += g.GetComponent<Client>().kills;
            }
            else
            {
                team2Names[team2Count].gameObject.SetActive(true);
                team2Names[team2Count].text = g.GetComponent<Client>().playerName + "     " + g.GetComponent<Client>().kills + "     " + g.GetComponent<Client>().deaths;
                team2Count++;
                team2ScoreNo += g.GetComponent<Client>().kills;
            }
        }
        //Update each teams score
        team1Score.text = team1ScoreNo.ToString();
        team2Score.text = team2ScoreNo.ToString();

        //Display each teams number of players
        team1PlayerTotal.text = team1Count + "/5";
        team2PlayerTotal.text = team2Count + "/5";
    }

    public void ToggleScoreBoard()
    {
        if (scoreBoardActive)
        {
            scoreBoard.SetActive(false);
            scoreBoardActive = false;
        }
        else
        {
            scoreBoard.SetActive(true);
            scoreBoardActive = true;
        }
    }

    //Hard disable function for when player is dead
    public void DeactivateScoreBoard()
    {
        if (scoreBoardActive)
        {
            scoreBoard.SetActive(false);
            scoreBoardActive = false;
        }
    }
}
