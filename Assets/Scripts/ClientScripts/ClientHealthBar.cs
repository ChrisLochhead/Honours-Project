using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ClientHealthBar : NetworkBehaviour {

    //For manipulating the health bar
    public GameObject floatingHealthBar;
    public GameObject floatingRankIcon;
    public GameObject floatingName;

    //Colour of the healthbar
    [SyncVar(hook = "ChangeHealth")] public float healthPercentage = 1;
    [SyncVar(hook = "ChangeHealthColour")] public Color healthColour = Color.green;

    //For handling death and respawning
    public bool deathSet = false;


    public Client Owner;

    public void InitialiseHealthbar()
    {
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.0f, Owner.player.transform.position.z);
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

            Color tmp = floatingRankIcon.GetComponent<Image>().color;
            tmp.a = 1.0f;
            floatingRankIcon.GetComponent<Image>().color = tmp;
            deathSet = false;
        }
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        if (!isServer)
        {
            floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            healthColour = Color.green;

            Color tmp = floatingRankIcon.GetComponent<Image>().color;
            tmp.a = 1.0f;
            floatingRankIcon.GetComponent<Image>().color = tmp;
            deathSet = false;
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
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.0f, Owner.player.transform.position.z);
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }

    [ClientRpc]
    public void RpcUpdateHealth()
    {
        //update rank image
        floatingRankIcon.GetComponent<Image>().sprite = Owner.clientHUD.rankIcons[Owner.rank];

        //Set it's position
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.0f, Owner.player.transform.position.z);
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
    public void CmdDeath()
    {
        Death();
        RpcDeath();
    }

    public void Death()
    {
        Color tmp = floatingRankIcon.GetComponent<Image>().color;
        tmp.a = 0.0f;
        floatingRankIcon.GetComponent<Image>().color = tmp;
    }

    [ClientRpc]
    public void RpcDeath()
    {
        Color tmp = floatingRankIcon.GetComponent<Image>().color;
        tmp.a = 0.0f;
        floatingRankIcon.GetComponent<Image>().color = tmp;
    }
    [Command]
    public void CmdUpdateHealth()
    {
        UpdateHealth();
        RpcUpdateHealth();
    }

    public void UpdatePosition()
    {
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.0f, Owner.player.transform.position.z);
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);

    }





    // Update is called once per frame
    void Update () {

        CmdUpdateHealth();

        if (Owner.isDead && !deathSet)
        {
            CmdDeath();
            deathSet = true;
        }

    }
}
