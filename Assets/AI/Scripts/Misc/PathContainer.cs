using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class PathContainer : MonoBehaviour {

    public string mlagentsPath;
    public string anacondaPath;
    public string buildPath;

    public GameObject PathMenu;
    public GameObject HomeMenu;

    public TMP_InputField MLAgents;
    public TMP_InputField Anaconda;
    public TMP_InputField Build;

    public Button Done;
    public ImitationManager imitationManager;
    public FileUtilities fileutils;

    MapFinder mapfinder;
    DirectoryInfo pathFile;
    public bool pathsValid = false;
    public bool pathsSet = false;

    private void Start()
    {
        ReadPaths();

        if(GameObject.Find("MapFinder(Clone)"))
        mapfinder = GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>();
    }

    void PromptPathInput()
    {
        HomeMenu.SetActive(false);
        PathMenu.SetActive(true);        
    }

    public void AssignPaths()
    {
        mlagentsPath = MLAgents.text;
        buildPath = Build.text;
        anacondaPath = Anaconda.text;
        pathsValid = true;

        StreamWriter sr;
        sr = new StreamWriter(pathFile.FullName + "/paths.txt", false);
        sr.Write(mlagentsPath + "," + buildPath + "," + anacondaPath);
        sr.Close();
    }
            
    void ReadPaths()
    {
        pathFile = new DirectoryInfo(Application.dataPath);
        if (!Application.isEditor)
        {
            pathFile = pathFile.Parent;
            pathFile = pathFile.Parent;
        }
        else
        {
            pathFile = pathFile.Parent;
            pathFile = new DirectoryInfo(pathFile.FullName + "/Builds");
        }

        if (File.Exists(pathFile.FullName + "/paths.txt"))
        {
            StreamReader sr = new StreamReader(pathFile.FullName + "/paths.txt");
            string data = sr.ReadToEnd();

            string[] paths = Regex.Split(data, ",");
            if (paths[0] == "")
            {
                PromptPathInput();
            }
            else
            {
                mlagentsPath = paths[0];
                buildPath = paths[1];
                anacondaPath = paths[2];
                pathsValid = true;
            }
            MLAgents.text = paths[0];
            Build.text = paths[1];
            Anaconda.text = paths[2];

            sr.Close();
        }
    }

    private void Update()
    {
        if (!mapfinder)
        {
            if (GameObject.Find("MapFinder(Clone)"))
            {
                mapfinder = GameObject.Find("MapFinder(Clone)").GetComponent<MapFinder>();
            }
        }

        if (pathsValid && pathsSet == false)
        {
            Done.enabled = true;
            imitationManager.getDemos();
            fileutils.GetEnvironments();
            mapfinder.FindFiles();
            pathsSet = true;
        }
        else if (!pathsValid && !pathsSet)
            Done.enabled = false;
    }

}
