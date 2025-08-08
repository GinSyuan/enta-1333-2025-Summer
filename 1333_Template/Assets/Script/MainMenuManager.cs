/// <summary>
/// Controls main menu navigation and button actions.
/// Key Usage: Assign to main menu canvas; hooks up buttons to scene loading or quitting.
/// </summary>
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    public void StartNewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }


    public void LoadGame()
    {
        SaveSystem.LoadGame();
        SceneManager.LoadScene("SampleScene");
    }


    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
