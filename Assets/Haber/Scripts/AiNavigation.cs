using System.Collections.Generic;
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

public class Grid
{
    public Node[,] nodes;

    public Grid(Node[,] nodes)
    {
        this.nodes = nodes;
    }
}

public class AiNavigation : MonoBehaviour
{
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 15;
    [SerializeField] private float nodeSize = 1f;
    [SerializeField] private Vector2 presentTimelineCenter = Vector2.zero;
    [SerializeField] private Vector2 futureTimelineCenter = Vector2.zero;
    [SerializeField] private LayerMask obstacleMask;

    public enum Timeline {Present, Future };
    [SerializeField] private Timeline debugShowGrid = Timeline.Present;
    private Dictionary<Timeline, Grid> timelineGrids;

    public Node[,] presentGrid;
    public Node[,] futureGrid;

    private void Awake()
    {
        timelineGrids = new Dictionary<Timeline, Grid>();
        timelineGrids[Timeline.Present] = CreateGrid(presentTimelineCenter, ref presentGrid);
        timelineGrids[Timeline.Future] = CreateGrid(futureTimelineCenter, ref futureGrid);
    }

    private Grid CreateGrid(Vector2 gridCenter, ref Node[,] gridRef)
    {
        int gridScaledWidth = (int)(gridWidth / nodeSize);
        int gridScaledheight = (int)(gridHeight / nodeSize);

        gridRef = new Node[gridScaledWidth, gridScaledheight];
        Vector2 bottomLeft = gridCenter - new Vector2(gridScaledWidth, gridScaledheight) * 0.5f * nodeSize;

        for (int x = 0; x < gridScaledWidth; x++)
            for (int y = 0; y < gridScaledheight; y++)
            {
                Vector2 worldPos = bottomLeft + new Vector2(x, y) * nodeSize + Vector2.one * nodeSize * 0.5f;
                bool walkable = !Physics2D.OverlapBox(worldPos, Vector2.one * nodeSize * 0.9f, 0f, obstacleMask);
                gridRef[x, y] = new Node(walkable, worldPos, x, y);
            }

        return new Grid(gridRef);
    }

    public Node NodeFromWorldPoint(Vector2 worldPos, Timeline timeline)
    {
        float percentX = Mathf.Clamp01((worldPos.x + gridWidth * nodeSize * 0.5f) / (gridWidth * nodeSize));
        float percentY = Mathf.Clamp01((worldPos.y + gridHeight * nodeSize * 0.5f) / (gridHeight * nodeSize));

        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int y = Mathf.RoundToInt((gridHeight - 1) * percentY);

        return timelineGrids[timeline].nodes[x,y];
    }

    void OnDrawGizmos()
    {
        if(timelineGrids != null)
            ShowGrid(timelineGrids[debugShowGrid]);
    }

    private void ShowGrid(Grid grid)
    {
        if (grid != null)
        {
            foreach (Node n in grid.nodes)
            {
                Color c = n.walkable ? Color.white : Color.red;
                c.a = 0.2f;
                Gizmos.color = c;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }

    public Grid GetGridRef(Timeline timeline)
    {
        return timelineGrids[timeline];
    }
}
