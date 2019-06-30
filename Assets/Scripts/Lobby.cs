using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Lobby : MonoBehaviour {

    [SyncVar] public bool hasGameStarted = false;

    [SyncVar] public int NumberOfPlayers = 0;

    private int MinNumOfPlayers = 2;

    public float timeTillGameStart = 30.0f;

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

    }

    public void CountDown()
    {

    }
    private void Update()
    {
        NumberOfPlayers = 0;
        for (int i = 0; i < GameObject.FindGameObjectsWithTag("Client").Length; i++)
        {
            NumberOfPlayers++;
        }

        if(NumberOfPlayers >= MinNumOfPlayers)
        {
            

        }


    }
}
