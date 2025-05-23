using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]                
public class Pathfinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;    // Reference to the GridManager in the scene
    [SerializeField] private Transform startTransform;   // World‐space marker for the path’s start
    [SerializeField] private Transform targetTransform;  // World‐space marker for the path’s goal

    private List<Vector3> pathPositions;                // Holds the resulting world‐space path

    private void Awake()
    {
        // When the game starts playing, automatically find a path once
        if (Application.isPlaying)
            FindPath();
    }

    private void OnValidate()
    {
        // In the Editor, re‐run pathfinding whenever you tweak references
        if (!Application.isPlaying)
            FindPath();
    }

    
    /// Public entry point: converts markers to grid indices and runs BFS.
    public void FindPath()
    {
        if (gridManager == null || startTransform == null || targetTransform == null)
            return;  // Ensure all references are assigned

        // Convert world‐space marker positions into grid cell indices
        Vector2Int startIdx = gridManager.GetXYIndex(startTransform.position);
        Vector2Int targetIdx = gridManager.GetXYIndex(targetTransform.position);

        // Run the BFS pathfinder
        pathPositions = BFSPath(startIdx, targetIdx);

        Debug.Log($"Path length = {(pathPositions == null ? 0 : pathPositions.Count)}");
    }

    
    /// BFS implementation on a grid.
    /// Finds the shortest unweighted path from start to goal.
    private List<Vector3> BFSPath(Vector2Int start, Vector2Int goal)
    {
        var queue = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();

        // Initialize BFS
        queue.Enqueue(start);
        visited.Add(start);

        bool found = false;

        // Perform BFS until queue is empty or goal is found
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == goal)
            {
                found = true;
                break;
            }

            // Examine each 4-direction neighbor
            foreach (var nbr in gridManager.GetNeighborsXY(current))
            {
                var nodeInfo = gridManager.GetNode(nbr.x, nbr.y);

                // Enqueue if walkable and not yet visited
                if (nodeInfo.Walkable && !visited.Contains(nbr))
                {
                    visited.Add(nbr);
                    parent[nbr] = current;
                    queue.Enqueue(nbr);
                }
            }
        }

        // If no path found, return null
        if (!found)
            return null;

        // Reconstruct the path from goal back to start
        var path = new List<Vector3>();
        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(gridManager.GetNode(step.x, step.y).WorldPosition);
            step = parent[step];
        }
        // Add the start position at the end
        path.Add(gridManager.GetNode(start.x, start.y).WorldPosition);

        // Reverse so it's from start → goal
        path.Reverse();
        return path;
    }

    
    /// Draws the found path and start/goal markers in the Scene view.
    private void OnDrawGizmos()
    {
        // Draw the path as magenta lines if available
        if (pathPositions != null && pathPositions.Count > 1)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < pathPositions.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPositions[i], pathPositions[i + 1]);
            }
        }

        // Draw the start marker (green) and goal marker (red) snapped to the grid
        if (gridManager != null && startTransform != null && targetTransform != null)
        {
            Vector2Int sIdx = gridManager.GetXYIndex(startTransform.position);
            Vector2Int gIdx = gridManager.GetXYIndex(targetTransform.position);

            Vector3 sPos = gridManager.GetNode(sIdx.x, sIdx.y).WorldPosition;
            Vector3 gPos = gridManager.GetNode(gIdx.x, gIdx.y).WorldPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(sPos, gridManager.GridSettings.NodeSize * 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(gPos, gridManager.GridSettings.NodeSize * 0.2f);
        }
    }
}
