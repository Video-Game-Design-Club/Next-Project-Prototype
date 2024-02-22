using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitScript : MonoBehaviour
{
    public void Quit() {
        SceneManager.LoadScene(0);  //Quits to the menu
    }
}
