using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements A* pathfinding on a grid of GridNodes managed by GridManager.
/// Visualizes the frontier (yellow), visited (cyan), and final path (magenta) using Gizmos.
/// Press 'R' at runtime to re-run the pathfinding with current start/target positions.
/// Press 'G' at runtime to toggle Gizmos on or off.
/// </summary>
[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the GridManager that provides node data.")]
    [SerializeField] private GridManager gridManager;
    [Tooltip("Transform marking the starting position of the path.")]
    [SerializeField] private Transform startTransform;
    [Tooltip("Transform marking the target/goal position of the path.")]
    [SerializeField] private Transform targetTransform;

    [Header("Gizmos Toggle")]
    [Tooltip("Enable or disable drawing Gizmos at runtime.")]
    public bool showGizmos = true;

    private List<Vector3> pathPositions;             // World positions for the computed path
    private List<Vector3> frontierPositions = new(); // World positions of nodes in the open set
    private List<Vector3> visitedPositions = new();  // World positions of nodes in the closed set

    /// <summary>
    /// Exposes the computed path as a read-only list of world positions.
    /// </summary>
    public IReadOnlyList<Vector3> PathPositions => pathPositions;

    /// <summary>
    /// Initializes the references for the pathfinder at runtime (called by UnitManager).
    /// </summary>
    /// <param name="gm">GridManager instance to query for nodes.</param>
    /// <param name="start">Transform of the unit/start point.</param>
    /// <param name="target">Transform of the target/goal point.</param>
    public void Init(GridManager gm, Transform start, Transform target)
    {
        gridManager = gm;
        startTransform = start;
        targetTransform = target;
    }

    /// <summary>
    /// In Update:
    /// - Press 'R' to rerun the pathfinding algorithm from start to target.
    /// - Press 'G' to toggle whether Gizmos are drawn.
    /// </summary>
    private void Update()
    {
        // Recompute path on 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            FindPath();
        }

        // Toggle Gizmos on/off with 'G'
        if (Input.GetKeyDown(KeyCode.G))
        {
            showGizmos = !showGizmos;
            Debug.Log($"ShowGizmos = {showGizmos}");
        }
    }

    /// <summary>
    /// Validates references and starts A* from the grid indices under startTransform to targetTransform.
    /// </summary>
    public void FindPath()
    {
        if (gridManager == null || startTransform == null || targetTransform == null)
            return;

        Vector2Int startIdx = gridManager.GetXYIndex(startTransform.position);
        Vector2Int targetIdx = gridManager.GetXYIndex(targetTransform.position);
        AStarPath(startIdx, targetIdx);
    }

    /// <summary>
    /// Core A* implementation on a 2D grid. Computes a path from 'start' to 'goal'.
    /// Uses a simple openSet list and closedSet hash. Heuristic is Manhattan distance.
    /// Updates frontierPositions, visitedPositions, and pathPositions for Gizmo visualization.
    /// </summary>
    /// <param name="start">Grid coordinates of the start node.</param>
    /// <param name="goal">Grid coordinates of the goal node.</param>
    private void AStarPath(Vector2Int start, Vector2Int goal)
    {
        // Open set holds nodes to be evaluated; closedSet holds nodes already evaluated
        var openSet = new List<Vector2Int> { start };
        var closedSet = new HashSet<Vector2Int>();

        // Maps for scoring
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        var fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

        pathPositions = null;

        while (openSet.Count > 0)
        {
            // Update visualization lists (frontier = openSet, visited = closedSet)
            frontierPositions = openSet.ConvertAll(idx =>
                gridManager.GetNode(idx.x, idx.y).WorldPosition
            );
            visitedPositions.Clear();
            foreach (var v in closedSet)
            {
                visitedPositions.Add(
                    gridManager.GetNode(v.x, v.y).WorldPosition
                );
            }

            // Find the node in openSet with the lowest fScore
            var current = openSet[0];
            foreach (var node in openSet)
            {
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                {
                    current = node;
                }
            }

            // If we've reached the goal, reconstruct and return the path
            if (current == goal)
            {
                pathPositions = ReconstructPath(cameFrom, current);
                return;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Explore neighbors
            foreach (var neighbor in gridManager.GetNeighborsXY(current))
            {
                GridNode info = gridManager.GetNode(neighbor.x, neighbor.y);
                
                if (!info.Walkable || info.Occupied || closedSet.Contains(neighbor))
                    continue;


                int tentativeG = gScore[current] + info.Weight;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // No path found: pathPositions remains null
    }

    /// <summary>
    /// Reconstructs the path by walking back through the cameFrom dictionary until start is reached.
    /// Returns the list of world positions from start to goal (inclusive).
    /// </summary>
    /// <param name="cameFrom">Mapping of each node to the node it came from.</param>
    /// <param name="current">The goal node index.</param>
    /// <returns>List of world‐space positions in path order (start → goal).</returns>
    private List<Vector3> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current)
    {
        var totalPath = new List<Vector3>
        {
            gridManager.GetNode(current.x, current.y).WorldPosition
        };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Add(gridManager.GetNode(current.x, current.y).WorldPosition);
        }

        totalPath.Reverse();
        return totalPath;
    }

    /// <summary>
    /// Heuristic function (Manhattan distance) for grid‐based pathfinding.
    /// </summary>
    /// <param name="a">First grid coordinate.</param>
    /// <param name="b">Second grid coordinate.</param>
    /// <returns>Estimated cost from a to b.</returns>
    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// Draws debugging Gizmos in the Editor:
    /// - Yellow cubes for frontier (openSet).
    /// - Cyan cubes for visited (closedSet).
    /// - Magenta lines for the final path.
    /// - Green sphere at start node, Red sphere at goal node.
    /// Only draws when showGizmos is true.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        // Draw frontier (open set) in yellow
        Gizmos.color = Color.yellow;
        foreach (var pos in frontierPositions)
        {
            Gizmos.DrawCube(pos + Vector3.up * 0.1f, Vector3.one * 0.2f);
        }

        // Draw visited (closed set) in cyan
        Gizmos.color = Color.cyan;
        foreach (var pos in visitedPositions)
        {
            Gizmos.DrawCube(pos + Vector3.up * 0.1f, Vector3.one * 0.2f);
        }

        // Draw final path in magenta
        Gizmos.color = Color.magenta;
        if (pathPositions != null)
        {
            for (int i = 0; i < pathPositions.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPositions[i], pathPositions[i + 1]);
            }
        }

        // Draw start/goal spheres if available
        if (gridManager != null && startTransform != null && targetTransform != null)
        {
            Vector2Int startIdx = gridManager.GetXYIndex(startTransform.position);
            Vector2Int goalIdx = gridManager.GetXYIndex(targetTransform.position);

            Vector3 startWorld = gridManager.GetNode(startIdx.x, startIdx.y).WorldPosition;
            Vector3 goalWorld = gridManager.GetNode(goalIdx.x, goalIdx.y).WorldPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startWorld, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(goalWorld, 0.2f);
        }
    }
}
