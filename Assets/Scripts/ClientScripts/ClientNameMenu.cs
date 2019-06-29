using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

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
        CmdSetName(nameSelector.GetComponent<InputField>().text);
    }

    [ClientRpc]
    public void RpcSetName(string n)
    {
        Owner.playerName = n;
        Owner.clientHealthBar.floatingName.GetComponent<TextMeshPro>().text = n;
    }

    public void SetName(string n)
    {
        Owner.playerName = n;
        Owner.clientHealthBar.floatingName.GetComponent<TextMeshPro>().text = n;
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
