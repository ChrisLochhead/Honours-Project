using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

    public Vector2 pos;
    public float rot;
    public int type;

    Material wallMaterial;

    public float Health;
    public float totalHealth;

    private void Start()
    {
        SetHealth();    
    }

    public void SetType(int t)
    {
        type = t;
        SetHealth();
    }

    public void SetHealth()
    {
        if(type == 0)
        {
            totalHealth = 80;
        }
        else if(type == 1)
        {
            totalHealth = 125;
        }
        else if(type == 2)
        {
            totalHealth = 175;
        }
        else
        {
            totalHealth = 1;
        }

        Health = totalHealth;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(type == 3)
        {
            return;
        }
        else
        {
            if(collision.gameObject.GetComponent<Bullet>())
            {
                Health -= collision.gameObject.GetComponent<Bullet>().damageAmount / 2;
            }
        }
    }
    // Update is called once per frame
    void Update () {

        if (GetComponent<MeshRenderer>() && wallMaterial == null)
            wallMaterial = GetComponent<MeshRenderer>().material;

        if(wallMaterial)
        {
            Color tmp = wallMaterial.color;
            tmp.a = ((Health/100)*75)/totalHealth;
            wallMaterial.color = tmp;
        }

        if(Health <= 0)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
		
	}
}
