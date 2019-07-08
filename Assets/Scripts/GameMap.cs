using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : NetworkBehaviour {

    private List<GameObject> mapInfo;

    public GameObject ground;

    public GameObject redWall;
    public GameObject orangeWall;
    public GameObject greenWall;
    public GameObject greyWall;

    public GameObject goldCoin;
    public GameObject silverCoin;
    public GameObject bronzeCoin;

    public GameObject teamFlag1;
    public GameObject teamFlag2;

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
        //create the ground 9X9 cube 
        GameObject environmentObj = (GameObject)Instantiate(ground, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(200, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(-200, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(0, 200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(0, -200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(200, -200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(200, 200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(-200, -200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);
        environmentObj = (GameObject)Instantiate(ground, new Vector3(-200, 200, 0), Quaternion.identity);
        NetworkServer.Spawn(environmentObj);

        //Draw the border walls
        GameObject borderObj = (GameObject)Instantiate(borderWall, new Vector3(300, 0, -10), Quaternion.identity);
        NetworkServer.Spawn(borderObj);
        borderObj = (GameObject)Instantiate(borderWall, new Vector3(-300, 0, -10), Quaternion.identity);
        NetworkServer.Spawn(borderObj);
        borderObj = (GameObject)Instantiate(borderWall, new Vector3(0, -300, -10), Quaternion.identity * Quaternion.Euler(0, 0, 90));
        NetworkServer.Spawn(borderObj);
        borderObj = (GameObject)Instantiate(borderWall, new Vector3(0, 300, -10), Quaternion.identity * Quaternion.Euler(0, 0, 90));
        NetworkServer.Spawn(borderObj);

        //get the size and then draw the floor

        for (int i = 0; i < mapInfo.Count; i++)
        {
            if (mapInfo[i].GetComponent<Wall>())
            {
                if (mapInfo[i].GetComponent<Wall>().type == 0 && isServer)
                {
                    GameObject spawnObj = (GameObject)Instantiate(redWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 1 && isServer)
                {
                    GameObject spawnObj = (GameObject)Instantiate(orangeWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 2 && isServer)
                {
                    GameObject spawnObj = (GameObject)Instantiate(greenWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
                if (mapInfo[i].GetComponent<Wall>().type == 3 && isServer)
                {
                    GameObject spawnObj = (GameObject)Instantiate(greyWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, -5), Quaternion.identity * Quaternion.Euler(0, 0, mapInfo[i].GetComponent<Wall>().rot));
                    NetworkServer.Spawn(spawnObj);
                }
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 4 && isServer)
            {
                GameObject spawnObj = (GameObject)Instantiate(goldCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 5 && isServer)
            {
                GameObject spawnObj = (GameObject)Instantiate(silverCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 6 && isServer)
            {
                GameObject spawnObj = (GameObject)Instantiate(bronzeCoin, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(spawnObj);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 7)
            {
                GameObject tmp = Instantiate(teamFlag1, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(tmp);
            }
            else
                if (mapInfo[i].GetComponent<Coin>().type == 8)
            {
                GameObject tmp = Instantiate(teamFlag2, new Vector3(mapInfo[i].GetComponent<Coin>().pos.x, mapInfo[i].GetComponent<Coin>().pos.y, -5), Quaternion.identity);
                NetworkServer.Spawn(tmp);
            }
        }

    }
}
