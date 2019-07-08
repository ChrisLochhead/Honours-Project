using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    public string imageTexturePath;

    public void addWallItem(int t, float px, float py, float r, GameObject p)
    {
        GameObject temp = new GameObject();
        temp.name = "Wall";
        temp.AddComponent<Wall>();

        temp.GetComponent<Wall>().pos.x = px;
        temp.GetComponent<Wall>().pos.y = py;
        temp.GetComponent<Wall>().rot = r;
        temp.GetComponent<Wall>().type = t;

        if(mapItems == null)
            mapItems = new List<GameObject>();

        temp.transform.parent = p.transform;

        mapItems.Add(temp);
    }

    public void addCoinItem(int t, float px, float py, GameObject p)
    {
        GameObject temp = new GameObject();
        temp.name = "Coin";
        temp.AddComponent<Coin>();

        temp.GetComponent<Coin>().pos.x = px;
        temp.GetComponent<Coin>().pos.y = py;
        temp.GetComponent<Coin>().type = t;

        if (mapItems == null)
            mapItems = new List<GameObject>();

        temp.transform.parent = p.transform;

        mapItems.Add(temp);
    }

    public List<GameObject> GetMapItems()
    {
        return mapItems;
    }

    private List<GameObject> mapItems;
}
