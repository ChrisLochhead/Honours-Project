using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Lobby : NetworkBehaviour {

    [SyncVar] public bool hasGameStarted = false;

    [SyncVar] public int NumberOfPlayers = 0;

    private int currentNumberOfPlayers = 0;

    public GameObject[] playerTags;

    private int MinNumOfPlayers = 2;

    public float timeTillGameStart = 30.0f;

    public TextMeshProUGUI status;

    public void startGame()
    {
        hasGameStarted = true;
    }

    [Command]
    public void CmdCountDown()
    {
        CountDown();
        RpcCountDown();
    }

    [ClientRpc]
    public void RpcCountDown()
    {
        timeTillGameStart -= Time.deltaTime;
    }

    public void CountDown()
    {
        timeTillGameStart -= Time.deltaTime;
    }

    [Command]
    public void CmdRefreshLobby()
    {
        RefreshLobby();
        RpcRefreshLobby();
    }

    [ClientRpc]
    public void RpcRefreshLobby()
    {
        currentNumberOfPlayers = 0;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            playerTags[NumberOfPlayers].GetComponentInChildren<TextMeshProUGUI>().text = g.GetComponent<Client>().playerName;
            NumberOfPlayers++;
            currentNumberOfPlayers++;
        }
    }

    public void RefreshLobby()
    {
        currentNumberOfPlayers = 0;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            playerTags[NumberOfPlayers].GetComponentInChildren<TextMeshProUGUI>().text = g.GetComponent<Client>().playerName;
            NumberOfPlayers++;
            currentNumberOfPlayers++;
        }
    }

    private void Update()
    {
        NumberOfPlayers = 0;

        GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");

        if(clients.Length > currentNumberOfPlayers)
        {
            CmdRefreshLobby();
        }
        
        if(NumberOfPlayers >= MinNumOfPlayers)
        {
            CmdCountDown();
        }
        else
        {
            timeTillGameStart = 30.0f;
        }

        if(timeTillGameStart <= 0)
        {
            //Start the game by removing the lobby
            foreach(GameObject g in clients)
            {
                g.GetComponent<Client>().InitialisePlayer();
            }
        }

        status.text = "Game will begin in" + ((int)timeTillGameStart).ToString() + "seconds";
    }
}
