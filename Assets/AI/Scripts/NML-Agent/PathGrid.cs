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

    int counter;

	void Start () {
        //Calculate the grid to node ratio 
        gridXDimension = Mathf.RoundToInt(gridDimensions.x / (nodeRadius * 2));
        gridYDimension = Mathf.RoundToInt(gridDimensions.y / (nodeRadius * 2));

        counter = 0;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        //Create new array of nodes
        grid = new PathNode[gridXDimension, gridYDimension];

        //Find starting position for building the grid
        Vector3 bottomLeft = transform.position - Vector3.right * gridDimensions.x / 2 - Vector3.up * gridDimensions.y / 2;

        //Iterate through both rows and columns
        for (int x = 0; x < gridXDimension; x++)
        {
            for (int y = 0; y < gridYDimension; y++)
            {
                //Find the centre of the node
                Vector3 NodeCentre = bottomLeft + Vector3.right * (x * (nodeRadius * 2) + nodeRadius) + Vector3.up * (y * (nodeRadius * 2) + nodeRadius);
                Debug.Log(NodeCentre);
                //Use physics to check for any obstruction
                bool canwalk = !(Physics.CheckSphere(NodeCentre, nodeRadius, boundaryMask));
                //Create a new node with this information
                grid[x, y] = new PathNode(canwalk, NodeCentre);
                counter++;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridDimensions.x, gridDimensions.y, 10));
        Debug.Log(counter);
        if(grid != null)
        {
            foreach(PathNode n in grid)
            {
                Gizmos.color = (n.canWalk) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeRadius * 2));
            }
        }
    }
}
