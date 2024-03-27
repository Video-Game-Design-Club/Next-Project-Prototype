using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{
    public Transform cam;
    public bool looksUpWithCam;
    void Update()
    {
        Vector3 camPos = cam.transform.position;

        if (looksUpWithCam==true)
        {
            transform.LookAt(camPos); //looks at camera
            transform.Rotate(90,0,0); //rotates 90 degrees on x-axis
        }
        else
        {
            camPos.y = transform.position.y; //locks y-axis of camPos vector
            transform.LookAt(camPos); //looks at camPos vector, which is the same as the camera's position except for the y-axis
            transform.Rotate(90,0,0); //rotates 90 degrees on x-axis to be standing right way up again
        }            
    }
}