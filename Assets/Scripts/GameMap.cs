using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour {

    private List<GameObject> mapInfo;
    Vector2 mapSize;

    public GameObject ground;
    public GameObject redWall;
    public GameObject orangeWall;
    public GameObject greenWall;
    public GameObject goldCoin;
    public GameObject silverCoin;
    public GameObject bronzeCoin;

    // Use this for initialization
    void Start () {
        mapInfo = new List<GameObject>();
        mapInfo = GameObject.Find("PersistentObject").GetComponent<MapFinder>().selectedMap.GetComponent<Map>().GetMapItems();
        mapSize = GameObject.Find("PersistentObject").GetComponent<MapFinder>().selectedMap.GetComponent<Map>().getMapSize();
        InitMap();
	}

    void InitMap()
    {
        //create the ground and set its scale
        Instantiate(ground, new Vector3(0, 0, 0), Quaternion.identity);
        ground.transform.localScale = new Vector3(mapSize.x, 1, mapSize.y);
        //get the size and then draw the floor
        for (int i = 0; i < mapInfo.Count; i++) {
            if (mapInfo[i].GetComponent<Wall>())
            {
                if (mapInfo[i].GetComponent<Wall>().type == 0)
                {
                    Instantiate(redWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, 0), Quaternion.identity);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 1)
                {
                    Instantiate(orangeWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, 0), Quaternion.identity);
                }
                else
                if (mapInfo[i].GetComponent<Wall>().type == 2)
                {
                    Instantiate(greenWall, new Vector3(mapInfo[i].GetComponent<Wall>().pos.x, mapInfo[i].GetComponent<Wall>().pos.y, 0), Quaternion.identity);
                }
            }else
                if (mapInfo[i].GetComponent<Coin>())
            {
                Debug.Log("My nama jeff");
            }
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
