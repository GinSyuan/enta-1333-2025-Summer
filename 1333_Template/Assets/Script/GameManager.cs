using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the GridManager component that handles grid creation and pathfinding.")]
    [SerializeField] private GridManager gridManager;

    [Tooltip("Reference to the main menu UI GameObject.")]
    [SerializeField] private GameObject mainMenuUI;

    [Tooltip("Reference to the player GameObject.")]
    public Transform player;

    [Tooltip("Reference to gameplay-related UI (optional).")]
    [SerializeField] private GameObject gameplayUI;

    private bool gameStarted = false;

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

    
        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (player != null)
            player.gameObject.SetActive(false); 
    }

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
}
