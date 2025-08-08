/// <summary>
/// Initializes and coordinates core game systems.
/// Key Usage: Ensures GridManager and other systems are ready at scene start.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [Tooltip("Reference to the GridManager component that handles grid creation and pathfinding.")]
    [SerializeField] private GridManager gridManager;

    [Tooltip("Reference to the main menu UI GameObject.")]
    [SerializeField] private GameObject mainMenuUI;

    [Tooltip("Reference to the player GameObject.")]
    public Transform player;

    [Tooltip("Reference to gameplay-related UI (optional).")]
    [SerializeField] private GameObject gameplayUI;

    // ======== Added for Victory/Defeat system ========
    [Header("Victory/Defeat Settings")]
    [Tooltip("Reference to the player's main tower Building.")]
    public Building mainTower; // Drag your main tower here in Inspector

    [Tooltip("How many enemy waves before the player wins?")]
    public int maxWaves = 5;
    [HideInInspector] public int currentWave = 0;

    private bool isGameOver = false;
    // ======== End Victory/Defeat section ========

    private bool gameStarted = false;

    private void Awake()
    {
        // Singleton assignment for global access
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gridManager != null)
        {
            gridManager.InitializeGrid();
        }
        else
        {
            Debug.LogError("GameManager: GridManager reference is missing in the Inspector.");
        }

        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (player != null)
            player.gameObject.SetActive(false);
    }

/// <summary>
    /// StartGame - Run setup logic at the beginning
    /// </summary>
    public void StartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (player != null)
            player.gameObject.SetActive(true);

        gameStarted = true;
    }

/// <summary>
    /// Update - Update state or handle per-frame logic
    /// </summary>
    private void Update()
    {
        if (!gameStarted) return;

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

    // ================== Victory/Defeat Methods ==================
    /// <summary>
    /// Call this method when a wave ends (from WaveManager)
    /// </summary>
    public void OnWaveCompleted()
    {
        currentWave++;
        Debug.Log($"[GameManager] Wave {currentWave} completed.");

        // If max waves completed and main tower still alive, player wins
        if (currentWave >= maxWaves && mainTower != null && mainTower.Health > 0)
        {
            Victory();
        }
    }

    /// <summary>
    /// Call this when the main tower is destroyed (from Building.cs)
    /// </summary>
    public void OnMainTowerDestroyed()
    {
        if (!isGameOver)
        {
            Defeat();
        }
    }


    private void Victory()
    {
        isGameOver = true;
        Debug.Log("[GameManager] You Win!");
        PauseMenuManager.Instance.ShowResultPanel(true);
    }

    private void Defeat()
    {
        isGameOver = true;
        Debug.Log("[GameManager] You Lose!");
        PauseMenuManager.Instance.ShowResultPanel(false);
        Debug.Log("Defeat() called!");
    }
    // ================== End Victory/Defeat Methods ==================

    // ================== Save/Load Methods ==================
    public void SaveGame()
    {
        PlayerData data = new PlayerData(player);
        SaveSystem.SaveGame(data);
    }


    public void LoadGame()
    {
        PlayerData data = SaveSystem.LoadGame();
        if (data != null)
        {
            player.position = new Vector3(data.playerX, data.playerY, 0f);
        }
    }
    // ================== End Save/Load ==================
}
