using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

    //Recording of image path
    public string imageTexturePath;
    //List of items
    private List<GameObject> mapItems = new List<GameObject>();

    public void addWallItem(int t, float px, float py, float r, GameObject p)
    {
        //Create a new empty object, assign it as a wall and add a wall component
        GameObject temp = new GameObject();
        temp.name = "Wall";
        temp.AddComponent<Wall>();

        //Set its position, type and rotation
        temp.GetComponent<Wall>().pos.x = px;
        temp.GetComponent<Wall>().pos.y = py;
        temp.GetComponent<Wall>().rot = r;
        temp.GetComponent<Wall>().type = t;

        //Parent the object to it's map
        temp.transform.parent = p.transform;

        //Add it to map items
        mapItems.Add(temp);
    }

    public void addCoinItem(int t, float px, float py, GameObject p)
    {
        //initialise a gameobject to be a coin
        GameObject temp = new GameObject();
        temp.name = "Coin";
        temp.AddComponent<Coin>();

        //Assign position and type
        temp.GetComponent<Coin>().pos.x = px;
        temp.GetComponent<Coin>().pos.y = py;
        temp.GetComponent<Coin>().type = t;

        //Parent the object
        temp.transform.parent = p.transform;

        //Add it to map items
        mapItems.Add(temp);
    }

    public List<GameObject> GetMapItems()
    {
        return mapItems;
    }

}
