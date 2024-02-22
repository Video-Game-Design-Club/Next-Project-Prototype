using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseButtons;
    public GameObject SettingsMenu;

    void Update() {                             //Every frame check for if the escape key was pressed, then either pause or unpause the game.
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(GameIsPaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Resume() {                                             //Set the physics speed to 1 and resume camera movement
        PauseButtons.SetActive(false);
        Time.timeScale = 1f;
        SettingsMenu.GetComponent<Controls>().ResumeSensitivity();
        GameIsPaused = false;
    }

    public void Pause() {                                              //Set the phyiscs speed to 0 and stop camera movement
        PauseButtons.SetActive(true);
        Time.timeScale = 0f;
        SettingsMenu.GetComponent<Controls>().PauseSensitivity();
        GameIsPaused = true;
    }
}
