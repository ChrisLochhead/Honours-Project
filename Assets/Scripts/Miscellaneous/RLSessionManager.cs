using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class RLSessionManager : MonoBehaviour {

    public InputField[] hyperParameterSettings;
    //batch size (buffer size always 10*)
    //hidden units
    //lambda
    //learning rate
    //max steps
    //isNormalised
    //number of epochs
    //number of layers
    //summary frequency

    public InputField[] modelSettings;
    //name
    //kill reward
    //death penalty
    //collision penalty
    //render graphics
    public int testNumber = 0;
    bool graphics = true;

    private void Start()
    {
        //get current test number (test numbers start at 1 for simplicity)
        int testNumber = System.IO.Directory.GetDirectories(@"D:\repos\Honours Project\Code\Honours-Project\Assets\AI\models").Length + 1;
    }

    void Awake () {
        DontDestroyOnLoad(this);
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

        StreamWriter sr = new StreamWriter(Application.dataPath + "/AI/exe_config.txt", false);
        sr.Write(serialisedData);

    }

    public void GetModelSettings()
    {

    }

    public void OpenAnacondaPrompt()
    {
        SceneManager.LoadScene(4);

        //Sets up command prompt
        ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.RedirectStandardOutput = false;
        processStartInfo.UseShellExecute = false;

        //Initialises anaconda
        Process process = Process.Start(processStartInfo);
        process.StandardInput.WriteLine(@"cd /d D:/repos/Honours Project/Code/Honours-Project/Assets/AI");
        process.StandardInput.WriteLine(@" D:\\Anaconda\\Scripts\\activate.bat D:\\Anaconda");
        process.StandardInput.WriteLine(@"activate tensorflow-env");

        //begins training session
        process.StandardInput.WriteLine(@"mlagents-learn exe_config.yaml --run-id=" + testNumber + " --load --train");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
