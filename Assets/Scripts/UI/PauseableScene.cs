using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseableScene : MonoBehaviour
{
    public static bool isGamePaused = false;
    public static bool resumeButtonCLicked = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) || resumeButtonCLicked) {
            resumeButtonCLicked = false;
            if(isGamePaused) {
                Resume();
            }
            else {
                Pause();
            }
        }
    }

    void Resume() {
        Time.timeScale = 1.0f;
        isGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        SceneManager.UnloadSceneAsync("PauseMenu");
    }

    void Pause() {
        Time.timeScale = 0.0f;
        isGamePaused = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("PauseMenu", LoadSceneMode.Additive);
    }
}
