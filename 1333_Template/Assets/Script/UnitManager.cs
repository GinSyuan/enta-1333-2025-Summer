/// <summary>
/// Manages all units in the game, both player and enemy.
/// Key Usage: Stores unit list; issues commands; integrates with pathfinding.
/// </summary>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all units in the scene, including their pathfinding components,
/// and handles unit movement along precomputed paths.
/// Press 'R' at runtime to reseed the grid and reset all units to their initial positions.
/// </summary>
public class UnitManager : MonoBehaviour
{
    /// <summary>
    /// A data container for each unit, holding its transform, target, path, etc.
    /// </summary>
    [System.Serializable]
    public class UnitEntry
    {
        [Tooltip("Transform of the unit GameObject in the scene.")]
        public Transform unitTransform;

        [Tooltip("Empty GameObject used as the target marker for pathfinding.")]
        public Transform targetTransform;

        [HideInInspector] public Pathfinder pathfinder;     // Pathfinder component for this unit
        [HideInInspector] public List<Vector3> path;        // The current calculated path
        [HideInInspector] public int pathIndex;             // Which waypoint in the path we are currently following
        [HideInInspector] public Vector3 initialPosition;   // Remembered starting position for resetting

        [HideInInspector] public Vector2Int lastGridIndex;  // Last known grid cell of this unit
    }

    [Header("Units Settings")]
    [Tooltip("List of units to manage at startup.")]
    [SerializeField] private List<UnitEntry> units = new List<UnitEntry>();

    [Header("Movement Settings")]
    [Tooltip("Speed at which each unit moves along its path.")]
    [SerializeField] private float moveSpeed = 2f;

    private GridManager gridManager;

    /// <summary>
    /// Public accessor for other scripts to get the list of units.
    /// </summary>
    public List<UnitEntry> Units => units;

/// <summary>
    /// Start - Run setup logic at the beginning
    /// </summary>
    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("UnitManager: No GridManager found in the scene.");
            return;
        }

        // Initialize each unit
        foreach (var u in units)
        {
            u.initialPosition = u.unitTransform.position;

            // If no target transform is assigned, create a marker object
            if (u.targetTransform == null)
            {
                GameObject marker = new GameObject($"{u.unitTransform.name}_TargetMarker");
                marker.transform.position = u.unitTransform.position;
                u.targetTransform = marker.transform;
            }

            // Add and initialize the Pathfinder component
            u.pathfinder = u.unitTransform.gameObject.AddComponent<Pathfinder>();
            u.pathfinder.Init(gridManager, u.unitTransform, u.targetTransform);

            // Compute initial path
            u.pathfinder.FindPath();
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;

            // Mark the unit’s starting grid cell as occupied
            u.lastGridIndex = gridManager.GetXYIndex(u.unitTransform.position);
            gridManager.SetCellOccupied(u.lastGridIndex.x, u.lastGridIndex.y, true);
        }
    }

/// <summary>
    /// Update - Update state or handle per-frame logic
    /// </summary>
    private void Update()
    {
        // Press R to randomize grid and reset all units
        if (Input.GetKeyDown(KeyCode.R))
        {
            gridManager.RandomizeSeedAndRebuild();
            ResetUnits();
            return;
        }

        // Move all units along their path
        foreach (var u in units)
        {
            if (u.path == null || u.pathIndex >= u.path.Count)
                continue;

            Vector3 nextPosition = u.path[u.pathIndex];

            // Move towards the next waypoint
            u.unitTransform.position = Vector3.MoveTowards(
                u.unitTransform.position,
                nextPosition,
                moveSpeed * Time.deltaTime
            );

            // If we reached this waypoint
            if (Vector3.Distance(u.unitTransform.position, nextPosition) < 0.05f)
            {
                // Update the occupied cells in the grid
                Vector2Int currentIndex = gridManager.GetXYIndex(u.unitTransform.position);
                if (currentIndex != u.lastGridIndex)
                {
                    gridManager.SetCellOccupied(u.lastGridIndex.x, u.lastGridIndex.y, false);
                    gridManager.SetCellOccupied(currentIndex.x, currentIndex.y, true);
                    u.lastGridIndex = currentIndex;
                }

                // Advance to the next waypoint
                u.pathIndex++;
            }
        }
    }

    /// <summary>
    /// Resets all units to their initial positions and recomputes their paths.
    /// </summary>
/// <summary>
    /// ResetUnits - Reset values or state
    /// </summary>
    private void ResetUnits()
    {
        foreach (var u in units)
        {
            // Reset position
            u.unitTransform.position = u.initialPosition;

            // Recompute path
            u.pathfinder.FindPath();
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;

            // Update grid cell occupancy
            u.lastGridIndex = gridManager.GetXYIndex(u.unitTransform.position);
            gridManager.SetCellOccupied(u.lastGridIndex.x, u.lastGridIndex.y, true);
        }
    }
}
