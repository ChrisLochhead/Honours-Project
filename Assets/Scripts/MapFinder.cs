using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class MapFinder : NetworkBehaviour {

                                 // Use this for initialization
    void Start () {
        FindFiles();   

        if(GameObject.Find("MainMenu"))
        {
            isMainMenu = true;
        }
    }

    public void FindFiles()
    {
        path = new DirectoryInfo("D:/repos/Honours Project/Code/Honours-Project/Assets/Maps");
        FileInfo[] info = path.GetFiles("*.txt");
        int fileIterator = 0;

        selectedMap.AddComponent<Map>();
        maps = new List<GameObject>();

        foreach (FileInfo f in info)
        {

            map = new GameObject();
            map.transform.parent = this.transform;
            map.AddComponent<Map>();

            int lineIterator = 0;
            reader = f.OpenText();
            text = "";
            while (text != null)
            {
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
                            map.GetComponent<Map>().imageTexturePath = lines[0];
                        }
                        else
                        {

                            if (lines.Length > 3)
                                map.GetComponent<Map>().addWallItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]), map);
                            else
                                map.GetComponent<Map>().addCoinItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), map);
                        }
                        lineIterator++;
                    }
                }
            }

            maps.Add(map);
            fileIterator++;
            reader.Close();
            listAdded = false;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update () {

        if (GameObject.Find("MapLoader"))
        {
            Dropdown tmp = GameObject.Find("MapLoader").GetComponent<Dropdown>();
            if (listAdded == false)
            {
                tmp.options.Clear();

                if(isMainMenu)
                tmp.options.Add(new Dropdown.OptionData() { text = "" });

                for (int i = 0; i < maps.Count; i++)
                    tmp.options.Add(new Dropdown.OptionData() { text = maps[i].name });

                listAdded = true;
            }

            selectedMap = maps[GameObject.Find("MapLoader").GetComponent<Dropdown>().value];
            mapNumber = GameObject.Find("MapLoader").GetComponent<Dropdown>().value;
        }
    }

    public bool isMainMenu = false;
    public bool spawned = false;

    protected DirectoryInfo path = null;
    protected StreamReader reader = null;
    protected string text = " "; // assigned to allow first line to be read below

    public List<GameObject> maps;
    public GameObject map;

    [SyncVar]
    public int mapNumber = 0;

    public GameObject selectedMap;

    protected bool listAdded;
}
