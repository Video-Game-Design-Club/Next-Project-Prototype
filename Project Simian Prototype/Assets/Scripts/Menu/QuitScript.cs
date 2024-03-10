using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitScript : MonoBehaviour
{
    public void Quit() {
        PauseScript.GameIsPaused = false; //unpauses game
        SceneManager.LoadScene(0);  //Quits to the menu

    }
}
