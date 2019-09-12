using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour {


    public void FindPath(PathGrid g, Vector3 startPos, Vector3 endPos)
    {
        //Translate start and end position into nodes on this
        //agents map
        PathNode start = g.GetNode(startPos);
        PathNode end = g.GetNode(endPos);

        //Two lists initialised to represent possible and impossible nodes for the path
        List<PathNode> openNodes = new List<PathNode>();
        List<PathNode> closedNodes = new List<PathNode>();

        //Initialise by adding the first node
        openNodes.Add(start);

        //While the open list has not exhausted all of the nodes (and a solution has not been found)
        while(openNodes.Count > 0)
        {
            //Initialise the current node to the first in the open list
            PathNode current = openNodes[0];

            //Cycle through the open list, if the f cost is less than the current,
            //or it is equal but the heuristic cost is lower, make this node the current
            for(int i = 1; i < openNodes.Count; i++)
            {
                if(openNodes[i].getFCost()< current.getFCost()
                    || openNodes[i].getFCost() == current.getFCost() && openNodes[i].hCost < current.hCost)
                {
                    current = openNodes[i];
                }
            }

            //Remove the node with the lowest predicted cost from the open list and add
            //it to the closed list
            openNodes.Remove(current);
            closedNodes.Add(current);

            //Break the while loop if the target node has been reached
            if (current == end)
            {
                InvertPath(start, end, g);
                return;
            }

            //Cycle through the current nodes neighbours, ignoring the ones 
            //that are blocked
            foreach (PathNode n in g.GetNeighbours(current))
            {
                if (!n.canWalk || closedNodes.Contains(n))
                    continue;

                //Establish the neighbouring node with the lowest predicted cost
                //Add it to the path list
                int movementCost = current.gCost + GetNodeDistance(current, n);
                if(movementCost < n.gCost || !openNodes.Contains(n))
                {
                    n.gCost = movementCost;
                    n.hCost = GetNodeDistance(n, end);
                    n.parent = current;

                    if (!openNodes.Contains(n))
                        openNodes.Add(n);
                }
            }
        }

    }

    int GetNodeDistance(PathNode a, PathNode b)
    {
        //Get number of nodes in x and y between the two nodes
        int xDist = Mathf.Abs(a.XGridPos - b.XGridPos);
        int yDist = Mathf.Abs(a.YGridPos - b.YGridPos);

        //Discern which value is greater in order to
        //know which moves are diagonal and will incur the greater estimated
        //cost
        if (xDist > yDist)
            return 14 * yDist + 10 * (xDist - yDist);

        return 14 * xDist + 10 * (yDist - xDist);
    }
    
    private void InvertPath(PathNode start, PathNode end, PathGrid g)
    {
        List<PathNode> p = new List<PathNode>();
        PathNode current = end;

        while(current != start)
        {
            p.Add(current);
            current = current.parent;
        }

        p.Reverse();

        g.path = p;
    }
}
