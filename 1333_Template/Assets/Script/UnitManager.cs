using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all units in the scene, including their pathfinding components,
/// and handles unit movement along precomputed paths.
/// Press 'R' at runtime to reseed the grid and reset all units to their initial positions.
/// </summary>
public class UnitManager : MonoBehaviour
{
    [System.Serializable]
    public class UnitEntry
    {
        [Tooltip("Transform of the unit GameObject in the scene.")]
        public Transform unitTransform;

        [Tooltip("Empty GameObject used as the target marker for pathfinding.")]
        public Transform targetTransform;

        [HideInInspector] public Pathfinder pathfinder;
        [HideInInspector] public List<Vector3> path;
        [HideInInspector] public int pathIndex;
        [HideInInspector] public Vector3 initialPosition;
    }

    [Header("Units Settings")]
    [Tooltip("List of units to manage at startup.")]
    [SerializeField] private List<UnitEntry> units = new List<UnitEntry>();

    [Header("Movement Settings")]
    [Tooltip("Speed at which each unit moves along its path.")]
    [SerializeField] private float moveSpeed = 2f;

    private GridManager gridManager;

    /// <summary>
    /// Exposes the list of units so other scripts can add entries at runtime.
    /// </summary>
    public List<UnitEntry> Units => units;

    /// <summary>
    /// At Start, find the GridManager, initialize each unit's Pathfinder, and compute initial paths.
    /// If a unit has no target marker assigned, creates a new one at the unit's position.
    /// </summary>
    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("UnitManager: No GridManager found in the scene.");
            return;
        }

        foreach (var u in units)
        {
            u.initialPosition = u.unitTransform.position;

            // If targetTransform is not assigned in the Inspector, create one at the unit's position
            if (u.targetTransform == null)
            {
                GameObject marker = new GameObject($"{u.unitTransform.name}_TargetMarker");
                marker.transform.position = u.unitTransform.position;
                u.targetTransform = marker.transform;
            }

            // Add a Pathfinder component to the unit's GameObject
            u.pathfinder = u.unitTransform.gameObject.AddComponent<Pathfinder>();
            u.pathfinder.Init(gridManager, u.unitTransform, u.targetTransform);

            // Compute the initial path
            u.pathfinder.FindPath();
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;
        }
    }

    /// <summary>
    /// In Update:
    /// - If the player presses 'R', reseed the grid, rebuild it, and reset all units.
    /// - Otherwise, iterate through each unit and move it along its path one step per frame.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reseed the grid and reset all units back to their start positions
            gridManager.RandomizeSeedAndRebuild();
            ResetUnits();
            return;
        }

        // Move each unit along its computed path
        foreach (var u in units)
        {
            if (u.path == null || u.pathIndex >= u.path.Count)
                continue;

            Vector3 nextPosition = u.path[u.pathIndex];
            u.unitTransform.position = Vector3.MoveTowards(
                u.unitTransform.position,
                nextPosition,
                moveSpeed * Time.deltaTime
            );

            // When the unit reaches the next point (within a small threshold), advance the index
            if (Vector3.Distance(u.unitTransform.position, nextPosition) < 0.05f)
            {
                u.pathIndex++;
            }
        }
    }

    /// <summary>
    /// Resets each unit to its initial position, recomputes its path, and resets pathIndex to zero.
    /// Called after the grid is rebuilt or when pressing 'R'.
    /// </summary>
    private void ResetUnits()
    {
        foreach (var u in units)
        {
            u.unitTransform.position = u.initialPosition;
            u.pathfinder.FindPath();
            u.path = new List<Vector3>(u.pathfinder.PathPositions ?? new List<Vector3>());
            u.pathIndex = 0;
        }
    }
}
