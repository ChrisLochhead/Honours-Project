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
        // Owner.InitialisePlayerName(nameSelector.GetComponent<InputField>().text);
        CmdSetName(nameSelector.GetComponent<InputField>().text);
    }

    [ClientRpc]
    public void RpcSetName(string n)
    {
        Owner.playerName = n;
    }

    public void SetName(string n)
    {
        Owner.playerName = n;
    }

    [Command]
    public void CmdSetName(string n)
    {
        SetName(n);
        RpcSetName(n);
    }

    // Called from the naming menu
    public void InitialisePlayerName(string n)
    {
        CmdSetName(n);
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
