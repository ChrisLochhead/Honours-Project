using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;

public class CurriculumLearningManager : MonoBehaviour
{

    public FileUtilities fileutils;

    public TMP_InputField[] hyperParameterSettings;

    public TMP_Dropdown[] curriculumSessions;

    int sessionNumber = 0;
    public TMP_InputField numberOfSessions;

    public TMP_InputField[] modelSettings;

    public string testName = "default";

    public PathContainer paths;

    public TextMeshProUGUI trainingInfo;

    Process process;

    public void FillDropDowns()
    {
        fileutils.FillDropDowns(curriculumSessions, "Curriculum");
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetModelSettings()
    {
        testName = modelSettings[0].text;
    }

    public void RunCurriculumLearning()
    {
        //Set the name of the model
        SetModelSettings();

        //error checking
        int result;
        if(!int.TryParse(numberOfSessions.text, out result))
        {       
            fileutils.throwError("enter a valid value for the number of sessions.");
            return;
        }

        if(int.Parse(numberOfSessions.text) > 6 || int.Parse(numberOfSessions.text) < 1)
        {
            fileutils.throwError("enter a number between 1 and 6 for number of sessions.");
            return;
        }

        //Count how many dropdowns have been assigned compared
        //to how many sessions have been specified
        int counter = 0;
        foreach(TMP_Dropdown dropdown in curriculumSessions)
        {
            counter++;
            //UnityEngine.Debug.Log(numberOfSessions.text + " " + counter + " " + dropdown.value);
            if (dropdown.value <= 0 && int.Parse(numberOfSessions.text) >= counter)
            {
                fileutils.throwError("please select a map for every session.");
                return;
            }
        }

        //write hyperparameters to current maximum steps
        while (sessionNumber < int.Parse(numberOfSessions.text))
        {
            //Write the hyperparameters to the .yaml file and run learning
            fileutils.WriteHyperParameters(hyperParameterSettings, sessionNumber);
            RunReinforcementLearning();

            while (!process.HasExited)
            {
                //stall loop to run one training run at a time
            }
            sessionNumber++;
        }
    }
    public void RunReinforcementLearning()
    {
        //GUI info
        trainingInfo.text = "Training session: " + sessionNumber + "/" + numberOfSessions.text;

        process = fileutils.SetupAnaconda(process);
        //Set the map
        string mapName = curriculumSessions[sessionNumber].options[curriculumSessions[sessionNumber].value].text;

        //Allows for multiple sessions of one model
        if (File.Exists(paths.buildPath + "/Curriculum-Learning-Models/" + testName))
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id= Curriculum-Learning-Models/" + testName + " -- load --train");
        }
        else if (sessionNumber == 0)
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --train");
        }
        else
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --load --train");
        }
    }

    private void Update()
    {
        if (process != null)
        {
            if (process.HasExited && sessionNumber == int.Parse(numberOfSessions.text))
            {
                trainingInfo.text = "training completed sucessfully.";
                fileutils.MoveModelFiles(testName, "Curriculum");
                process = null;
            }
        }
    }
}
