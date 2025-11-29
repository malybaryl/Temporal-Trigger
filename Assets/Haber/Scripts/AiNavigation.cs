using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
    public Node parent;

    public Node(bool walkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}

public class AiNavigation : MonoBehaviour
{
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 15;
    [SerializeField] private float nodeSize = 1f;
    [SerializeField] private Vector2 pastTimelineCenter = Vector2.zero;
    [SerializeField] private Vector2 presentTimelineCenter = Vector2.zero;
    [SerializeField] private Vector2 futureTimelineCenter = Vector2.zero;
    [SerializeField] private LayerMask obstacleMask;

    private enum TimelineGrid { NoGrid, Past, Present, Future };
    [SerializeField] private TimelineGrid debugShowGrid = TimelineGrid.NoGrid;
    private Dictionary<TimelineGrid, Node[,]> timelineGrids = new Dictionary<TimelineGrid, Node[,]>();

    public Node[,] pastGrid;
    public Node[,] presentGrid;
    public Node[,] futureGrid;

    void Start()
    {
        CreateGrid(pastTimelineCenter, ref pastGrid);
        CreateGrid(presentTimelineCenter, ref presentGrid);
        CreateGrid(futureTimelineCenter, ref futureGrid);

        timelineGrids[TimelineGrid.Past] = pastGrid;
        timelineGrids[TimelineGrid.Present] = presentGrid;
        timelineGrids[TimelineGrid.Future] = futureGrid;
    }
    void CreateGrid(Vector2 gridCenter, ref Node[,] gridRef)
    {
        gridRef = new Node[gridWidth, gridHeight];
        Vector2 bottomLeft = gridCenter - new Vector2(gridWidth, gridHeight) * 0.5f * nodeSize;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 worldPos = bottomLeft + new Vector2(x, y) * nodeSize + Vector2.one * nodeSize * 0.5f;
                bool walkable = !Physics2D.OverlapBox(worldPos, Vector2.one * nodeSize * 0.9f, 0f, obstacleMask);
                gridRef[x, y] = new Node(walkable, worldPos, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x + gridWidth * nodeSize * 0.5f) / (gridWidth * nodeSize));
        float percentY = Mathf.Clamp01((worldPos.y + gridHeight * nodeSize * 0.5f) / (gridHeight * nodeSize));

        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int y = Mathf.RoundToInt((gridHeight - 1) * percentY);

        return pastGrid[x, y];
    }

    void OnDrawGizmos()
    {
        if (debugShowGrid == TimelineGrid.NoGrid)
            return;

        Node[,] currentGrid = timelineGrids[debugShowGrid];
        ShowGrid(ref currentGrid);
    }

    private void ShowGrid(ref Node[,] grid)
    {
        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Color c = n.walkable ? Color.white : Color.red;
                c.a = 0.2f;
                Gizmos.color = c;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }
}
