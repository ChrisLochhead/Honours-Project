using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TMPro;

public class ELSessionManager : MonoBehaviour
{

    //File reading 
    Process process;
    FileUtilities fileutils;
    StreamReader filereader = null;

    //Records highest performing candidate 
    //and accompanying score
    int highestIndex = 0;
    float highestIndexValue = 0.0f;

    //Candidate and generation info
    [SerializeField]
    int generation = 0;

    int numberOfGenerations = 3;
    int numberOfCandidates = 0;
    int candidateNo = 0;
    int currentCandidateBeingTrained = 0;
    public List<float> candidatescores;

    //Reference to paths
    public PathContainer paths;

    //Reference to hyperparameter settings
    public TMP_InputField[] hyperParameterSettings;

    //Map selection 
    public TMP_Dropdown trainingMaps;
    public int selectedMap;

    //Model settings such as name
    public TMP_InputField[] modelSettings;

    public string testName = "default";

    string FindCandidate(int c, bool isFinal = false)
    {
        //If on the final generation
        if (isFinal)
        {
            //Copy everything to new directory
            fileutils.DirectoryCopy(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0",
            paths.buildPath + "/models/" + testName + "-FinalModel/", "");
            return "";
        }

        //if not on the first or the last generation
        if (generation != 0)
        {           
            fileutils.DirectoryCopy(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0/", paths.buildPath + "/models/" + testName + "-generation-" + generation + "-candidate-" + c + "-0", testName);
        }

        return testName + "-generation-" + (generation) + "-candidate-" + c;
    }

    void FinishTraining()
    {
        //Copy the finished model and delete the original 
        fileutils.DirectoryCopy(paths.buildPath + "/models/", paths.buildPath + "/" + testName + "-EvolutionModel", "");
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

                fileutils.DirectoryCopy(dir.FullName, genPath, "");
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

    public void RunEvolutionaryLearning(string model)
    {
        fileutils.SetupAnaconda(process);

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
                fileutils.SetupAnaconda(process);

                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=training_env_small/training_env_small --run-id=" + FindCandidate(j) + " --load --train");
                    currentCandidateBeingTrained++;
                }
            }

            generation++;
            //Iterate the generation and rewrite hyperparameters to increase maximum steps
            fileutils.WriteHyperParameters(hyperParameterSettings);

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

        testName = modelSettings[3].text;

        fileutils.WriteHyperParameters(hyperParameterSettings);
    }

    public void InitiateEvolutionaryLearning()
    {
        //Initialise model settings and begin training
        SetModelSettings();
        RunEvolutionaryLearning(testName);
    }
}
