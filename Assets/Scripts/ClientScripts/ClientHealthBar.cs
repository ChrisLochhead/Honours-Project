using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ClientHealthBar : NetworkBehaviour {

    //For manipulating the health bar
    public GameObject floatingHealthBar;
    public GameObject floatingRankIcon;

    //Colour of the healthbar
    [SyncVar(hook = "ChangeHealth")] public float healthPercentage = 1;
    [SyncVar(hook = "ChangeHealthColour")] public Color healthColour = Color.green;

    public Client Owner;

    public void InitialiseHealthbar()
    {
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }

    public void CallRespawn()
    {
        CmdRespawn();
    }

    [Command]
    public void CmdRespawn()
    {
        Respawn();
        RpcRespawn();
    }

    public void Respawn()
    {
        if (isServer)
        {
            floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            healthColour = Color.green;

            floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(255);
        }
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        if (!isServer)
        {
            floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            healthColour = Color.green;

            floatingRankIcon.GetComponent<CanvasRenderer>().SetAlpha(255);
        }
    }
    public void UpdateHealth()
    {
        //Set colour
        if (healthPercentage > 0.7f)
            floatingHealthBar.GetComponent<Image>().color = Color.green;
        else
        if (healthPercentage <= 0.7f && healthPercentage > 0.25f)
            floatingHealthBar.GetComponent<Image>().color = Color.yellow;
        else
            floatingHealthBar.GetComponent<Image>().color = Color.red;

        //update rank image also
        floatingRankIcon.GetComponent<Image>().sprite = Owner.clientHUD.rankIcons[Owner.rank];

        //And finally set it's position
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }

    [ClientRpc]
    public void RpcUpdateHealth()
    {
        //update rank image
        floatingRankIcon.GetComponent<Image>().sprite = Owner.clientHUD.rankIcons[Owner.rank];

        //Set it's position
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }


    void ChangeHealth(float h)
    {
        //Set the Fill Amount
        floatingHealthBar.GetComponent<Image>().fillAmount = h;

        //Set colour
        if (h > 0.7f)
            healthColour = Color.green;
        else
        if (h <= 0.7f && h > 0.25f)
            healthColour = Color.yellow;
        else
            healthColour = Color.red;

    }

    void ChangeHealthColour(Color c)
    {
        floatingHealthBar.GetComponent<Image>().color = c;
    }

    [Command]
    public void CmdUpdateHealth()
    {
        UpdateHealth();
        RpcUpdateHealth();
    }

    public void UpdatePosition()
    {
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }
    // Update is called once per frame
    void Update () {

        CmdUpdateHealth();

    }
}
