using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    //// Use this for initialization
    void Start()
    {
        mapItems = new List<GameObject>();
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
      //  Instantiate(temp, transform.position, Quaternion.identity);
        temp.AddComponent<Wall>();

        temp.GetComponent<Wall>().pos.x = px;
        temp.GetComponent<Wall>().pos.y = py;
        temp.GetComponent<Wall>().rot = r;
        temp.GetComponent<Wall>().type = t;

        mapItems.Add(temp);
    }

    public void addCoinItem(int t, float px, float py)
    {
        GameObject temp = new GameObject();
    //    Instantiate(temp, transform.position, Quaternion.identity);
        temp.AddComponent<Coin>();

        temp.GetComponent<Coin>().pos.x = px;
        temp.GetComponent<Coin>().pos.y = py;
        temp.GetComponent<Coin>().type = t;

        mapItems.Add(temp);
    }

    private Vector2 mapSize;
    private List<GameObject> mapItems;
}
