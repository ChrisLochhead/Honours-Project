using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using TMPro;

public class MainMenu : NetworkBehaviour {

    public TMP_InputField nameInputHostMultiplayer;
    public TMP_InputField nameInputJoinMultiplayer;
    public GameObject killLimitDropdownMultiplayer;
    public GameObject timeLimitDropdownMultiplayer;

    public TMP_InputField nameInputHostLAN;
    public TMP_InputField nameInputJoinLAN;
    public GameObject killLimitDropdownLAN;
    public GameObject timeLimitDropdownLAN;

    private NetworkManager networkManager;

    public GameObject mapFinderPrefab;

    public TextMeshProUGUI errorMessage;

    private void Start()
    {

        InitialiseMapFinder();

        //Set up networking
        networkManager = NetworkManager.singleton;


        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

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

    public void HostButton()
    {
        if (nameInputHostMultiplayer.text != "")
        {
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

            if (killLimitDropdownLAN.GetComponent<Dropdown>().value == 0)
                gameInfo.GetComponent<GameInfo>().killLimit = 5;
            else if (killLimitDropdownLAN.GetComponent<Dropdown>().value == 1)
                gameInfo.GetComponent<GameInfo>().killLimit = 15;
            else
                gameInfo.GetComponent<GameInfo>().killLimit = 60;

            if (timeLimitDropdownLAN.GetComponent<Dropdown>().value == 0)
                gameInfo.GetComponent<GameInfo>().timeLimit = 10;
            else if (timeLimitDropdownLAN.GetComponent<Dropdown>().value == 1)
                gameInfo.GetComponent<GameInfo>().timeLimit = 15;
            else
                gameInfo.GetComponent<GameInfo>().timeLimit = 30;

            //Create a match
            networkManager.matchMaker.CreateMatch(nameInputHostMultiplayer.text + "'s room", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }
        else
        {
            errorMessage.gameObject.SetActive(true);
        }
        
    }

    public void JoinButton()
    {
        if (nameInputJoinMultiplayer.text != "")
        {
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputJoinMultiplayer.text;

            networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        }
        else
        {
            errorMessage.gameObject.SetActive(true);
        }
    }

    public void HostButtonLAN()
    {
        if (nameInputHostLAN.text != "")
        {
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
            gameInfo.GetComponent<GameInfo>().name = nameInputHostLAN.text;

            if (killLimitDropdownLAN.GetComponent<Dropdown>().value == 0)
                gameInfo.GetComponent<GameInfo>().killLimit = 5;
            else if (killLimitDropdownLAN.GetComponent<Dropdown>().value == 1)
                gameInfo.GetComponent<GameInfo>().killLimit = 15;
            else
                gameInfo.GetComponent<GameInfo>().killLimit = 60;

            if (timeLimitDropdownLAN.GetComponent<Dropdown>().value == 0)
                gameInfo.GetComponent<GameInfo>().timeLimit = 10;
            else if (timeLimitDropdownLAN.GetComponent<Dropdown>().value == 1)
                gameInfo.GetComponent<GameInfo>().timeLimit = 15;
            else
                gameInfo.GetComponent<GameInfo>().timeLimit = 30;


            networkManager.StartHost();
        }
        else
        {
            errorMessage.gameObject.SetActive(true);
        }
    }

    public void JoinButtonLAN()
    {
        if (nameInputJoinLAN.text != "")
        {
            errorMessage.gameObject.SetActive(false);

            //Store info for the next scene
            GameObject gameInfo = new GameObject();
            gameInfo.AddComponent<GameInfo>();
            gameInfo.name = "gameInfo";
            gameInfo.GetComponent<GameInfo>().name = nameInputJoinLAN.text;

            networkManager.StartClient();
        }
        else
        {
            errorMessage.gameObject.SetActive(true);
        }
    }

    public void SelectLAN()
    {
        if (networkManager.matchMaker != null)
        {
            networkManager.matchMaker = null;
        }
    }

    public void SelectMultiPlayer()
    {
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

        networkManager.matchMaker.JoinMatch(priorityMatch.networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
    }

    public void BuildButton()
    {
        Destroy(GameObject.Find("MapFinder(Clone)"));
        SceneManager.LoadScene(2);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

}
