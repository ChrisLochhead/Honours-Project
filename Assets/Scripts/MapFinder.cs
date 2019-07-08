using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

public class MapFinder : NetworkBehaviour {

    //References to each scenes maploaders
    public GameObject[] mapLoaderDropdowns;
    private int currentNoOfDropdowns = 0;

    //For reading the files
    protected DirectoryInfo path = null;
    protected StreamReader reader = null;
    protected string text = " ";

    //List to contain the maps that have been loaded
    public List<GameObject> maps;
    //For each map as it is loaded
    public GameObject map;

    //So clients can determine which map to load
    [SyncVar]
    public int mapNumber = 0;

    //Reference for the currently selected map
    public GameObject selectedMap;

    void Start () {
        //Initialise existing maps
        FindFiles();

    }

    public void FindFiles()
    {
        //Get the path to the map folder
        path = new DirectoryInfo(Application.dataPath + "/Maps");
        //Get all .txt files to avoid the .meta files
        FileInfo[] info = path.GetFiles("*.txt");

        //Initialise a list of maps
        maps = new List<GameObject>();

        //Iterate through each file
        foreach (FileInfo f in info)
        {
            //Initialise a new map
            map = new GameObject();
            map.transform.parent = this.transform;
            map.AddComponent<Map>();

            //Record which line is being read
            int lineIterator = 0;

            //Open the file
            reader = f.OpenText();
            text = "";

            //While file has a valid line
            while (text != null)
            {
                //Read the line into a text object
                text = reader.ReadLine();
                if (text != null)
                {
                    //Split the line into an array of strings, seperated by ","
                    string[] lines = Regex.Split(text, ",");
                    if (lines != null)
                    {
                        //If the first line, assign this line as the name
                        if (lineIterator == 0)
                        {
                            map.name = lines[0];
                        }
                        else if (lineIterator == 1)
                        {
                            //If the second line, this is the path to the texture
                            map.GetComponent<Map>().imageTexturePath = lines[0];
                        }
                        else
                        {
                            //The rest will either be walls or coins (or spawn points, represented by coins)
                            if (lines.Length > 3)
                                map.GetComponent<Map>().addWallItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]), map);
                            else
                                map.GetComponent<Map>().addCoinItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), map);
                        }
                        //Iterate through the lines
                        lineIterator++;
                    }
                }
            }
            //Once the file is finished, add the map to the list
            maps.Add(map);
            Debug.Log(map.GetComponent<Map>().GetMapItems().Count);
            //Close the reader
            reader.Close();
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update () {

        //Find this scenes map loaders
        mapLoaderDropdowns = GameObject.FindGameObjectsWithTag("MapLoader");
        if (mapLoaderDropdowns.Length > 0 && currentNoOfDropdowns != mapLoaderDropdowns.Length)
        {
            Debug.Log("stage 2");
            foreach (GameObject d in mapLoaderDropdowns)
            {
                Dropdown tmp = d.GetComponent<Dropdown>();
                Debug.Log("stage 3");
                //Clear the list
                tmp.options.Clear();

                //Add the list in
                for (int i = 0; i < maps.Count; i++)
                    tmp.options.Add(new Dropdown.OptionData() { text = maps[i].name });

            }
        }
        currentNoOfDropdowns = mapLoaderDropdowns.Length;
        if (mapLoaderDropdowns.Length > 0)
        {
            selectedMap = maps[mapLoaderDropdowns[0].GetComponent<Dropdown>().value];
            mapNumber = mapLoaderDropdowns[0].GetComponent<Dropdown>().value;
        }
    }
}
