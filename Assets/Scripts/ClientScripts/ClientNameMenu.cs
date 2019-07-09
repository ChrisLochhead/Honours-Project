using UnityEngine;
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
        CmdSetName(Owner.playerName);
    }

    [ClientRpc]
    public void RpcSetName(string n)
    {
        Owner.clientHealthBar.floatingName.GetComponent<TextMeshPro>().text = n;
    }

    public void SetName(string n)
    {
        Owner.clientHealthBar.floatingName.GetComponent<TextMeshPro>().text = n;
    }

    [Command]
    public void CmdSetName(string n)
    {
        SetName(n);
        RpcSetName(n);
    }

}
