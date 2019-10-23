﻿using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TMPro;
using UnityEngine.SceneManagement;

public class ELSessionManager : MonoBehaviour {

    Process process;

    StreamReader filereader = null;

    public List<float> candidatescores;

    int highestIndex = 0;
    float highestIndexValue = 0.0f;

    [SerializeField]
    int generation = 0;

    int stepsPerGeneration = 50000;
    int numberOfGenerations = 3;
    int numberOfCandidates = 0;
    int candidateNo = 0;



    public PathContainer paths;

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

    int currentCandidateBeingTrained = 0;
    //name
    //kill reward
    //death penalty
    //collision penalty
    //render graphics
    public string testName = "default";

    // Use this for initialization
    void Start() {

    }

    string FindCandidate()
    {
        //Get the path to all the model summaries
        DirectoryInfo dirPath = new DirectoryInfo(Application.dataPath + "/AI/summaries/");
        //Get all .csv files it can find
        FileInfo[] info = dirPath.GetFiles("*.csv", SearchOption.AllDirectories);

        //isolate the actual model name from the directory path
        string[] x = Regex.Split(info[highestIndex].FullName, "ies/");
        string[] y = Regex.Split(x[1], "-0_");
        return y[0];

    }

    void EvaluateCandidates()
    {
        //If in the editor and not the executable
        //This is because directory locations change in the .exe version
        DirectoryInfo dirPath = new DirectoryInfo(paths.buildPath + "/summaries/");

        //Get all .csv files it can find
        FileInfo[] info = dirPath.GetFiles("*.csv");

        while (candidateNo < numberOfCandidates)
        {
            //Iterate through each file
            foreach (FileInfo f in info)
            {
                //open the file and check its name to make sure
                //you are only reading from the current generation
                if (f.Name == testName + "-generation-" + (generation-1) + "-candidate-" + candidateNo + "-0_LearningBrain.csv")
                {
                    filereader = f.OpenText();

                    //Initialise line counting and the candidates total score
                    string text = "";
                    int linecounter = 0;
                    float candidateTotal = 0.0f;

                    //Read through the file
                    while (text != null)
                    {
                        text = filereader.ReadLine();
                        //If the line is valid
                        if (text != null)
                        {
                            //Split the line into an array of strings, seperated by ","
                            string[] lines = Regex.Split(text, ",");

                            //If not the first line, transform string into numeric value if applicable,
                            //otherwise add zero, then iterate the line counter
                            //else ignore the first line which contains the titles
                            if (linecounter != 0 && linecounter % 2 != 0)
                            {
                                float trainingValue = 0.0f;
                                if (lines.Length == 6)
                                {
                                    float.TryParse(lines[5], out trainingValue);
                                }
                                candidateTotal += trainingValue;
                                linecounter++;
                            }
                            else
                                linecounter++;
                        }
                    }
                    //calculate the average score then move onto the next candidate.
                    float averageScore = candidateTotal / linecounter;
                    candidatescores.Add(averageScore);
                    candidateNo++;
                }
            }
        }
        //once all candidates scores retrieved
        //select the one with the highest average
        for (int i = 0; i < candidatescores.Count; i++)
        {
            if (candidatescores[i] > highestIndexValue)
            {
                highestIndex = i;
                highestIndexValue = candidatescores[i];
            }
        }

        //reset value for the next function call
        candidateNo = 0;
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

        //correct max steps according to generation
        int steps = generation * stepsPerGeneration;

        if (generation == 0)
            steps = stepsPerGeneration;
           
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
                "   max_steps: " + steps + "\n" +
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
        //if (Application.isEditor)
        //    sr = new StreamWriter(paths.editorPath + "/AI/exe_config.yaml", false);
        //else
            sr = new StreamWriter(paths.buildPath + "/TrainerConfiguration/exe_config.yaml", false);

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
    public void RunEvolutionaryLearning(string model)
    {
        SetupAnaconda();

        //Cycle through every generation
        for (int i = 0; i < numberOfGenerations; i++)
        {
            //if acurrent base of candidates has been developed
            if (currentCandidateBeingTrained == 0 && generation == 0)//Otherwise dont evaluate candidates and start from scratch
            {
                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=training_env_small/training_env_small --run-id=" + model + "-generation-" + generation + "-candidate-" + currentCandidateBeingTrained + " --train");
                    currentCandidateBeingTrained++;
                }
            }
            else
            {
                process.WaitForExit();
                UnityEngine.Debug.Log("hello i am zoidberg");
                //Evaluate candidates and work on pre-trained model
                EvaluateCandidates();
                SetupAnaconda();

                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=training_env_small/training_env_small --run-id=" + FindCandidate() + " --load --train");
                    currentCandidateBeingTrained++;
                }
            }


            //Iterate the generation and rewrite hyperparameters to increase maximum steps
            generation++;
            WriteHyperParameters();

        }

    }

    void SetModelSettings()
    {
        int result = 0;
        int.TryParse(modelSettings[0].text, out result);
        numberOfGenerations = result;

        result = 0;
        int.TryParse(modelSettings[1].text, out result);
        numberOfCandidates = result;

        result = 0;
        int.TryParse(modelSettings[2].text, out result);
        stepsPerGeneration = result;

        testName = modelSettings[3].text;

        WriteHyperParameters();
    }

    public void InitiateEvolutionaryLearning()
    {
        SetModelSettings();
        RunEvolutionaryLearning(testName);
    }

}
