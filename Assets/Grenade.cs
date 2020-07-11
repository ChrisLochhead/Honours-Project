using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {

    float fuseTimer = 5.0f;
    bool hasExploded = false;
    bool debug = false;

    //Movement info
    Vector3 velocity;

	// Use this for initialization
	void Start () {
		
	}
	
    void InflictDamage()
    {
        Collider [] enemiesInRange = Physics.OverlapSphere(this.transform.position, 10);
        foreach(Collider c in enemiesInRange)
        {
            Debug.Log(c.gameObject.name);
            if(c.gameObject.tag == "Player1")
            {
                if (c.transform.parent.GetComponent<Client>().team != this.transform.parent.transform.parent.GetComponent<Client>().team)
                {
                    c.transform.parent.GetComponent<Client>().CmdTakeDamage(30);
                }
            }
        }
    }

    // Update is called once per frame
    void Update () {

        if (fuseTimer > 0.0f && debug == false)
        {
            fuseTimer -= Time.deltaTime;
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
