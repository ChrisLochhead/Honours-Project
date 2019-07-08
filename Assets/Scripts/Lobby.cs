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

    private int MinNumOfPlayers = 2;

    public float timeTillGameStart = 10.0f;

    public TextMeshProUGUI status;
    public TextMeshProUGUI mapName;

    public Client Owner;

    public bool lobbyFinished = false;

    public Button StartButton;

    public RawImage gamePreview;

    public bool initialised = false;

    private void Start()
    {
        StartButton.interactable = false;
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

        for (int i = NumberOfPlayers; i < playerTags.Length; i++)
        {
            playerTags[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }


    public void Init()
    {

        //Get the preview image path
        Map m = GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>().maps[GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>().mapNumber].GetComponent<Map>();
        string mPath = m.imageTexturePath;

        //Set name
        mapName.text = m.gameObject.name;

        //Assign the preview image path to the lobby
        byte[] fileData = File.ReadAllBytes(mPath);
        Texture2D testTex = new Texture2D(2, 2);
        testTex.LoadImage(fileData);
        gamePreview.texture = testTex;

        GameObject gameInfo = GameObject.Find("gameInfo");
        if (gameInfo)
        {
            Owner.CmdSetName(gameInfo.GetComponent<GameInfo>().name);
            if (gameInfo.GetComponent<GameInfo>().killLimit != 0 && gameInfo.GetComponent<GameInfo>().timeLimit != 0)
            {
                Owner.killLimit = 1;// gameInfo.GetComponent<GameInfo>().killLimit;
                Owner.timeLimit = gameInfo.GetComponent<GameInfo>().timeLimit;
            }
            else
            {
                //Find the host and use his instead
                Owner.killLimit = GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().killLimit;
                Owner.timeLimit = GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().timeLimit;
            }

            Destroy(gameInfo);
        }

        //Check if game already begun on host
        if (GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().GameStarted == true)
        {
            timeTillGameStart = 0.0f;
            status.text = "Press start to join the game";
        }
    }
    private void Update()
    {

        //Check for initialisation
        if(GameObject.Find("MapFinder(Clone)") && initialised == false)
        {
            Init();
            initialised = true;
        }

        if (initialised) { 
        status.text = "Game will begin in " + ((int)timeTillGameStart).ToString() + " seconds";


            if (lobbyFinished == false)
            {
                NumberOfPlayers = 0;

                GameObject[] clients = GameObject.FindGameObjectsWithTag("Client");
                RefreshLobby();

                //Timer
                if (currentNumberOfPlayers >= MinNumOfPlayers)
                {
                    timeTillGameStart -= Time.deltaTime;
                }
                else
                {
                    status.text = "Looking for " + (MinNumOfPlayers - currentNumberOfPlayers) + " more players";
                    timeTillGameStart = 10.0f;
                }

                //For all players in the initial load of the match
                if (timeTillGameStart <= 0)
                {
                    status.text = "Press start to join the game";
                    Owner.InitialisePlayer();
                    lobbyFinished = true;
                }
            }
            else
            {
                //Check if game already begun on host
                if (GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().GameStarted == true && Owner.GameStarted == false)
                {
                    timeTillGameStart = 0.0f;
                    StartButton.interactable = true;
                    status.text = "Press start to join the game";
                    lobbyFinished = true;
                }//Otherwise if the game hasn't yet started
            }
        }
    }

}
