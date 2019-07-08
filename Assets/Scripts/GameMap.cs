using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : NetworkBehaviour {

    //List of map objects
    private List<GameObject> mapInfo;

    //Prefabs for map objects
    //Ground
    public GameObject ground;

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

    //Border
    public GameObject borderWall;

    // Use this for initialization
    void Start () {
        mapInfo = new List<GameObject>();
        mapInfo = GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>().selectedMap.GetComponent<Map>().GetMapItems();
        if(isServer)
        InitMap();
	}

    void InitMap()
    {
        //create the ground and border walls
        GameObject environmentObj = (GameObject)Instantiate(ground, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        GameObject borderObj = (GameObject)Instantiate(borderWall, new Vector3(-297.7f, 0, -10), Quaternion.identity);
        NetworkServer.Spawn(borderObj);

        //Cycle through and create map from map info
        for (int i = 0; i < mapInfo.Count; i++)
        {
            if (mapInfo[i].GetComponent<Wall>())
            {
                if (mapInfo[i].GetComponent<Wall>().type == 0 && isServer)
                {
                    //Weak wall
                    GameObject spawnObj = (GameObject)Instantiate(redWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 1 && isServer)
                {
                    //Medium wall
                    GameObject spawnObj = (GameObject)Instantiate(orangeWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 2 && isServer)
                {
                    //Strong wall
                    GameObject spawnObj = (GameObject)Instantiate(greenWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                if (mapInfo[i].GetComponent<Wall>().type == 3 && isServer)
                {
                    //Impenetrable wall
                    GameObject spawnObj = (GameObject)Instantiate(greyWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 4 && isServer)
            {
                //Gold coin
                GameObject spawnObj = (GameObject)Instantiate(goldCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 5 && isServer)
            {
                //Silver coin
                GameObject spawnObj = (GameObject)Instantiate(silverCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 6 && isServer)
            {
                //Bronze coin
                GameObject spawnObj = (GameObject)Instantiate(bronzeCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 7)
            {
                //Red team spawnpoint
                GameObject tmp = Instantiate(teamFlag1, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(tmp);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 8)
            {
                //Blue team spawnpoint
                GameObject tmp = Instantiate(teamFlag2, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(tmp);
            }
        }

    }
}
