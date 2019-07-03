using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Lobby : NetworkBehaviour
{

    [SyncVar] public int NumberOfPlayers = 0;

    private int currentNumberOfPlayers = 0;

    public GameObject[] playerTags;

    private int MinNumOfPlayers = 2;

    public float timeTillGameStart = 10.0f;

    public TextMeshProUGUI status;

    public Client Owner;

    public GameManager gameManager;

    public bool lobbyFinished = false;

    private void Start()
    {
        GameObject gameInfo = GameObject.Find("gameInfo");
        Owner.CmdSetName(gameInfo.GetComponent<GameInfo>().name);
        //Owner.playerName = gameInfo.GetComponent<GameInfo>().name;
        if (gameInfo.GetComponent<GameInfo>().killLimit != 0 && gameInfo.GetComponent<GameInfo>().timeLimit != 0)
        {
            gameManager.killLimit = gameInfo.GetComponent<GameInfo>().killLimit;
            gameManager.timeLimit = gameInfo.GetComponent<GameInfo>().timeLimit;
        }
        Destroy(gameInfo);
    }

    //public void StartGame()
    //{
    //    //Start the game by removing the lobby
    //    foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
    //    {
    //        g.GetComponent<Client>().InitialisePlayer();
    //    }
    //}

    public void RefreshLobby()
    {
        currentNumberOfPlayers = 0;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            playerTags[NumberOfPlayers].GetComponentInChildren<TextMeshProUGUI>().text = g.GetComponent<Client>().playerName;
            NumberOfPlayers++;
            currentNumberOfPlayers++;
        }

        for (int i = NumberOfPlayers; i < playerTags.Length; i++)
        {
            playerTags[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }

    private void Update()
    {
        if (lobbyFinished == false)
        {
            NumberOfPlayers = 0;

            GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");
            RefreshLobby();


            if (currentNumberOfPlayers >= MinNumOfPlayers)
            {
                timeTillGameStart -= Time.deltaTime;
            }
            else
            {
                timeTillGameStart = 10.0f;
            }

            if (timeTillGameStart <= 0)
            {
                //Start the game by removing the lobby
                foreach (GameObject g in clients)
                {
                    g.GetComponent<Client>().InitialisePlayer();
                }

                lobbyFinished = true;
            }

            status.text = "Game will begin in " + ((int)timeTillGameStart).ToString() + " seconds";
        }
    }
}
