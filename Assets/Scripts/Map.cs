using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    //// Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setMapSize(Vector2 s)
    {
        mapSize = s;
    }

    public Vector2 getMapSize()
    {
        return mapSize;
    }

    public void addWallItem(int t, float px, float py, float r)
    {
        GameObject temp = new GameObject();

        temp.AddComponent<Wall>();

        temp.GetComponent<Wall>().pos.x = px;
        temp.GetComponent<Wall>().pos.y = py;
        temp.GetComponent<Wall>().rot = r;
        temp.GetComponent<Wall>().type = t;

        if(mapItems == null)
            mapItems = new List<GameObject>();

        mapItems.Add(temp);
    }

    public void addCoinItem(int t, float px, float py)
    {
        GameObject temp = new GameObject();
        temp.AddComponent<Coin>();

        temp.GetComponent<Coin>().pos.x = px;
        temp.GetComponent<Coin>().pos.y = py;
        temp.GetComponent<Coin>().type = t;

        if (mapItems == null)
            mapItems = new List<GameObject>();

        mapItems.Add(temp);
    }

    public List<GameObject> GetMapItems()
    {
        return mapItems;
    }

    private Vector2 mapSize;
    private List<GameObject> mapItems;
}
