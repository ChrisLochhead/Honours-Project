﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class MapFinder : MonoBehaviour {

                                 // Use this for initialization
    void Start () {

        path = new DirectoryInfo("D:/repos/Honours Project/Code/Honours-Project/Assets/Maps");
        FileInfo[] info = path.GetFiles("*.*");
        int fileIterator = 0;

        map.AddComponent<Map>();
        maps = new List<GameObject>();

        foreach (FileInfo f in info)
        {

            int lineIterator = 0;
            reader = f.OpenText();
           while(text != null)
           {
                bool isCoins = false;
                text = reader.ReadLine();
                if (text != null)
                {
                    string[] lines = Regex.Split(text, ",");
                    if (lines != null)
                    {
                        if (lineIterator == 0)
                        {
                            map.name = lines[0];
                        }
                        else if (lineIterator == 1)
                        {
                            map.GetComponent<Map>().setMapSize(new Vector2(int.Parse(lines[0]), int.Parse(lines[1])));
                            // Debug.Log(map.GetComponent<Map>().getMapSize().x.ToString() + " , " + map.GetComponent<Map>().getMapSize().y.ToString());
                        }
                        else {

                            if(lines[0] == "-----")
                            {
                                isCoins = true;
                            }
                            if (isCoins == false)
                            map.GetComponent<Map>().addWallItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]));
                            else
                            
                        }



                        Debug.Log(text);
                        lineIterator++;
                    }
                }
           }

            maps.Add(map);
            fileIterator++;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
   
    protected DirectoryInfo path = null;
    protected StreamReader reader = null;
    protected string text = " "; // assigned to allow first line to be read below

    public List<GameObject> maps;
    public GameObject map;
}
