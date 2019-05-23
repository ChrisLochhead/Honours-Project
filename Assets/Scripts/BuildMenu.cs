using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BuildMenu : MonoBehaviour {

    public GameObject openState;
    public GameObject closedState;

    int currentState;

    public Sprite[] buttons;

	// Use this for initialization
	void Start () {
        currentState = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void CloseClicked()
    {
        if (currentState == 0)
        {
            openState.SetActive(true);
            closedState.SetActive(false);
            currentState = 1;
        }
        else
        {
            openState.SetActive(false);
            closedState.SetActive(true);
            currentState = 0;
        }
    }
}
