using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TMPro;
using UnityEditor;

public class ELSessionManager : MonoBehaviour
{

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

    private static void DirectoryCopy(string sourceDirName, string destDirName, string trainingName, bool copySubDirs = true)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string[] prefix = Regex.Split(file.Name, "-");

            if (prefix[0] != trainingName || trainingName == "")
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, trainingName, copySubDirs);
            }
        }
    }


    string FindCandidate(int c, bool isFinal = false)
    {
        if (isFinal)
        {
            DirectoryCopy(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0",
            paths.buildPath + "/models/" + testName + "-FinalModel/", "");
            //FileUtil.CopyFileOrDirectory(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0",
            //paths.buildPath + "/models/" + testName + "-FinalModel/");

            return "";
        }

        if (generation != 0)
        {

            DirectoryCopy(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0/", paths.buildPath + "/models/" + testName + "-generation-" + generation + "-candidate-" + c + "-0", testName);

        }
        return testName + "-generation-" + (generation) + "-candidate-" + c;
    }

    void FinishTraining()
    {
        DirectoryCopy(paths.buildPath + "/models/", paths.buildPath + "/" + testName + "-EvolutionModel", "");
        DirectoryInfo d = new DirectoryInfo(paths.buildPath + "/models/");

        foreach (DirectoryInfo directory in d.GetDirectories())
        {
            directory.Delete(true);
        }

    }

    void EvaluateCandidates(int generationNo)
    {
        //Get paths of both where the summaries have been stored and where
        //Also get a string for the new generations destination
        DirectoryInfo summaryPath = new DirectoryInfo(paths.buildPath + "/summaries/");
        DirectoryInfo modelPath = new DirectoryInfo(paths.buildPath + "/models/");
        string generationPath = modelPath + "gen-" + testName + "-generation-" + (generation - 1) + "/";

        //Count the candidate files to double check
        int candidateCounter = 0;
        foreach (DirectoryInfo dir in modelPath.GetDirectories())
        {
            string[] gens = Regex.Split(dir.Name, "-");
            if (gens[0] == "gen")
                continue;

            candidateCounter++;
        }

        //Create a path for this generation just completed
        Directory.CreateDirectory(generationPath);

        candidateCounter = 0;
        //Then do the same for the model files
        foreach (DirectoryInfo dir in modelPath.GetDirectories())
        {
            //check if an actual model or just a generation file
            string[] gens = Regex.Split(dir.Name, "-");
            if (gens[0] == "gen")
                continue;

            if (candidateCounter < numberOfCandidates)
            {
                string[] s = Regex.Split(dir.Name, "-c");
                string[] genNo = Regex.Split(s[0], "n-");
                int genNoInt = int.Parse(genNo[1]);

                string genPath = modelPath + "gen-" + testName + "-generation-" + genNoInt + "/candidate-" + candidateCounter + "-0";

                DirectoryCopy(dir.FullName, genPath, "");
                DeleteDirectory(dir.FullName);
                candidateCounter++;
            }
        }

        candidateCounter = 0;

        //Cycle through all the  summary files and delete them when done
        foreach (string file in Directory.GetFiles(paths.buildPath + "/summaries/"))
        {
            string[] s = Regex.Split(file, "-0_");
            string[] genNo = Regex.Split(s[0], "e-");
            int genNoInt = int.Parse(genNo[1]);

            string candidatePath = modelPath + "gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + genNoInt + "-0";
            File.Copy(file, Path.Combine(candidatePath, Path.GetFileName(file)));
            File.Delete(file);
        }

        foreach (DirectoryInfo dir in summaryPath.GetDirectories())
        {
            foreach (FileInfo f in dir.GetFiles())
            {
                f.Delete();
            }
            dir.Delete(true);
        }

        //Create a reference to this generations info
        DirectoryInfo genInfo = new DirectoryInfo(generationPath);

        //Get all .csv files it can find
        FileInfo[] info = genInfo.GetFiles("*.csv", SearchOption.AllDirectories);

        //breaks here
        while (candidateNo < numberOfCandidates)
        {
            //Iterate through each file
            foreach (FileInfo f in info)
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
                        if (linecounter != 0 && linecounter % 2 == 0)
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
                UnityEngine.Debug.Log("score = " + averageScore);
                candidatescores.Add(averageScore);
                candidateNo++;
                filereader.Close();
            }

        }
        //once all candidates scores retrieved
        //select the one with the highest average
        highestIndexValue = 0.0f;
        highestIndex = 0;

        for (int i = 0; i < candidatescores.Count; i++)
        {
            if (candidatescores[i] > highestIndexValue)
            {
                highestIndex = i;
                highestIndexValue = candidatescores[i];
            }
        }

        candidatescores.Clear();
        UnityEngine.Debug.Log("winner : " + highestIndex);
        //reset value for the next function call
        candidateNo = 0;

        //Duplicate winning candidate into next generation
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
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
                "   num_layers: " + hyperParameterSettings[3].text + "\n" +
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
        for (int i = 0; i < numberOfGenerations - 1; i++)
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

                //Evaluate candidates and work on pre-trained model
                EvaluateCandidates(generation);
                SetupAnaconda();

                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=training_env_small/training_env_small --run-id=" + FindCandidate(j) + " --load --train");
                    currentCandidateBeingTrained++;
                }
            }

            generation++;
            //Iterate the generation and rewrite hyperparameters to increase maximum steps
            WriteHyperParameters();

        }
        process.WaitForExit();
        EvaluateCandidates(generation - 1);
        FindCandidate(0, true);
        FinishTraining();

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
