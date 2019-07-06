using UnityEngine;
using UnityEngine.SceneManagement;
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

    //for win condition
    public TextMeshProUGUI winCondition;

    public Client Owner;

    //For the end of the game
    public bool ConditionSet = false;
    public GameObject ContinueButton;

	// Update is called once per frame
	void Update () {
        if (scoreBoardActive)
            UpdateScoreBoard();

        //Check player hasn't already won or lost
        if (Owner.hasWon == false && Owner.hasLost == false)
        {
            CheckVictory();
        }

        if (Owner.hasWon == true) ClientWon();
        if (Owner.hasLost == true) ClientLost();
    }

    public void ClientWon()
    {
        scoreBoard.SetActive(true);
        scoreBoardActive = true;
        winCondition.gameObject.SetActive(true);
        winCondition.text = "You Win!";
        ContinueButton.SetActive(true);
    }

    public void ClientLost()
    {
        scoreBoard.SetActive(true);
        scoreBoardActive = true;
        winCondition.gameObject.SetActive(true);
        winCondition.text = "You Lose!";
        ContinueButton.SetActive(true);
    }

    public void CheckVictory()
    {
        //Check if player has won or lost
        if (Owner.team == 0)
        {
            if (team1ScoreNo >= Owner.killLimit && Owner.killLimit != 0)
            {
                Owner.hasWon = true;            
            }
            if (team2ScoreNo >= Owner.killLimit && Owner.killLimit != 0)
            {
                Owner.hasLost = true;
            }
        }
        else
        {
            if (team1ScoreNo >= Owner.killLimit && Owner.killLimit != 0)
            {
                Owner.hasLost = true;
            }
            if (team2ScoreNo >= Owner.killLimit && Owner.killLimit != 0)
            {
                Owner.hasWon = true;
            }
        }
    }

    void UpdateScoreBoard()
    {
        //Update each teams score
        team1Score.text = team1ScoreNo.ToString();
        team2Score.text = team2ScoreNo.ToString();

        //Display each teams number of players
        team1PlayerTotal.text = team1Count + "/5";
        team2PlayerTotal.text = team2Count + "/5";

        //Clear players no longer in game
        for (int i = 0; i < team1Names.Length; i++)
        {
            if (i >= team1Count)
                team1Names[i].gameObject.SetActive(false);
        }
        //Clear players no longer in game
        for (int i = 0; i < team1Names.Length; i++)
        {
            if (i >= team2Count)
                team2Names[i].gameObject.SetActive(false);
        }
    }

    public void UpdateScores()
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
                team1Names[team1Count].text = g.GetComponent<Client>().playerName;
                TextMeshProUGUI[] childText = team1Names[team1Count].GetComponentsInChildren<TextMeshProUGUI>();
                childText[1].text = g.GetComponent<Client>().kills.ToString();
                childText[2].text = g.GetComponent<Client>().deaths.ToString();

                team1Count++;
                team1ScoreNo += g.GetComponent<Client>().kills;
            }
            else
            {
                team2Names[team2Count].gameObject.SetActive(true);
                team2Names[team2Count].text = g.GetComponent<Client>().playerName;
                TextMeshProUGUI[] childText = team2Names[team2Count].GetComponentsInChildren<TextMeshProUGUI>();
                childText[1].text = g.GetComponent<Client>().kills.ToString();
                childText[2].text = g.GetComponent<Client>().deaths.ToString();

                team2Count++;
                team2ScoreNo += g.GetComponent<Client>().kills;
            }
        }
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
