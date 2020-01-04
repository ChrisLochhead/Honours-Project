using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TMPro;
using UnityEngine.UI;

public class ELManager : MonoBehaviour
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
    public TMP_Dropdown[] trainingMaps;

    //Model settings such as name
    public TMP_InputField[] modelSettings;

    public Toggle debugMode;

    public string testName = "default";

    int startingStep = 0;

    bool isLoaded = false;

    private void Start()
    {
        fileutils = GetComponent<FileUtilities>();
    }

    public void FillDropDowns()
    {
        for (int i = 0; i < trainingMaps.Length; i++)
            trainingMaps[i].ClearOptions();

        fileutils.FillDropDowns(trainingMaps, "Evolution");
    }

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
            fileutils.DirectoryCopy(paths.buildPath + "/models/gen-" + testName + "-generation-" + (generation - 1) + "/candidate-" + highestIndex + "-0/", paths.buildPath + "/models/" + testName + "-generation-" + generation + "-candidate-" + c + "-0", "");
        }
        return testName + "-generation-" + (generation) + "-candidate-" + c;
    }

    void FinishTraining()
    {
        //Copy the finished model and delete the original 
        //Update the starting step and generation values
        int newStartingStep = int.Parse(hyperParameterSettings[3].text) * numberOfGenerations + startingStep;
        string fileinfo = newStartingStep + "," + numberOfGenerations;
        FileStream fs = File.Create(paths.buildPath + "/models/EL-GenerationAndSteps.txt");

        fs.Close();
        StreamWriter sw = new StreamWriter(paths.buildPath + "/models/EL-GenerationAndSteps.txt");
        sw.Write(fileinfo);
        sw.Close();

        fileutils.DirectoryCopy(paths.buildPath + "/models/", paths.buildPath + "/Evolutionary-Learning-Models/" + testName, "");

        //Remove the copy of the generation text file
        DirectoryInfo d = new DirectoryInfo(paths.buildPath + "/models/");
        foreach (FileInfo file in d.GetFiles())
        {
            file.Delete();
        }

        //Remove everything else in the models folder
        foreach (DirectoryInfo directory in d.GetDirectories())
        {

            foreach (FileInfo f in directory.GetFiles())
            {
                f.Delete();
            }
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


        //Iterate through each file
        foreach (FileInfo f in info)
        {
            string[] infoName = Regex.Split(f.Name, "-c");
            if (infoName[0] == testName + "-generation-" + (generation-1))
            {
                UnityEngine.Debug.Log("got in here");
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
        process = fileutils.SetupAnaconda(process);

        string mapName = trainingMaps[0].options[trainingMaps[0].value].text;

        if (Directory.Exists(paths.buildPath + "/Evolutionary-Learning-Models/" + testName))
        {
            isLoaded = true;

            //Move files back into model folder 
            fileutils.DirectoryCopy(paths.buildPath + "/Evolutionary-Learning-Models/" + testName, paths.buildPath + "/models/", "");

            //Delete file in evolutionary learning models folder
            DirectoryInfo storageDir = new DirectoryInfo(paths.buildPath + "/Evolutionary-Learning-Models/" + testName);
            foreach (DirectoryInfo d in storageDir.GetDirectories())
            {
                foreach (FileInfo f in d.GetFiles())
                {
                    f.Delete();
                }
            }
            storageDir.Delete(true);

            //Get Current Generation
            FileInfo fs = new FileInfo(paths.buildPath + "/models/EL-GenerationAndSteps.txt");
            StreamReader sr = fs.OpenText();
            string [] generationText = Regex.Split(sr.ReadLine(), ",");
            sr.Close();
            generation = int.Parse(generationText[1]);

            //Amend total generations to include starting point
            numberOfGenerations = numberOfGenerations + generation;

            //Get starting step
            startingStep = int.Parse(generationText[0]);

            //Create 3 new model folders for evaluation
            DirectoryInfo modelDir = new DirectoryInfo(paths.buildPath + "/models/");

            //Delete final model in models folder
            foreach (DirectoryInfo d in modelDir.GetDirectories())
            {
                if (d.Name == testName + "-FinalModel")
                {
                    //Copy the folder containing existing training checkpoints over
                    foreach(DirectoryInfo dSub in d.GetDirectories())
                    {
                       // for (int i = 0; i < numberOfCandidates; i++)
                       //     fileutils.DirectoryCopy(dSub.FullName, paths.buildPath + "/models/" + testName + "-generation-" + generation + "-candidate-" + i + "/EL-Brain", "");
                    }

                    foreach (FileInfo f in d.GetFiles())
                    {
                        //Copy the brain to a new generation of candidates
                        if (f.Name == "EL-Brain.nn")
                        {
                          //  for (int i = 0; i < numberOfCandidates; i++)
                          //      File.Copy(f.FullName, paths.buildPath + "/models/" + testName + "-generation-" + generation + "-candidate-" + i + "/" + f.Name);
                        } else
                        {
                            //For summary files
                            for (int i = 0; i < numberOfCandidates; i++)
                            {
                                //Create a directory to house the info
                             //   Directory.CreateDirectory(paths.buildPath + "/summaries/" + testName + "-generation-" + generation + "-candidate-" + i);
                             //   File.Copy(f.FullName, paths.buildPath + "/summaries/" + testName + "-generation-" + generation + "-candidate-" + i + "/" + f.Name);
                            }
                        }
                    }

                    //Then delete everything in the final model file
                    foreach (FileInfo f in d.GetFiles())
                    {
                        foreach (DirectoryInfo dSub in d.GetDirectories())
                        {
                            foreach (FileInfo fSub in dSub.GetFiles())
                            {
                                fSub.Delete();
                            }
                            dSub.Delete();
                        }
                        f.Delete();
                    }
                    d.Delete();
                }
            }
        }

        //Cycle through every generation
        for (int i = generation; i < numberOfGenerations; i++)
        {
            //if acurrent base of candidates has been developed
            if (currentCandidateBeingTrained == 0 && generation == 0)//Otherwise dont evaluate candidates and start from scratch
            {
                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=" + mapName + "/" + mapName + " --run-id=" + model + "-generation-" + generation + "-candidate-" + currentCandidateBeingTrained + " --train");
                    currentCandidateBeingTrained++;
                }
            }
            else
            {
                //if (isLoaded)
                //{
                //    process.StandardInput.WriteLine("exit");
                //}else
                process.WaitForExit();

                //Evaluate candidates and work on pre-trained model
                //here is where it breaks
                if(!isLoaded)
                EvaluateCandidates(generation);

                isLoaded = false;

                process = fileutils.SetupAnaconda(process);

                for (int j = 0; j < numberOfCandidates; j++)
                {
                    process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/exe_config.yaml --env=" + mapName + "/" + mapName + " --run-id=" + FindCandidate(j) + " --load --train");
                    currentCandidateBeingTrained++;
                }

                startingStep += int.Parse(hyperParameterSettings[3].text);
            }

            generation++;
            //Iterate the generation and rewrite hyperparameters to increase maximum steps
            fileutils.WriteHyperParameters(hyperParameterSettings, 0, false, null, startingStep);
        }

        process.WaitForExit();
        //Get the final winner
        EvaluateCandidates(generation);
        FindCandidate(highestIndex, true);
        FinishTraining();

        if (!debugMode.isOn)
            process.StandardInput.WriteLine("exit");
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

        fileutils.WriteHyperParameters(hyperParameterSettings, 0, false, null, startingStep);
    }

    public void InitiateEvolutionaryLearning()
    {
        //Initialise model settings and begin training
        SetModelSettings();
        RunEvolutionaryLearning(testName);
    }
}
