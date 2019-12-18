using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TMPro;

public class FileUtilities : MonoBehaviour {

    public PathContainer paths;

    string BrainName = "";

    public List<string> environments;
    // Use this for initialization
    void Start() {
        if(paths.pathsValid)
        GetEnvironments();
    }

    public void DeleteDirectoryFiles(string directoryName)
    {
        //Erase all files in summaries
        foreach (string file in Directory.GetFiles(directoryName))
        {
            File.Delete(file);
        }
        //Erase all directories inside summaries
        foreach (string dir in Directory.GetDirectories(directoryName))
        {
            foreach (string file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
            Directory.Delete(dir);
        }
        Directory.Delete(directoryName);

    }
    public void DirectoryCopy(string sourceDirName, string destDirName, string trainingName, bool copySubDirs = true)
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

    public void MoveModelFiles(string testName, string type)
    {
        //Copy files from summary and models into new folder for this model
        if (type == "Imitation") BrainName = "IL-Brain";
        else if (type == "Curriculum") BrainName = "CL-Brain";
        else if (type == "Reinforcement") BrainName = "DRL-Brain";


        DirectoryCopy(paths.buildPath + "/summaries/" + testName + "-0_" + BrainName, paths.buildPath + "/" + type + "-Learning-Models/" + testName, "");
            DirectoryCopy(paths.buildPath + "/models/" + testName + "-0", paths.buildPath + "/" + type + "-Learning-Models/" + testName, "");

        //Erase all files in summaries
        foreach (string file in Directory.GetFiles(paths.buildPath + "/summaries/"))
        {
            string candidatePath = paths.buildPath + "/" + type + "-Learning-Models/" + testName;
            File.Copy(file, Path.Combine(candidatePath, Path.GetFileName(file)));
            File.Delete(file);
        }
        //Erase all directories inside summaries
        foreach (string dir in Directory.GetDirectories(paths.buildPath + "/summaries/"))
        {
            foreach (string file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
            Directory.Delete(dir);
        }

        //Delete everything in models directory
        foreach (string dir in Directory.GetDirectories(paths.buildPath + "/models/"))
        {
            foreach (string dirSub in Directory.GetDirectories(dir))
            {
                foreach (string file in Directory.GetFiles(dirSub))
                {
                    File.Delete(file);
                }
                Directory.Delete(dirSub);
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
            Directory.Delete(dir);
        }
    }

    public void throwError(string text)
    {
        UnityEngine.Debug.Log(text);
    }

    public void WriteHyperParameters(TMP_InputField[] hyperParameterSettings, int sessionNumber = 0, bool isImitation = false, TMP_Dropdown demoInputs = null)
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

        UnityEngine.Debug.Log((float.Parse(hyperParameterSettings[3].text) * (sessionNumber + 1)));
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
                "   max_steps: " + (float.Parse(hyperParameterSettings[3].text) * (sessionNumber + 1)) + "\n" +
                "   memory_size: 256 \n" +
                "   normalize: false \n" +
                "   num_epoch: " + hyperParameterSettings[4].text + "\n" +
                "   num_layers: " + hyperParameterSettings[5].text + "\n" +
                "   time_horizon: 64 \n" +
                "   sequence_length: 64 \n" +
                "   summary_freq: 1250 \n" +
                "   use_recurrent: false \n" +
                "   vis_encode_type: simple \n";
        if(!isImitation)
            serialisedData += 
                "   reward_signals: \n" +
                "       extrinsic: \n" +
                "           strength: 1.0 \n" +
                "           gamma: 0.99 \n" +
                "       curiosity: \n" +
                "           strength: 0.01 \n" +
                "           gamma: 0.99 \n" +
                "           encoding_size: 256";
        else
            serialisedData +=
                    "   reward_signals: \n" +
                    "       gail: \n" +
                    "           strength: 1.0 \n" +
                    "           gamma: 0.99 \n" +
                    "           demo_path: Demonstrations/" + demoInputs.options[demoInputs.value].text + "\n";

        StreamWriter sr;
        if (!isImitation)
            sr = new StreamWriter(paths.buildPath + "/TrainerConfiguration/exe_config.yaml", false);
        else
            sr = new StreamWriter(paths.buildPath + "/TrainerConfiguration/gail_config.yaml", false);

        sr.Write(serialisedData);
        sr.Close();
    }

    public Process SetupAnaconda(Process process)
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

        return process;
    }

    public void FillDropDowns(TMP_Dropdown[] maps, string type)
    {
        //Fill everything with all environments data
        for (int i = 0; i < maps.Length; i++)
        {
            for (int j = 0; j < environments.Count; j++)
            {
                if (j == 0)
                    maps[i].options.Add(new TMP_Dropdown.OptionData() { text = "select an environment." });

                if(type == "Curriculum")
                {
                    string [] w = Regex.Split(environments[j], "-");
                    if(w[0] == "training_env_CL")
                        maps[i].options.Add(new TMP_Dropdown.OptionData() { text = environments[j] });
                }
                else if (type == "Reinforcement")
                {
                    string[] w = Regex.Split(environments[j], "-");
                    if (w[0] == "training_env_DRL")
                        maps[i].options.Add(new TMP_Dropdown.OptionData() { text = environments[j] });
                }
                else if (type == "Evolution")
                {
                    string[] w = Regex.Split(environments[j], "-");
                    if (w[0] == "training_env_EL")
                        maps[i].options.Add(new TMP_Dropdown.OptionData() { text = environments[j] });
                }
                else if (type == "Imitation")
                {
                    string[] w = Regex.Split(environments[j], "-");
                    if (w[0] == "training_env_IL")
                        maps[i].options.Add(new TMP_Dropdown.OptionData() { text = environments[j] });
                }

            }
            //needs to reset after clearing for some reason
            maps[i].value = 1;
            maps[i].value = 0;
        }
    }

    public void GetEnvironments()
    {
        environments.Clear();

        DirectoryInfo dir = new DirectoryInfo(paths.buildPath);
        foreach(DirectoryInfo env in dir.GetDirectories())
        {
            string[] prefix = Regex.Split(env.Name, "_");
            if (prefix[0] == "training")
            {
                environments.Add(env.Name);
            }
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
