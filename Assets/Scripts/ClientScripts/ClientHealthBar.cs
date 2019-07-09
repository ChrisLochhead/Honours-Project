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
    
    //Reference to this healthbars owner
    public Client Owner;

    public void InitialiseHealthbar()
    {
        //Initialise the healthbar, name and ranks position above the players head
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 8.5f, Owner.player.transform.position.z);
    }

    [Command]
    public void CmdRespawn()
    {
        Respawn();
        RpcRespawn();
    }

    public void Respawn()
    {
        //Only carry out this respawn on the server side
        if (isServer)
        {
            //Reset the healthbar
            floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            healthColour = Color.green;

            //Recolour the rank icon
            Color tmp = floatingRankIcon.GetComponent<Image>().color;
            tmp.a = 1.0f;
            floatingRankIcon.GetComponent<Image>().color = tmp;

            //Reactivate the name
            floatingName.SetActive(true);

            deathSet = false;
        }
    }

    [ClientRpc]
    public void RpcRespawn()
    {
        //Only carry this out on clients
        if (!isServer)
        {
            //Reset the healthbar
            floatingHealthBar.GetComponent<Image>().fillAmount = 1;
            healthColour = Color.green;

            //Recolour the rank icon
            Color tmp = floatingRankIcon.GetComponent<Image>().color;
            tmp.a = 1.0f;
            floatingRankIcon.GetComponent<Image>().color = tmp;

            //Reactivate the name
            floatingName.SetActive(true);

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
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 8.5f, Owner.player.transform.position.z);
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);
    }

    [ClientRpc]
    public void RpcUpdateHealth()
    {
        //update rank image
        floatingRankIcon.GetComponent<Image>().sprite = Owner.clientHUD.rankIcons[Owner.rank];

        //Set it's position
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 8.5f, Owner.player.transform.position.z);
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
        //Hide the name and rank icon
        Color tmp = floatingRankIcon.GetComponent<Image>().color;
        tmp.a = 0.0f;
        floatingRankIcon.GetComponent<Image>().color = tmp;
        floatingName.SetActive(false);
    }

    [ClientRpc]
    public void RpcDeath()
    {
        //Hide the name and rank icon
        Color tmp = floatingRankIcon.GetComponent<Image>().color;
        tmp.a = 0.0f;
        floatingRankIcon.GetComponent<Image>().color = tmp;
        floatingName.SetActive(false);
    }
    [Command]
    public void CmdUpdateHealth()
    {
        UpdateHealth();
        RpcUpdateHealth();
    }

    public void UpdatePosition()
    {
        //Update all the icons positions to stay above the players head
        floatingName.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 8.5f, Owner.player.transform.position.z);
        floatingHealthBar.transform.position = new Vector3(Owner.player.transform.position.x, Owner.player.transform.position.y + 7.5f, Owner.player.transform.position.z);
        floatingRankIcon.transform.position = new Vector3(Owner.player.transform.position.x - 5.8f, Owner.player.transform.position.y + 7.75f, Owner.player.transform.position.z);

    }

    void Update () {

        //Update everything
        CmdUpdateHealth();

        //Call the death function if the owner has died
        if (Owner.isDead && !deathSet)
        {
            CmdDeath();
            deathSet = true;
        }

    }
}
