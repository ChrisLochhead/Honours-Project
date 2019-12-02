﻿using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

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

    void Start () {
        if (paths.pathsValid)
        {
            getDemos();
        }
        fileutils = this.GetComponent<FileUtilities>();
	}

    public void fillDropDowns()
    {
        fileutils.FillDropDowns(mapInputs);
    }

    public void getDemos()
    {
        //Find all demonstrations and add them to the dropdown menu
        DirectoryInfo dir = new DirectoryInfo(paths.buildPath + "/Demonstrations/");

        foreach(FileInfo file in dir.GetFiles())
        {
            demoInputs.options.Add(new TMP_Dropdown.OptionData() { text = file.Name });
        }

    }

    public void ExitDemonstration()
    {
        SceneManager.LoadScene(0);
    }

    public void DemoTraining()
    {
        SceneManager.LoadScene(5);
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

        fileutils.WriteHyperParameters(hyperParameterSettings, 0 ,true, demoInputs);

        if (!File.Exists(paths.buildPath + "/Imitation-Learning-Models/" + testName))
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml --env=" + map + "/" + map + " --run-id=" + modelName.text + " --train");
        }
        else
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml --env=" + map + "/" + map + " --run-id=Imitation-Learning-Models/" + modelName.text + " --load --train");
        }
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