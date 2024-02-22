using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class Controls : MonoBehaviour
{
    public CinemachineFreeLook cam;
    private float sens = 0.4f;  //Base value, in the future we will need to load from the settings file

    public void ChangeSensitivity(float newSens) {      //This function changes the sensitivity. Currently the ratio between vertical and horizontal movement is 1:100, but this can be changed in the future.
        sens = newSens;
        cam.m_XAxis.m_MaxSpeed = (1000 * newSens);
        cam.m_YAxis.m_MaxSpeed = (10 * newSens);
    }

    public void PauseSensitivity() {                     //The next two functions simply pause and unpause the camera speed, since it is untethered to the physics engine and needs to be turned off separately.
        cam.m_XAxis.m_MaxSpeed = 0;
        cam.m_YAxis.m_MaxSpeed = 0;
    }

    public void ResumeSensitivity() {
        cam.m_XAxis.m_MaxSpeed = (1000 * sens);
        cam.m_YAxis.m_MaxSpeed = (10 * sens);
    }
}

