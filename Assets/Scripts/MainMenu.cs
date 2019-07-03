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

    public TMP_InputField nameInput;

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
        gameInfo.GetComponent<GameInfo>().name = nameInput.text;
        gameInfo.GetComponent<GameInfo>().killLimit = killLimitDropdown.GetComponent<Dropdown>().value;
        gameInfo.GetComponent<GameInfo>().timeLimit = timeLimitDropdown.GetComponent<Dropdown>().value;

        //Create a match
        networkManager.matchMaker.CreateMatch("roomName", 4, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
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
