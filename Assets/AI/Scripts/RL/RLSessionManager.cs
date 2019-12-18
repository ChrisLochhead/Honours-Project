using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

public class RLSessionManager : MonoBehaviour {

    public TMP_InputField[] hyperParameterSettings;

    public TMP_Dropdown [] trainingMaps;

    public TMP_InputField[] modelSettings;

    public string testName = "default";

    public PathContainer paths;

    public TextMeshProUGUI trainingInfo;

    Process process;
    public FileUtilities fileutils;
    void Awake () {
        DontDestroyOnLoad(this.gameObject);
    }

    public void FillDropDowns()
    {
        for (int i = 0; i < trainingMaps.Length; i++)
            trainingMaps[i].ClearOptions();

        fileutils.FillDropDowns(trainingMaps, "Reinforcement");
    }

    public void SetModelSettings()
    {
        testName = modelSettings[0].text;
    }

    public void RunReinforcementLearning()
    {
        //Finalise the selected hyperparameters
        fileutils.WriteHyperParameters(hyperParameterSettings);
        SetModelSettings();

        trainingInfo.text = "Training...";

        process = fileutils.SetupAnaconda(process);
        string mapName = trainingMaps[0].options[trainingMaps[0].value].text;
        UnityEngine.Debug.Log(paths.buildPath + "/Reinforcement-Learning-Models/" + testName + "/DRL-Brain.nn");

        if (!File.Exists(paths.buildPath + "/Reinforcement-Learning-Models/" + testName + "/DRL-Brain.nn"))
        {
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --train");
        }
        else
        {
            fileutils.DirectoryCopy(paths.buildPath + "/Reinforcement-Learning-Models/" + testName, paths.buildPath + "/models/" + testName + "-0", "");
            fileutils.DeleteDirectoryFiles(paths.buildPath + "/Reinforcement-Learning-Models/" + testName);

            foreach(string f in Directory.GetFiles(paths.buildPath + "/models/" + testName + "-0"))
            {
                FileInfo fInfo = new FileInfo(f);
                string[] nameContents = Regex.Split(fInfo.Name, "-B");
                if (nameContents[0] != "DRL") {
                    File.Copy(fInfo.FullName, paths.buildPath + "/summaries/" + fInfo.Name);
                    File.Delete(fInfo.FullName);
                }
            }
            process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=" + mapName + "/" + mapName + " --run-id=" + testName + " --load --train");
        }
        
    }

    private void Update()
    {
        if (process != null)
        {
            if (process.HasExited)
            {
                trainingInfo.text = "training completed sucessfully.";
                fileutils.MoveModelFiles(testName, "Reinforcement");
                process = null;
            }
        }
    }
}
