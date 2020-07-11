using UnityEngine.Networking;
using UnityEngine;

public class Grenade : NetworkBehaviour {

    float fuseTimer = 5.0f;
    bool hasExploded = false;
    bool debug = false;
    float pulseRate = 0.5f;
    public int team;
    //Movement info
    Vector3 velocity;
	
    void InflictDamage()
    {
        Collider [] enemiesInRange = Physics.OverlapSphere(this.transform.position, 10);
        foreach(Collider c in enemiesInRange)
        {
            Debug.Log(c.gameObject.name);
            if(c.gameObject.tag == "Player1")
            {
                if (c.transform.parent.GetComponent<Client>().team != team)
                {
                    c.transform.parent.GetComponent<Client>().CmdTakeDamage(30);
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        if (fuseTimer > 0.0f && fuseTimer < 1.5f) pulseRate = 0.1f;
        else if (fuseTimer >= 1.5f && fuseTimer < 3.5f) pulseRate = 0.25f;
        else pulseRate = 0.5f;

        if (fuseTimer % pulseRate < pulseRate / 5)
           this.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
        else
           this.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f));
        

        if (fuseTimer > 0.0f && debug == false)
        {
            fuseTimer -= Time.deltaTime/3;
        }
        else if(debug == false)
            hasExploded = true;

        if(hasExploded)
        {
            InflictDamage();
            hasExploded = false;
            Destroy(this.gameObject);
        }
        
	}
}
