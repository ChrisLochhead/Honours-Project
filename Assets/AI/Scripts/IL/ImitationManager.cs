using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImitationManager : MonoBehaviour {

    //Contains information for the demonstrations
    //the model will follow
    public TMP_Dropdown demoInputs;
    public TMP_Dropdown[] mapInputs;

    string testName;
    string map;

    //Reference to the path container
    public PathContainer paths;

    //Contains menu hyperparameters and model name
    public TMP_InputField[] hyperParameterSettings;
    public TMP_InputField modelName;

    //Extension to file utilities
    FileUtilities fileutils;

    //Anaconda program
    Process process;

    //For debug mode
    public Toggle isDebug;

    void Start () {
        if (paths.pathsValid)
        {
            getDemos();
        }
        fileutils = this.GetComponent<FileUtilities>();
	}

    public void fillDropDowns()
    {
        for(int i = 0; i < mapInputs.Length; i++) 
        mapInputs[i].ClearOptions();
       
        fileutils.FillDropDowns(mapInputs, "Imitation");
    }

    public void getDemos()
    {
        //Find all demonstrations and add them to the dropdown menu
        demoInputs.ClearOptions();

        DirectoryInfo dir = new DirectoryInfo(paths.buildPath + "/Demonstrations/");

        foreach(FileInfo file in dir.GetFiles())
        {
            demoInputs.options.Add(new TMP_Dropdown.OptionData() { text = file.Name });
        }

        //needs to reset after clearing for some reason
        demoInputs.value = 1;
        demoInputs.value = 0;

    }

    public void ExitDemonstration()
    {
        SceneManager.LoadScene(0);
    }

    public void DemoTraining()
    {
        SceneManager.LoadScene(4);
    }

    void SetModelSettings()
    {
        testName = modelName.text;
        map = mapInputs[0].options[mapInputs[0].value].text;
    }

    public void ImitationLearning()
    {
        SetModelSettings();
        //Start anaconda, write hyperparameters and start training
        process = fileutils.SetupAnaconda(process);

        //Finalise the selected hyperparameters
        int startStep = 0;

        if (File.Exists(paths.buildPath + "/Imitation-Learning-Models/" + testName + "/IL-Brainsteps.txt"))
        {
            FileInfo f = new FileInfo(paths.buildPath + "/Imitation-Learning-Models/" + testName + "/IL-Brainsteps.txt");
            StreamReader sr = f.OpenText();
            startStep = int.Parse(sr.ReadLine());
            sr.Close();
        }
        else if (File.Exists(paths.buildPath + "/models/IL-Brainsteps.txt"))
        {
            FileInfo f = new FileInfo(paths.buildPath + "/models/IL-Brainsteps.txt");
            StreamReader sr = f.OpenText();
            startStep = int.Parse(sr.ReadLine());
            sr.Close();
        }

        UnityEngine.Debug.Log(startStep);
        fileutils.WriteHyperParameters(hyperParameterSettings, 0 ,true, demoInputs, startStep);

        if (!Directory.Exists(paths.buildPath + "/Imitation-Learning-Models/" + testName))
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml --env=" + map + "/" + map + " --run-id=" + modelName.text + " --train");
            if (!isDebug.isOn)
                process.StandardInput.WriteLine("exit");
        }
        else
        {
            //Move files back into models folder
            fileutils.DirectoryCopy(paths.buildPath + "/Imitation-Learning-Models/" + testName, paths.buildPath + "/models/" + testName + "-0", "");
            fileutils.DeleteDirectoryFiles(paths.buildPath + "/Imitation-Learning-Models/" + testName);

            foreach (string f in Directory.GetFiles(paths.buildPath + "/models/" + testName + "-0"))
            {
                FileInfo fInfo = new FileInfo(f);
                string[] nameContents = Regex.Split(fInfo.Name, "-B");
                if (nameContents[0] != "IL")
                {
                    File.Copy(fInfo.FullName, paths.buildPath + "/summaries/" + fInfo.Name);
                    File.Delete(fInfo.FullName);
                }
            }

            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml --env=" + map + "/" + map  + " --run-id=" + modelName.text + " --load --train");
            if (!isDebug.isOn)
                process.StandardInput.WriteLine("exit");
        }

        //Update the starting step value
        int newStartingStep = int.Parse(hyperParameterSettings[3].text) + startStep;
        FileStream fs = File.Create(paths.buildPath + "/models/IL-Brainsteps.txt");

        fs.Close();
        StreamWriter sw = new StreamWriter(paths.buildPath + "/models/IL-Brainsteps.txt");
        sw.Write(newStartingStep);
        sw.Close();
    }

    private void Update()
    {
        //Wait for the process to finish then save and move the files
        if (process != null)
        {
            if (process.HasExited)
            {
                fileutils.MoveModelFiles(modelName.text, "Imitation");
                process = null;
            }
        }
    }
}
