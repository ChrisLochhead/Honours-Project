using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class ImitationManager : MonoBehaviour {

    public TMP_Dropdown demoInputs;
    public PathContainer paths;

    public TMP_InputField[] hyperParameterSettings;
    public TMP_InputField modelName;

    //batch size (buffer size always 10*)
    //hidden units
    //learning rate
    //max steps
    //number of epochs
    //number of layers

    Process process;

    // Use this for initialization
    void Start () {
        getDemos();
	}

    void getDemos()
    {
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

    public void WriteHyperParameters()
    {
        //Error check the hyperparameters
        for (int i = 0; i < hyperParameterSettings.Length; i++)
        {
            //check no fields are empty
            if (hyperParameterSettings[i].text == "")
                UnityEngine.Debug.Log("empty field detected");

            //Check no fields are non numeric
            float n = 0;
            if (float.TryParse(hyperParameterSettings[i].text, out n) == false)
                UnityEngine.Debug.Log("non-numeric field detected");

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
                "       gail: "+
                "           strength: 1.0 " +
                "           gamma: 0.99" +
                "           demo_path: Demonstrations/" + demoInputs.options[demoInputs.value].text;

        StreamWriter sr;
        sr = new StreamWriter(paths.buildPath + "/TrainerConfiguration/gail_config.yaml", false);

        sr.Write(serialisedData);
        sr.Close();
    }
    void SetupAnaconda()
    {
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

        //Change to build directory path
        process.StandardInput.WriteLine(@"cd " + paths.buildPath);
    }

    void ImitationLearning()
    {
        SetupAnaconda();
        WriteHyperParameters();

        process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml 
        --env=training_env_small/training_env_small --run-id=" + modelName.text  + " --train");
    }

    private void Update()
    {

    }
}
