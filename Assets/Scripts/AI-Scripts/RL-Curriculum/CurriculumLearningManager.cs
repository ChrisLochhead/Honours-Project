using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class CurriculumLearningManager : MonoBehaviour
{

    public FileUtilities fileutils;

    public TMP_InputField[] hyperParameterSettings;

    public TMP_Dropdown[] curriculumSessions;

    public Toggle debugMode;

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
        int startingStep = 0;
        //write hyperparameters to current maximum steps
        while (sessionNumber < int.Parse(numberOfSessions.text))
        {
            //If this is an existing file
            if (File.Exists(paths.buildPath + "/Curriculum-Learning-Models/" + testName + "/CL-Brainsteps.txt"))
            {
                FileInfo f = new FileInfo(paths.buildPath + "/Curriculum-Learning-Models/" + testName + "/CL-Brainsteps.txt");
                StreamReader sr = f.OpenText();
                startingStep = int.Parse(sr.ReadLine());
                sr.Close();
            }else if(File.Exists(paths.buildPath + "/models/CL-Brainsteps.txt"))
            {
                FileInfo f = new FileInfo(paths.buildPath + "/models/CL-Brainsteps.txt");
                StreamReader sr = f.OpenText();
                startingStep = int.Parse(sr.ReadLine());
                sr.Close();
            }

            //Write the hyperparameters to the .yaml file
            fileutils.WriteHyperParameters(hyperParameterSettings, sessionNumber, false, null, startingStep);

            //Update the starting step value
            int newStartingStep = int.Parse(hyperParameterSettings[3].text) + startingStep;
            FileStream fs = File.Create(paths.buildPath + "/models/CL-Brainsteps.txt");
            
            fs.Close();
            StreamWriter sw = new StreamWriter(paths.buildPath + "/models/CL-Brainsteps.txt");
            sw.Write(newStartingStep);
            sw.Close();

            //Run learning
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
        UnityEngine.Debug.Log(paths.buildPath + "/Curriculum-Learning-Models/" + testName + "/CL-Brain.nn");
        if (File.Exists(paths.buildPath + "/Curriculum-Learning-Models/" + testName + "/CL-Brain.nn"))
        {
            fileutils.DirectoryCopy(paths.buildPath + "/Curriculum-Learning-Models/" + testName, paths.buildPath + "/models/" + testName + "-0", "");
            fileutils.DeleteDirectoryFiles(paths.buildPath + "/Curriculum-Learning-Models/" + testName);

            foreach (string f in Directory.GetFiles(paths.buildPath + "/models/" + testName + "-0"))
            {
                FileInfo fInfo = new FileInfo(f);
                string[] nameContents = Regex.Split(fInfo.Name, "-B");
                if (nameContents[0] != "CL")
                {
                    File.Copy(fInfo.FullName, paths.buildPath + "/summaries/" + fInfo.Name);
                    File.Delete(fInfo.FullName);
                }
            }

            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --load --train");
        }
        else if (sessionNumber == 0)
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --train");
        }
        else
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --load --train");
        }
        if (!debugMode.isOn)
            process.StandardInput.WriteLine("exit");
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
                sessionNumber = 0;
            }
        }
    }
}
