using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{

    public void Resume() {
        PauseableScene.resumeButtonCLicked = true;
    }

    public void GoToScene(string sceneName) {
        Time.timeScale = 1.0f;
        PauseableScene.isGamePaused = false;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Game has quit!");
    }
}
