using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    private void Start()
    {
        if (!GameObject.Find("PersistentObject"))
        {
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
    }
    public void PlayButton()
    {
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
