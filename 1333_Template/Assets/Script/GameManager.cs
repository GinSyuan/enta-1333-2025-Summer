using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GameManager is responsible for initializing core systems at startup
/// and handling global input controls for toggles.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the GridManager component that handles grid creation and pathfinding.")]
    [SerializeField] private GridManager gridManager;

    /// <summary>
    /// On Awake, initialize the grid so that all dependent systems have a valid grid to work on.
    /// </summary>
    private void Awake()
    {
        if (gridManager != null)
        {
            gridManager.InitializeGrid();
        }
        else
        {
            Debug.LogError("GameManager: GridManager reference is missing in the Inspector.");
        }
    }

    /// <summary>
    /// Handle global input for toggling combat and unit gizmos.
    /// </summary>
    private void Update()
    {
        // Toggle unit range gizmos with H
        if (Input.GetKeyDown(KeyCode.H))
        {
            Unit.showRangeGizmos = !Unit.showRangeGizmos;
            Debug.Log($"[GameManager] showRangeGizmos = {Unit.showRangeGizmos}");
        }

        // Disable combat with 1 (e.g. pixel formation mode)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Unit.isCombatEnabled = false;
            Debug.Log("[GameManager] Combat disabled for formation.");
        }

        // Enable combat again with C
        if (Input.GetKeyDown(KeyCode.C))
        {
            Unit.isCombatEnabled = true;
            Debug.Log("[GameManager] Combat enabled.");
        }
    }
}
