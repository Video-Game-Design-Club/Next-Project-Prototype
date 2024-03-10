using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Controls : MonoBehaviour
{
    public CinemachineFreeLook cam;
    public Slider sensSlider;

    //private float sens = 0.4f;  //Base value, in the future we will need to load from the settings file   //changed code to reference "Sensitivity" in PlayerPrefs instead of "private float sens" here

    void Awake()
    {
        sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");
    }
    void Start()
    {
        sensSlider.value = PlayerPrefs.GetFloat("Sensitivity");
    }

    public void ChangeSensitivity(float newSens) {      //This function changes the sensitivity. Currently the ratio between vertical and horizontal movement is 1:100, but this can be changed in the future.
        if (newSens != 0)
        {
            PlayerPrefs.SetFloat("Sensitivity", newSens);   //updates "Senseitivity" in PlayerPrefs to newSens
            Debug.Log("Sensitivity Changed to: " + PlayerPrefs.GetFloat("Sensitivity"));
        }
        else
        return;

        // cam.m_XAxis.m_MaxSpeed = (1000 * PlayerPrefs.GetFloat("Sensitivity"));
        // cam.m_YAxis.m_MaxSpeed = (10 * PlayerPrefs.GetFloat("Sensitivity"));

    }

    public void PauseSensitivity() {                     //The next two functions simply pause and unpause the camera speed, since it is untethered to the physics engine and needs to be turned off separately.
        cam.m_XAxis.m_MaxSpeed = 0;
        cam.m_YAxis.m_MaxSpeed = 0;
        Debug.Log("Paused Game");
    }

    public void ResumeSensitivity() {
        cam.m_XAxis.m_MaxSpeed = (1000 * PlayerPrefs.GetFloat("Sensitivity"));
        cam.m_YAxis.m_MaxSpeed = (10 * PlayerPrefs.GetFloat("Sensitivity"));
        //Debug.Log("Resumed Game");
    }
}

