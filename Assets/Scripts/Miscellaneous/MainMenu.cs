using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using TMPro;

public class MainMenu : NetworkBehaviour {

    //Multiplayer dropdowns and input fields
    public TMP_InputField nameInputHostMultiplayer;
    public TMP_InputField nameInputJoinMultiplayer;
    public Dropdown killLimitDropdownMultiplayer;
    public Dropdown timeLimitDropdownMultiplayer;

    //LAN dropdowns and input fields
    public TMP_InputField nameInputHostLAN;
    public TMP_InputField nameInputJoinLAN;
    public Dropdown killLimitDropdownLAN;
    public Dropdown timeLimitDropdownLAN;

    //Reference to the network manager
    private NetworkManager networkManager;

    //Reference to the mapfinder
    public GameObject mapFinderPrefab;

    //Name entry error message
    public TextMeshProUGUI errorMessage;

    private void Start()
    {
        //Create a map finder and find all the available maps
        InitialiseMapFinder();

        //Set up networking
        networkManager = NetworkManager.singleton;
    }

    public void DisableErrorMessage()
    {
        errorMessage.gameObject.SetActive(false);
    }

    public void InitialiseMapFinder()
    {
        //Initialise prefab but dont add to network
        GameObject tmp = (GameObject)Instantiate(mapFinderPrefab);
    }

    public void DeleteMapFinder()
    {
        Destroy(GameObject.Find("MapFinder(Clone)"));
    }

    private float getDropdownValue(bool isKillLimit, int value)
    {
        //Return the value based on which type of dropdown, and said dropdowns value
        if (isKillLimit)
        {
            if (value == 0)
                return 5;
            else if (value == 1)
                return 15;
            else
                return 60;
        }
        else
        {
            if (value == 0)
                return 10;
            else if (value == 1)
                return 15;
            else
                return 30;
        }
    }

    public void HostButton()
    {
        if (nameInputHostMultiplayer.text != "")
        {
            //Disable the error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Destroy any previously loaded games
            if (GameObject.Find("game"))
            {
                Destroy(GameObject.Find("game"));
            }

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputHostMultiplayer.text;

            //Assign the kill and time limit based on value
            gameInfo.GetComponent<GameInfo>().killLimit = (int)getDropdownValue(true, killLimitDropdownMultiplayer.value);
            gameInfo.GetComponent<GameInfo>().timeLimit = getDropdownValue(false, timeLimitDropdownMultiplayer.value);

            //Create a match
            networkManager.matchMaker.CreateMatch(nameInputHostMultiplayer.text + "'s room", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void JoinButton()
    {
        //Join game if name isn't empty
        if (nameInputJoinMultiplayer.text != "")
        {
            //Remove error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Only store name info when joining a game
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputJoinMultiplayer.text;

            //Join the match
            networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void HostButtonLAN()
    {
        //If the name input isn't empty
        if (nameInputHostLAN.text != "")
        {
            //Disable any error message if applicable
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputHostLAN.text;

            //Assign the kill and time limit based on value
            gameInfo.GetComponent<GameInfo>().killLimit = (int)getDropdownValue(true, killLimitDropdownLAN.value);
            gameInfo.GetComponent<GameInfo>().timeLimit = getDropdownValue(false, timeLimitDropdownLAN.value);

            //Start a local game
            networkManager.StartHost();
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void JoinButtonLAN()
    {
        //If the name isn't empty
        if (nameInputJoinLAN.text != "")
        {
            //Disable any applicable error message
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputJoinLAN.text;

            //Join local game as client
            networkManager.StartClient();
        }
        else
            errorMessage.gameObject.SetActive(true);
    }

    public void SelectLAN()
    {
        //Disable multiplayer
        if (networkManager.matchMaker != null)
        {
            networkManager.matchMaker = null;
        }
    }

    public void SelectMultiPlayer()
    {
        //Enable multiplayer
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
    }


    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        MatchInfoSnapshot priorityMatch = new MatchInfoSnapshot();

        //Find match closest to being full
        foreach (MatchInfoSnapshot match in matches)
        {
            if(match.currentSize < match.maxSize && priorityMatch.currentSize < match.currentSize)
            {
                priorityMatch = match;
            }
        }
        //Join this match
        networkManager.matchMaker.JoinMatch(priorityMatch.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
    }

    public void BuildButton()
    {
        //Destroy the main menus copy of mapfinder
        Destroy(GameObject.Find("MapFinder(Clone)"));
        SceneManager.LoadScene(2);
    }

    public void QuitButton()
    {
        //Close the application
        Application.Quit();
    }

}
