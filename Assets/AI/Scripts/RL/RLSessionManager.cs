using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;

public class RLSessionManager : MonoBehaviour {

    public TMP_InputField[] hyperParameterSettings;

    public TMP_Dropdown trainingMaps;
    public int selectedMap;

    public TMP_InputField[] modelSettings;

    public string testName = "default";

    public PathContainer paths;

    public TextMeshProUGUI trainingInfo;

    Process process;
    public FileUtilities fileutils;
    void Awake () {
        DontDestroyOnLoad(this.gameObject);
    }    

    public void SetModelSettings()
    {
        testName = modelSettings[0].text;
        selectedMap = trainingMaps.value;
    }

    public void RunReinforcementLearning()
    {
        //Finalise the selected hyperparameters
        fileutils.WriteHyperParameters(hyperParameterSettings);
        SetModelSettings();

        trainingInfo.text = "Training...";

        process = fileutils.SetupAnaconda(process);

        if (!File.Exists(paths.buildPath + "/models/" + testName + "-0"))
        {
            if (selectedMap == 0)
                process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_small/training_env_small --run-id=" + testName + " --train");
            else
                process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_large/training_env_large --run-id=" + testName + " --train");
        }
        else
        {
            if (selectedMap == 0)
            {
                process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_small/training_env_small --run-id=" + testName + " --load --train");
            }
            else
            {
                process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_large/training_env_large --run-id=" + testName + " --load --train");
            }
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
