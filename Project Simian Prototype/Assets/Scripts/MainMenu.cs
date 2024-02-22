using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame (){            //When the play button is selected, the scene changes to the sample one. This can be changed in the future.
        SceneManager.LoadScene(1);
    }

    public void ExitGame (){           //When the exit button is clicked the scene closes.
        Debug.Log ("Exited");
        Application.Quit();
    }
}
