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
                "       gail: \n"+
                "           strength: 1.0 \n" +
                "           gamma: 0.99 \n" +
                "           demo_path: Demonstrations/" + demoInputs.options[demoInputs.value].text + "\n";

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

    public void ImitationLearning()
    {
        SetupAnaconda();
        WriteHyperParameters();

        process.StandardInput.WriteLine(@"mlagents-learn TrainerConfiguration/gail_config.yaml --env=training_env_small/training_env_small --run-id=" + modelName.text  + " --train");

    }
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

    void MoveModelFiles()
    {
        //Copy files from summary and models into new folder for this model
        DirectoryCopy(paths.buildPath + "/summaries/" + modelName.text + "-0_LearningBrain", paths.buildPath + "/Imitation-Learning-Models/" + modelName.text, "");
        DirectoryCopy(paths.buildPath + "/models/" + modelName.text + "-0", paths.buildPath + "/Imitation-Learning-Models/" + modelName.text, "");

        //Erase all files in summaries
        foreach (string file in Directory.GetFiles(paths.buildPath + "/summaries/"))
        {
            string candidatePath = paths.buildPath + "/Imitation-Learning-Models/" + modelName.text;
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

    private void Update()
    {
        if (process != null)
        {
            if (process.HasExited)
            {
                //trainingInfo.text = "training completed sucessfully.";
                MoveModelFiles();
                process = null;
            }
        }
    }
}
