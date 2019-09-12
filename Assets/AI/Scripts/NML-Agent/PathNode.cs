using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode {

    public bool canWalk;

    public int gCost;
    public int hCost;
    int fCost;

    public int XGridPos;
    public int YGridPos;

    public Vector3 worldPos;

    public PathNode parent;

    public PathNode(bool w, Vector3 p, int x, int y)
    {
        canWalk = w;
        worldPos = p;
        XGridPos = x;
        YGridPos = y;
    }
	
    public int getFCost()
    {
        return gCost + hCost;
    }

	// Update is called once per frame
	void Update () {
		
	}
}
