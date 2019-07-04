using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using TMPro;

public class MainMenu : MonoBehaviour {


    public GameObject killLimitDropdown;
    public GameObject timeLimitDropdown;

    public GameObject gameManager;

    public TMP_InputField nameInputHost;
    public TMP_InputField nameInputJoin;

    private NetworkManager networkManager;

    private void Start()
    {

        if (GameObject.Find("PersistentObject"))
        {
            Destroy(GameObject.Find("PersistentObject"));
        }

            GameObject PersistentObject = new GameObject();
            PersistentObject.AddComponent<MapFinder>();
            PersistentObject.name = "PersistentObject";

            GameObject Map = new GameObject();
            Map.AddComponent<Map>();
            Map.transform.parent = PersistentObject.transform;
            Map.name = "Map";

            GameObject selectedMap = new GameObject();
            selectedMap.transform.parent = PersistentObject.transform;
            selectedMap.name = "SelectedMap";

            PersistentObject.GetComponent<MapFinder>().selectedMap = selectedMap;
            PersistentObject.GetComponent<MapFinder>().map = Map;

        //Set up networking
        networkManager = NetworkManager.singleton;

        if(networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }

    }
    public void HostButton()
    {
        //Destroy any previously loaded games
        if (GameObject.Find("game"))
        {
            Destroy(GameObject.Find("game"));
        }

        //Store info for the next scene
        GameObject gameInfo = new GameObject();
        gameInfo.AddComponent<GameInfo>();
        gameInfo.name = "gameInfo";
        gameInfo.GetComponent<GameInfo>().name = nameInputHost.text;
        gameInfo.GetComponent<GameInfo>().killLimit = killLimitDropdown.GetComponent<Dropdown>().value;
        gameInfo.GetComponent<GameInfo>().timeLimit = timeLimitDropdown.GetComponent<Dropdown>().value;

        //Create a match
        Debug.Log("called network new");
        networkManager.matchMaker.CreateMatch( nameInputHost.text + "'s room", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        
    }

    public void JoinButton()
    {
        //Store info for the next scene
        GameObject gameInfo = new GameObject();
        gameInfo.AddComponent<GameInfo>();
        gameInfo.name = "gameInfo";
        gameInfo.GetComponent<GameInfo>().name = nameInputJoin.text;

        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
    }

    public void HostButtonLAN()
    {
        networkManager.StartHost();
    }

    public void JoinButtonLAN()
    {
        networkManager.StartClient();
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
        SceneManager.LoadScene(2);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

}
