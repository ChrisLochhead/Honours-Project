using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class Lobby : NetworkBehaviour
{
    //Player numbers
    private int currentNumberOfPlayers = 0;
    public int MinNumOfPlayers = 2;

    //Player names
    public GameObject[] playerTags;

    //Lobby timer
    [SyncVar]
    public float timeTillGameStart = 10.0f;

    //Status and map name
    public TextMeshProUGUI status;
    public TextMeshProUGUI mapName;

    //Reference to the lobbies owner
    public Client Owner;

    //For detecting when the game has started
    public bool lobbyFinished = false;

    //References to lobby start button and map image
    public Button StartButton;
    public RawImage gamePreview;

    //To check if lobby has been initialised
    public bool initialised = false;

    int currentMapValue = 0;

    private void RefreshLobby()
    {
        //Reset player count
        currentNumberOfPlayers = 0;

        //Cycle through all present clients
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
        {
            //Add their names to the list
            playerTags[currentNumberOfPlayers].GetComponentInChildren<TextMeshProUGUI>().text = g.GetComponent<Client>().playerName;
            currentNumberOfPlayers++;
        }

        //Clear all the unused lobby spaces
        for (int i = currentNumberOfPlayers; i < playerTags.Length; i++)
        {
            if(Owner.isStudy)
                playerTags[i].GetComponentInChildren<TextMeshProUGUI>().text = "AI Agent";
            else
                playerTags[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
    }


    private void Init()
    {

        if(Owner.isStudy)
        {
            MinNumOfPlayers = 2;
        }

        currentMapValue = GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>().mapNumber;

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
            Owner.CmdSetName(gameInfo.GetComponent<GameInfo>().infoName);
            if (gameInfo.GetComponent<GameInfo>().killLimit != 0 && gameInfo.GetComponent<GameInfo>().timeLimit != 0)
            {
                Owner.killLimit = gameInfo.GetComponent<GameInfo>().killLimit;
                Owner.timeLimit = gameInfo.GetComponent<GameInfo>().timeLimit * 60;
            }
            else
            {
                //Find the host and use his instead
                Owner.killLimit = GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().killLimit;
                Owner.timeLimit = GameObject.FindGameObjectsWithTag("Client")[0].GetComponent<Client>().timeLimit * 60;
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
        //Dont update if game has begun
        if (initialised && lobbyFinished)
            return;

        //Check for initialisation
        if (GameObject.Find("MapFinder(Clone)") && initialised == false)
        {
            Init();
            initialised = true;
        }

        //Update once initialised
        if (initialised) { 
        status.text = "Game will begin in " + ((int)timeTillGameStart).ToString() + " seconds";


            if (lobbyFinished == false)
            {
                //Update the lobby
                RefreshLobby();

                //If there is enough players to start, decrement the timer
                if (currentNumberOfPlayers >= MinNumOfPlayers)
                {
                    if (isServer)
                        timeTillGameStart -= Time.deltaTime/3;
                    else
                    {
                        //Find the first gameobject to copy the timer from as this will
                        //always be the host, then break the loop
                        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Client"))
                        {                          
                            timeTillGameStart = g.GetComponent<Lobby>().timeTillGameStart;
                            break;
                        }
                    }
                }
                else
                {
                    //if someone has left, reset the timer 
                    status.text = "Looking for " + (MinNumOfPlayers - currentNumberOfPlayers) + " more players";
                    timeTillGameStart = 10.0f;
                }

                //For all players in the initial load of the match
                if (timeTillGameStart <= 0)
                {
                    status.text = "Press start to join the game";
                    Owner.InitialisePlayer();
                    lobbyFinished = true;

                    //For the study
                    if(GameObject.Find("StudyManager"))
                    {
                        GameObject.Find("StudyManager").GetComponent<StudyManager>().gameInitialised = true;
                    }
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
                }
            }
        }
    }
}
