using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager; //Reference to the GridManager that probides node data.
    [SerializeField] private Transform startTransform; 
    [SerializeField] private Transform targetTransform;

    private List<Vector3> pathPositions; // List of world positions representing the found path.
    private List<Vector3> frontierPositions = new(); // Positions of nodes to be explored.
    private List<Vector3> visitedPositions = new(); // Pssitions of nodes already explored.

    public IReadOnlyList<Vector3> PathPositions => pathPositions; // Exposes the found path as a ready-only list.

    // Initializes the references for the pathfinder
    public void Init(GridManager gm, Transform start, Transform target)
    {
        gridManager = gm;
        startTransform = start;
        targetTransform = target;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // When the "R" key is pressed, initiate pathfinding
            FindPath();
    }

    //Starts the pathfinding process by validating references and calling A*.
    public void FindPath()
    {
        if (gridManager == null || startTransform == null || targetTransform == null)
            return;

        AStarPath(
            gridManager.GetXYIndex(startTransform.position),
            gridManager.GetXYIndex(targetTransform.position)
        );
    }

    //Implements A* to find a path from start to goal on the grid.
    private void AStarPath(Vector2Int start, Vector2Int goal)
    {
        //OpenSet holds nodes to evaluate, closedSet holds nodes already evaluated.
        var openSet = new List<Vector2Int> { start };
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

        pathPositions = null;

        while (openSet.Count > 0)
        {
            //Update visuaization lists for frontier and visited nodes.
            frontierPositions = openSet.ConvertAll(i =>
                gridManager.GetNode(i.x, i.y).WorldPosition);
            visitedPositions.Clear();
            foreach (var v in closedSet)
                visitedPositions.Add(
                    gridManager.GetNode(v.x, v.y).WorldPosition);

            //Pick the node in openSet with the lowest fScore as current.
            var current = openSet[0];
            foreach (var node in openSet)
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                    current = node;

            // If current is goal, reconstruct path and return.
            if (current == goal)
            {
                pathPositions = ReconstructPath(cameFrom, current);
                return;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Explore neighbors of the current node.
            foreach (var n in gridManager.GetNeighborsXY(current))
            {
                var info = gridManager.GetNode(n.x, n.y);
                if (!info.Walkable || closedSet.Contains(n))
                    continue;

                //Calcuate tentative gScore and update if it's better.
                var tG = gScore[current] + info.Weight;
                if (!gScore.ContainsKey(n) || tG < gScore[n])
                {
                    cameFrom[n] = current;
                    gScore[n] = tG;
                    fScore[n] = tG + Heuristic(n, goal);
                    if (!openSet.Contains(n))
                        openSet.Add(n);
                }
            }
        }
    }

    // Reconstructs the path from start to goal using the cameFrom map.
    private List<Vector3> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current)
    {
        var total = new List<Vector3> {
            gridManager.GetNode(current.x, current.y).WorldPosition
        };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            total.Add(
                gridManager.GetNode(current.x, current.y).WorldPosition);
        }
        total.Reverse();
        return total;
    }

    //Calculates the heuristic (Manhattan distance) between two nodes.
    private int Heuristic(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    // Draws Gizmos in the scene for frontier, visited, and the final path.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var p in frontierPositions)
            Gizmos.DrawCube(p + Vector3.up * 0.1f, Vector3.one * 0.2f);

        Gizmos.color = Color.cyan;
        foreach (var p in visitedPositions)
            Gizmos.DrawCube(p + Vector3.up * 0.1f, Vector3.one * 0.2f);

        Gizmos.color = Color.magenta;
        if (pathPositions != null)
        {
            for (int i = 0; i < pathPositions.Count - 1; i++)
                Gizmos.DrawLine(
                    pathPositions[i], pathPositions[i + 1]);
        }

        if (gridManager != null && startTransform != null)
        {
            var s = gridManager.GetNode(
                gridManager.GetXYIndex(startTransform.position).x,
                gridManager.GetXYIndex(startTransform.position).y)
                .WorldPosition;
            var g = gridManager.GetNode(
                gridManager.GetXYIndex(targetTransform.position).x,
                gridManager.GetXYIndex(targetTransform.position).y)
                .WorldPosition;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(s, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(g, 0.2f);
        }
    }
}
