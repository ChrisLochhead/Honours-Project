using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class MapFinder : MonoBehaviour {

                                 // Use this for initialization
    void Start () {
        FindFiles();   
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
                            map.GetComponent<Map>().setMapSize(new Vector2(int.Parse(lines[0]), int.Parse(lines[1])));
                            // Debug.Log(map.GetComponent<Map>().getMapSize().x.ToString() + " , " + map.GetComponent<Map>().getMapSize().y.ToString());
                        }
                        else
                        {

                            if (lines.Length > 3)
                                map.GetComponent<Map>().addWallItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]), float.Parse(lines[3]));
                            else
                                map.GetComponent<Map>().addCoinItem(int.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
                        }
                        Debug.Log(text);
                        lineIterator++;
                    }
                }
            }

            maps.Add(map);
            // GameObject.Find("MapLoader").GetComponent<Dropdown>().options.Add(new Dropdown.OptionData() { text = map.name });
            fileIterator++;
            reader.Close();
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update () {
        // if (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == 2)
        // {
        int counter = 0;

            if (GameObject.Find("MapLoader"))
            {
            counter++;
                selectedMap = maps[GameObject.Find("MapLoader").GetComponent<Dropdown>().value];

                if (listAdded == false)
                {
                    for (int i = 0; i < maps.Count; i++)
                        GameObject.Find("MapLoader").GetComponent<Dropdown>().options.Add(new Dropdown.OptionData() { text = maps[i].name });

                    listAdded = true;
                }
            }
       // }
    }

    protected DirectoryInfo path = null;
    protected StreamReader reader = null;
    protected string text = " "; // assigned to allow first line to be read below

    public List<GameObject> maps;
    public GameObject map;
    public GameObject selectedMap;

    protected bool listAdded;
}
