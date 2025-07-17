using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameManager gameManager;
    public TextMeshProUGUI saveMessageText;


    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

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

    private IEnumerator ShowSaveMessage()
    {
        saveMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f); 
        saveMessageText.gameObject.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
}
