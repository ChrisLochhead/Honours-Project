﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Vector2 pos;
    public float rot;
    public int type;
	// Use this for initialization
	void Start () {
		
	}

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
