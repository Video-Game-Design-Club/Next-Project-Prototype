using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseButtons;
    public GameObject SettingsMenu;

    public GameObject PauseMenu;
    public Transform Player;


    void Awake()
    {
        GameIsPaused = false;
    }
    void Update() 
    {                             //Every frame check for if the escape key was pressed, then either pause or unpause the game.
        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (GameIsPaused)
                Resume();
            else 
                Pause();
        }       
    }   

    public void Resume()
    {   
        PauseButtons.SetActive(false); //hide pausebuttons
        SettingsMenu.SetActive(false); //hide settingsbuttons
        GameIsPaused = false;
        if(!DialogueManager.gameShouldBeFrozenForDialog)
            {
                UnFreeze();
            }
                                              
    }

    public void Pause() 
    {   
        PauseButtons.SetActive(true); //show pausebuttons
        GameIsPaused = true;
        // if(!DialogueManager.dialogueIsRunning)
        //     { 
        //         Freeze();
        //     }

        Freeze();
    }

    public void Freeze() 
    {                                              //Set the phyiscs speed to 0 and stop camera movement
        Time.timeScale = 0f;
        PauseMenu.GetComponent<Controls>().PauseSensitivity();
    }

    public void UnFreeze()
    {
        Time.timeScale = 1f;
        PauseMenu.GetComponent<Controls>().ResumeSensitivity();
    }

    public void FreezeMovementOnly() 
    {                                              //Set the phyiscs speed to 0 and stop camera movement
        Time.timeScale = 0f;
        PauseMenu.GetComponent<Controls>().PauseSensitivity();
        GameIsPaused = true;
        //insert lines of code that would lock player state to a state that would not allow player to move
    }

}
