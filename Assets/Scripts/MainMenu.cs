using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {


    public GameObject killLimitDropdown;
    public GameObject timeLimitDropdown;

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

    }
    public void PlayButton()
    {
        //Destroy any previously loaded games
        if (GameObject.Find("GameManager"))
        {
            Destroy(GameObject.Find("GameManager"));
        }

        //Initialise a game manager
        GameObject g = new GameObject();
        g.name = "GameManager";
        g.AddComponent<Game>();

        //Set up kill count for the game
        if (killLimitDropdown.GetComponent<Dropdown>().value == 0)
        {
            g.GetComponent<Game>().killLimit = 5;
        }
        else if (killLimitDropdown.GetComponent<Dropdown>().value == 1) g.GetComponent<Game>().killLimit = 15;
        else if (killLimitDropdown.GetComponent<Dropdown>().value == 2) g.GetComponent<Game>().killLimit = 60;

        //Set up time limit for the game
        if (timeLimitDropdown.GetComponent<Dropdown>().value == 0) g.GetComponent<Game>().timeLimit = 10;
        else if (timeLimitDropdown.GetComponent<Dropdown>().value == 1) g.GetComponent<Game>().timeLimit = 15;
        else if (timeLimitDropdown.GetComponent<Dropdown>().value == 2) g.GetComponent<Game>().timeLimit = 30;

        //Move to the next scene
        SceneManager.LoadScene(1);
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
