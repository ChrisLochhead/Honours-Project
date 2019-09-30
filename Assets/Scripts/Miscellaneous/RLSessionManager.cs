using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class RLSessionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }

    public void OpenCMDPrompt()
    {
        UnityEngine.Debug.Log("called");
        string strCmdText;
        strCmdText = "/K D:\\Anaconda\\Scripts\\activate.bat D:\\Anaconda";   //This command to open a new notepad
        System.Diagnostics.Process.Start("CMD.exe", strCmdText); //Start cmd process
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
