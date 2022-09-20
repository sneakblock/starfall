using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void GoToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Game has quit!");
    }
}
