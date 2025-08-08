/// <summary>
/// Controls pause menu display and game pausing.
/// Key Usage: Attach to persistent UI object; toggles menu and handles resume/quit.
/// </summary>
﻿using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages pause menu, result panel (victory/defeat), how-to-play popup, save, and main menu.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    // Singleton instance for easy access
    public static PauseMenuManager Instance;

    [Header("UI References")]
    public GameObject pauseMenuUI;          // The pause menu panel
    public GameObject resultPanel;          // Victory/Defeat panel
    public TextMeshProUGUI resultText;      // Shows "Victory!" or "Defeat!"
    public TextMeshProUGUI saveMessageText; // "Game Saved!" message
    public GameObject howToPlayPanel;       // How to Play panel

    [Header("Other References")]
    public GameManager gameManager;

    private bool isPaused = false;
    private bool hasGameStarted = false;    // To prevent pausing before starting


    private void Awake()
    {
        // Set up singleton instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

/// <summary>
    /// Start - Run setup logic at the beginning
    /// </summary>
    private void Start()
    {
        // Show HowToPlay at the start of the game, pause game time
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            Time.timeScale = 0f;
            hasGameStarted = false;
        }
        else
        {
            hasGameStarted = true;
        }
    }

/// <summary>
    /// Update - Update state or handle per-frame logic
    /// </summary>
    private void Update()
    {
        // Disable pausing if the how-to-play panel is active or game hasn't started yet
        if (howToPlayPanel != null && howToPlayPanel.activeSelf) return;
        if (!hasGameStarted) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    /// <summary>
    /// Show the pause menu and pause the game.
    /// </summary>
    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    /// <summary>
    /// Hide the pause menu and resume the game.
    /// </summary>
/// <summary>
    /// ResumeGame - Perform this action
    /// </summary>
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }


    public void SaveGame()
    {
        gameManager.SaveGame();
        StartCoroutine(ShowSaveMessage());
    }

    /// <summary>
    /// Show the "Game Saved!" message for 2 seconds.
    /// </summary>
    private IEnumerator ShowSaveMessage()
    {
        saveMessageText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        saveMessageText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Return to the main menu scene and reset time scale.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // =============== Victory/Defeat UI ===============
    /// <summary>
    /// Show result panel with win/lose text and pause the game.
    /// </summary>
    public void ShowResultPanel(bool isWin)
    {
        if (resultPanel != null && resultText != null)
        {
            resultPanel.SetActive(true);
            resultText.text = isWin ? "Victory!" : "Defeat!";
            Time.timeScale = 0f;
        }
    }
    // ==================================================

    // =============== HowToPlay UI ===============
    /// <summary>
    /// Called by the Start button on the HowToPlay panel.
    /// </summary>
/// <summary>
    /// StartGameAfterHowToPlay - Run setup logic at the beginning
    /// </summary>
    public void StartGameAfterHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        Time.timeScale = 1f;
        hasGameStarted = true;
    }

    /// <summary>
    /// Show the HowToPlay panel (optional, if you want to trigger it again).
    /// </summary>
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            Time.timeScale = 0f;
            hasGameStarted = false;
        }
    }
    // ==============================================
}
