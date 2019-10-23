using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class RLSessionManager : MonoBehaviour {

    public TMP_InputField[] hyperParameterSettings;
    //batch size (buffer size always 10*)
    //hidden units
    //learning rate
    //max steps
    //number of epochs
    //number of layers

    public TMP_Dropdown trainingMaps;
    public int selectedMap;

    public TMP_InputField[] modelSettings;
    public string[] stringModelSettings;
    public float[] floatModelSettings;
    //name
    //kill reward
    //death penalty
    //collision penalty
    //render graphics
    public string testName = "default";

    //public string modelPath;
    //public string mlagentsPath;
    //public string anacondaPath;

    //public string path;
    public PathContainer paths;

    public TextMeshProUGUI trainingInfo;

    Process process;

    //StreamReader filereader = null;

    //public List<float> candidatescores;

    //int highestIndex = 0;
    //float highestIndexValue = 0.0f;

    //int generation = 0;
    //int candidateNo = 0;

    //public int noOfCandidates = 0;

    void Awake () {
        DontDestroyOnLoad(this.gameObject);
    }

    void throwError(string text)
    {
        UnityEngine.Debug.Log(text);
    }

    public void WriteHyperParameters()
    {
        //Error check the hyperparameters
        for (int i = 0; i < hyperParameterSettings.Length; i++)
        {
            //check no fields are empty
            if (hyperParameterSettings[i].text == "")
                throwError("missing field");

            //Check no fields are non numeric
            float n = 0;
            if (float.TryParse(hyperParameterSettings[i].text, out n) == false)
                throwError("non numeric field detected");

        }

     //Correct any error with normalise setting
     if (float.Parse(hyperParameterSettings[5].text) != 0 && float.Parse(hyperParameterSettings[5].text) != 1)
         hyperParameterSettings[5].text = "0";

        //Write it to a string for the .yaml file
        string serialisedData =
                "default: \n" +
                "   trainer: ppo \n" +
                "   batch_size: " + hyperParameterSettings[0].text + "\n" +
                "   beta: 1.0e-2 \n" +
                "   buffer_size: " + float.Parse(hyperParameterSettings[0].text) * 10 + "\n" +
                "   epsilon: 0.15 \n" +
                "   hidden_units: " + hyperParameterSettings[1].text + "\n" +
                "   lambd: 0.92 \n" +
                "   learning_rate: " + hyperParameterSettings[2].text + "\n" +
                "   max_steps: " + hyperParameterSettings[3].text + "\n" +
                "   memory_size: 256 \n" +
                "   normalize: false \n" +
                "   num_epoch: " + hyperParameterSettings[4].text + "\n" +
                "   num_layers: " + hyperParameterSettings[5].text + "\n" +
                "   time_horizon: 64 \n" +
                "   sequence_length: 64 \n" +
                "   summary_freq: 1250 \n" +
                "   use_recurrent: false \n" +
                "   vis_encode_type: simple \n" +
                "   reward_signals: \n" +
                "       extrinsic: \n" +
                "           strength: 1.0 \n" +
                "           gamma: 0.99 \n" +
                "       curiosity: \n" +
                "           strength: 0.01 \n" +
                "           gamma: 0.99 \n" +
                "           encoding_size: 256";
        StreamWriter sr;
        if (Application.isEditor)
            sr = new StreamWriter(Application.dataPath + "/AI/exe_config.yaml", false);
        else
            sr = new StreamWriter(paths.buildPath + "/TrainerConfiguration/exe_config.yaml", false);

        sr.Write(serialisedData);
        sr.Close();
    }

    public void SetModelSettings()
    {
        //Perform error check first

        testName = stringModelSettings[0];
        selectedMap = trainingMaps.value;
        //name
        //kill reward
        //death penalty
        //collision penalty
        //render graphics
    }
    
    public void RunReinforcementLearning()
    {
        //Finalise the selected hyperparameters
        WriteHyperParameters();
        SetModelSettings();

        trainingInfo.text = "Training...";

        //Sets up command prompt
        ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardOutput = false;
        processStartInfo.UseShellExecute = false;

        //Initialises anaconda
        process = Process.Start(processStartInfo);
        process.StandardInput.WriteLine(@"cd " + paths.mlagentsPath);
        process.StandardInput.WriteLine(@paths.anacondaPath);
        process.StandardInput.WriteLine(@"activate tensorflow-env");

        //begins training session
        if (Application.isEditor)
        {

            if (!File.Exists(Application.dataPath + "AI/models/" + testName + "-0"))
                process.StandardInput.WriteLine(@"mlagents-learn exe_config.yaml --run-id=" + testName + "--train");
            else
                process.StandardInput.WriteLine(@"mlagents-learn exe_config.yaml --run-id=" + testName + " --load --train");

            UnityEngine.Debug.Log("To train in editor, stop the compile without closing the anaconda window, navigate to the desired training room and click play.");
        }
        else
        {
            process.StandardInput.WriteLine(@"cd " + paths.buildPath);

            if (!File.Exists(paths.buildPath + "/models/" + testName + "-0"))
            {
                if(selectedMap == 0)
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_small/training_env_small --run-id=" + testName + " --train");
                else
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_large/training_env_large --run-id=" + testName + " --train");
            }
            else
            {
                if (selectedMap == 0)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_small/training_env_small --run-id=" + testName + " --load --train");
                }else
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml  --env=training_env_large/training_env_large --run-id=" + testName + " --load --train");
                }
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
                process = null;
            }
        }
    }
}
