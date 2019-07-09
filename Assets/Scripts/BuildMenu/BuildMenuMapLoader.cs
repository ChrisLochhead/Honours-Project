using System.Collections.Generic;
using UnityEngine;

public class BuildMenuMapLoader : MonoBehaviour {

    //templates
    //Walls
    public GameObject redWall;
    public GameObject orangeWall;
    public GameObject greenWall;
    public GameObject greyWall;

    //Coins
    public GameObject goldCoin;
    public GameObject silverCoin;
    public GameObject bronzeCoin;

    //Spawns
    public GameObject teamFlag1;
    public GameObject teamFlag2;

    //Build menu reference
    public BuildMenu buildMenu;

    //Map finder reference
    public GameObject mapFinderPrefab;
    private GameObject mapFinder;

    public void LoadMap()
    {

        //clear scene first
        Destroy(GameObject.Find("CurrentMapState"));

        for (int i = 0; i < buildMenu.mapItems.Count; i++)
        {
            Destroy(buildMenu.mapItems[i]);
        }

        //Then initialise a new map state
        GameObject newState = new GameObject();
        newState.name = "CurrentMapState";

        //Find the selected maps map items.
        List<GameObject> mapInfo = mapFinder.GetComponent<MapFinder>().selectedMap.GetComponent<Map>().GetMapItems();

        //Iterate through the map items
        for (int i = 0; i < mapInfo.Count; i++)
        {
            GameObject tmp = new GameObject();

            //Add items to the scene depending on type, position and rotation data
            //Walls
            if (mapInfo[i].GetComponent<Wall>())
            {
                if (mapInfo[i].GetComponent<Wall>().type == 0)
                {
                    tmp = Instantiate(redWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 1)
                {
                    tmp = Instantiate(orangeWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 2)
                {
                    tmp = Instantiate(greenWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
                if (mapInfo[i].GetComponent<Wall>().type == 3)
                {
                    tmp = Instantiate(greyWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                }
            }
            else // Coins
                if (mapInfo[i].GetComponent<Coin>().type == 4)
            {
                tmp = Instantiate(goldCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 5)
            {
                tmp = Instantiate(silverCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 6)
            {
                tmp = Instantiate(bronzeCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
            }
            else // Spawns
                if (mapInfo[i].GetComponent<Coin>().type == 7)
            {
                tmp = Instantiate(teamFlag1, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity * Quaternion.Euler(-90, 0, 0));
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 8)
            {
                tmp = Instantiate(teamFlag2, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity * Quaternion.Euler(-90, 0, 180));
            }

            //Parent every map object to the game state
            tmp.transform.parent = newState.transform;
            //Add it to buildmenus copy of its map items.
            buildMenu.mapItems.Add(tmp);
        }
        //Close the load menu screen when loading is finished
        buildMenu.loadMenu.SetActive(false);
    }

    public void InitialiseMapFinder()
    {
        //Initialise prefab but dont add to network
        mapFinder = (GameObject)Instantiate(mapFinderPrefab);

    }

    public void DeleteMapFinder()
    {
        //Delete the mapfinder
        Destroy(mapFinder);
    }

}
