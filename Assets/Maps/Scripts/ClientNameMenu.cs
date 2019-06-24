using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ClientNameMenu : NetworkBehaviour {

    //HUD objects
    //Input field in name selection menu
    public GameObject nameSelector;
    //Button that assigns the players name
    public GameObject goButton;

    //The owner of this menu
    public Client Owner;


    // Use this for initialization
    public void SetPlayerName () {
        Owner.InitialisePlayerName(nameSelector.GetComponent<InputField>().text);
    }

    // Update is called once per frame
    void Update()
    {

        if (nameSelector.GetComponent<InputField>().text == "")
            goButton.GetComponent<Button>().enabled = false;
        else
            goButton.GetComponent<Button>().enabled = true;

        return;
    }
}
