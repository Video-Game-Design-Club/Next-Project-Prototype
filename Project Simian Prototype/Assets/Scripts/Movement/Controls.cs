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
    

    public void PauseSensitivity() {                     //The next two functions simply pause and unpause the camera speed, since it is untethered to the physics engine and needs to be turned off separately.
        cam.m_XAxis.m_MaxSpeed = 0;
        cam.m_YAxis.m_MaxSpeed = 0;
        Debug.Log("Paused Game");
    }

    public void ResumeSensitivity() {
        cam.m_XAxis.m_MaxSpeed = 1000 * PlayerPrefs.GetFloat("Sensitivity")+.1f;
        cam.m_YAxis.m_MaxSpeed = 10 * PlayerPrefs.GetFloat("Sensitivity")+.1f;
        //Debug.Log("Resumed Game");
    }
}

