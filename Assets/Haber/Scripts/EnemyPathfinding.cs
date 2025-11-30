using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static AiNavigation;

public class EnemyPathfinding : MonoBehaviour
{
    private AiNavigation aiNavigation;
    [SerializeField] private AiNavigation.Timeline enemyTimeline = AiNavigation.Timeline.Present;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float acceleration = 5f;
    private float lerpedSpeed = 0f;
    public Vector2 movementDirection { get; private set; } = Vector2.zero;


    private List<Node> path = new List<Node>();
    private int currentPathIndex = 0;

    private void Start()
    {
        GameObject gameManager = GameObject.FindWithTag("GameController");

        if (gameManager != null)
        {
            aiNavigation = gameManager.GetComponent<AiNavigation>();
            if (aiNavigation == null)
                Debug.LogError("Na GameController nie ma komponentu AiNavigation!");
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu z tagiem GameController!");
        }
    }

    void Update()
    {
        if (aiNavigation == null) return;
        MoveAlongPath();
    }

    public void SetPath(Vector2 targetPos)
    {
        Node[,] grid = aiNavigation.GetGridRef(enemyTimeline).nodes;
        Node startNode = GetClosestNode(transform.position, grid);
        Node targetNode = GetClosestNode(targetPos, grid);
        path = FindPath(startNode, targetNode, grid);
        currentPathIndex = 0;
    }

    void MoveAlongPath()
    {
        if (path == null || path.Count == 0 || currentPathIndex >= path.Count)
        {
            lerpedSpeed = 0;
            movementDirection = Vector2.zero;
            return;
        }

        Vector2 targetPos = path[currentPathIndex].worldPosition;
        Vector2 direction = targetPos - (Vector2)transform.position;
        movementDirection = direction.normalized;

        lerpedSpeed = Mathf.Clamp(lerpedSpeed + Time.deltaTime * acceleration, 0, maxSpeed);
        float scaledSpeed = lerpedSpeed * GameTime.timescale;

        transform.position = Vector2.MoveTowards(transform.position, targetPos, scaledSpeed * Time.deltaTime * GameTime.timescale);

        if ((Vector2)transform.position == targetPos)
            currentPathIndex++;
    }

    Node GetClosestNode(Vector2 worldPos, Node[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        float shortestDistance = float.MaxValue;
        Node closestNode = null;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid[x, y];
                if (!node.walkable) continue;

                float dist = Vector2.Distance(worldPos, node.worldPosition);
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    closestNode = node;
                }
            }
        }
        return closestNode;
    }

    List<Node> FindPath(Node startNode, Node targetNode, Node[,] grid)
    {
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(currentNode, grid))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<Node>();
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    List<Node> GetNeighbors(Node node, Node[,] grid)
    {
        List<Node> neighbors = new List<Node>();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    public void ClearPath()
    {
        path = null;
    }

    public void ResetSpeed()
    {
        lerpedSpeed = 0;
    }
}
