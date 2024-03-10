using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseButtons;
    public GameObject SettingsMenu;

    public GameObject PauseMenu;



    void Update() {                             //Every frame check for if the escape key was pressed, then either pause or unpause the game.
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(GameIsPaused) {
                Resume();
            } else {
                Pause();
            }
        }

        if (!GameIsPaused) //if the game is supposed to be unpaused, resume
        {
            Resume();
        }        
    }

    public void Resume() {                                             //Set the physics speed to 1 and resume camera movement
        PauseButtons.SetActive(false); //hide pausebuttons
        SettingsMenu.SetActive(false); //hide settingsbuttons
        Time.timeScale = 1f;
        PauseMenu.GetComponent<Controls>().ResumeSensitivity();
        GameIsPaused = false;
    }

    public void Pause() {                                              //Set the phyiscs speed to 0 and stop camera movement
        PauseButtons.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.GetComponent<Controls>().PauseSensitivity();
        GameIsPaused = true;
    }
}
