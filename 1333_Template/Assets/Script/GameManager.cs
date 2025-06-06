using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GameManager is responsible for initializing core systems at startup.
/// In this project, it ensures that the GridManager builds its grid before other logic runs.
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
}
