using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour {

    public string name;
    public float timeLimit = 0;
    public int killLimit = 0;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

}
