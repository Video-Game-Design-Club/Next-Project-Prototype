using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{
    public Camera cam;
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
            camPos.y = transform.position.y; //locks y-axis
            transform.LookAt(camPos); //looks at camera
            transform.Rotate(90,0,0); //rotates 90 degrees on x-axis
        }            
    }
}