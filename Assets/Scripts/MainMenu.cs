using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class MainMenu : MonoBehaviour {


    public GameObject killLimitDropdown;
    public GameObject timeLimitDropdown;

    public GameObject gameManager;

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
    public void PlayButton()
    {
        //Destroy any previously loaded games
        if (GameObject.Find("game"))
        {
            Destroy(GameObject.Find("game"));
        }

        //Move to the next scene
        //SceneManager.LoadScene(1);
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
