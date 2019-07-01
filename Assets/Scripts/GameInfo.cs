using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour {

    public string name;
    public float timeLimit;
    public int killLimit;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

}
