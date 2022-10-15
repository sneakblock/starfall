using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    private void Start()
    {
        if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
        if (Cursor.visible == false) Cursor.visible = true;
    }

    public void Resume() {
        PauseableScene.resumeButtonCLicked = true;
    }

    public void GoToScene(string sceneName) {
        if (Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;
        if (!Cursor.visible) Cursor.visible = false;
        Time.timeScale = 1.0f;
        PauseableScene.isGamePaused = false;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Game has quit!");
    }
}
