using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class Lobby : NetworkBehaviour
{

    [SyncVar] public int NumberOfPlayers = 0;

    private int currentNumberOfPlayers = 0;

    public GameObject[] playerTags;

    private int MinNumOfPlayers = 10;

    public float timeTillGameStart = 10.0f;

    public TextMeshProUGUI status;

    public Client Owner;

    public GameManager gameManager;

    public bool lobbyFinished = false;

    public Button StartButton;

    public RawImage gamePreview;

    private void Start()
    {

        //Test
        //DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/mapImages/");
        //FileInfo[] fileInfo = info.GetFiles();
        //foreach (FileInfo f in fileInfo)
        //{
            byte[] fileData = File.ReadAllBytes(Application.dataPath + "/MapImages/saved.png");
            Texture2D testTex = new Texture2D(2, 2);
            testTex.LoadImage(fileData);
            gamePreview.texture = testTex;

       // }

     


        GameObject gameInfo = GameObject.Find("gameInfo");
        Owner.CmdSetName(gameInfo.GetComponent<GameInfo>().name);
        //Owner.playerName = gameInfo.GetComponent<GameInfo>().name;
        if (gameInfo.GetComponent<GameInfo>().killLimit != 0 && gameInfo.GetComponent<GameInfo>().timeLimit != 0)
        {
            gameManager.killLimit = gameInfo.GetComponent<GameInfo>().killLimit;
            gameManager.timeLimit = gameInfo.GetComponent<GameInfo>().timeLimit;
        }
        Destroy(gameInfo);

        //Check if game already begun on host
        if (GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().GameStarted == true)
        {
            timeTillGameStart = 0.0f;
            status.text = "Press start to join the game";
        }
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

        status.text = "Game will begin in " + ((int)timeTillGameStart).ToString() + " seconds";


        //Check if game already begun on host
        if (GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().GameStarted == true)
        {
            timeTillGameStart = 0.0f;
            StartButton.enabled = true;
            status.text = "Press start to join the game";
        }

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
                status.text = "Looking for " + (MinNumOfPlayers - currentNumberOfPlayers) + " more players";
                timeTillGameStart = 10.0f;
            }

            if (timeTillGameStart <= 0 && StartButton.enabled == false)
            {
                status.text = "Press start to join the game";

                //Start the game by removing the lobby
                foreach (GameObject g in clients)
                {
                    g.GetComponent<Client>().InitialisePlayer();
                }
                

                lobbyFinished = true;
            }


        }
    }

    public void OnStartButtonClicked()
    {

        //GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");

        ////Start the game by removing the lobby
        //foreach (GameObject g in clients)
        //{
        //    g.GetComponent<Client>().InitialisePlayer();
        //}
        //StartButton.enabled = true;
    }
}
