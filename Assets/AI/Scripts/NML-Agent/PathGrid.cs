using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGrid : MonoBehaviour {

    //For finding obstacles
    public LayerMask boundaryMask;

    //Size of the grid
    public Vector2 gridDimensions;

    //Size of each node
    public float nodeRadius;

    //Container for the grid
    PathNode[,] grid;

    //Dimensions for the grid to node ratio
    int gridXDimension, gridYDimension;


    public GameObject player;

    public List<PathNode> path;

    public Vector2 gridCentre;

    void Start () {
        //Calculate the grid to node ratio 
        gridXDimension = Mathf.RoundToInt(gridDimensions.x / (nodeRadius * 2));
        gridYDimension = Mathf.RoundToInt(gridDimensions.y / (nodeRadius * 2));

        GenerateGrid();
    }

    void GenerateGrid()
    {
        //Create new array of nodes
        grid = new PathNode[gridXDimension, gridYDimension];

        //Find starting position for building the grid
        Vector3 bottomLeft = transform.position - Vector3.right * gridDimensions.x / 2 - Vector3.up * gridDimensions.y / 2;
        bottomLeft.x += gridCentre.x;
        bottomLeft.y += gridCentre.y;

        //Iterate through both rows and columns
        for (int x = 0; x < gridXDimension; x++)
        {
            for (int y = 0; y < gridYDimension; y++)
            {
                //Find the centre of the node
                Vector3 NodeCentre = bottomLeft + Vector3.right * (x * (nodeRadius * 2) + nodeRadius) + Vector3.up * (y * (nodeRadius * 2) + nodeRadius);
                NodeCentre.z = -10;
                //Use physics to check for any obstruction
                bool canwalk = !(Physics.CheckSphere(NodeCentre, nodeRadius, boundaryMask));
                //Create a new node with this information
                grid[x, y] = new PathNode(canwalk, NodeCentre, x, y);
            }
        }
    }


    public PathNode GetNode(Vector3 p)
    {
        //Get X and Y as a position local to the grid
        float xPoint = (p.x + gridDimensions.x / 2) / gridDimensions.x;
        float yPoint = (p.y + gridDimensions.y / 2) / gridDimensions.y;

        //Clamp to 0 and 1 for cases where p is outside the scope of the grid
        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        //Using these values, find which node this position corresponds to
        int NodeRow = Mathf.RoundToInt((gridXDimension - 1) * xPoint);
        int NodeColumn = Mathf.RoundToInt((gridYDimension - 1) * yPoint);

        PathNode test = grid[NodeRow, NodeColumn];
        return grid[NodeRow, NodeColumn];
    }

    public bool GetNodeEmpty(Vector3 p)
    {
        //Get X and Y as a position local to the grid
        float xPoint = (p.x + gridDimensions.x / 2) / gridDimensions.x;
        float yPoint = (p.y + gridDimensions.y / 2) / gridDimensions.y;

        //Clamp to 0 and 1 for cases where p is outside the scope of the grid
        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        //Using these values, find which node this position corresponds to
        int NodeRow = Mathf.RoundToInt((gridXDimension - 1) * xPoint);
        int NodeColumn = Mathf.RoundToInt((gridYDimension - 1) * yPoint);

        if (grid[NodeRow, NodeColumn].canWalk)
            return true;

        return false;
    }

    public List<PathNode> GetNeighbours(PathNode current)
    {
        List<PathNode> neighbours = new List<PathNode>();

        //Cycle though the nodes in a 3*3 cube around the current node
        for(int x = -1; x <= 1; x ++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //Ignore itself
                if (x == 0 && y == 0)
                    continue;

                //Find the position in the grid for the neighbour
                int xNode = current.XGridPos + x;
                int yNode = current.YGridPos + y;

                //Check that it actually exists, and if so add it to the list
                if (xNode >= 0 && xNode < gridXDimension &&
                    yNode >= 0 && yNode < gridYDimension)
                    neighbours.Add(grid[xNode, yNode]);
            }
        }
        return neighbours;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridDimensions.x, gridDimensions.y, 1));
        if (grid != null)
        {

            PathNode playerNode = GetNode(player.transform.position);
            foreach (PathNode n in grid)
            {
                if (playerNode.worldPos != n.worldPos)
                    Gizmos.color = (n.canWalk) ? Color.white : Color.red;
                else
                    Gizmos.color = Color.blue;

                if (path != null)
                    if (path.Contains(n))
                        Gizmos.color = Color.black;

                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeRadius * 2));

            }
        }
    }

}
