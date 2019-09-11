using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour {

    public bool canWalk;

    public int gCost;
    public int hCost;
    int fCost;

    public Vector3 worldPos;

    public PathNode(bool w, Vector3 p)
    {
        canWalk = w;
        worldPos = p;
    }
	
    public int getFCost()
    {
        return gCost + hCost;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
