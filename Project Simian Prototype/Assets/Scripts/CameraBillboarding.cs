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
            transform.LookAt(camPos);
            transform.Rotate(90,0,0);


        }
        else
        {
            
            camPos.y = transform.position.y;
            transform.LookAt(camPos);
            transform.Rotate(90,0,0);

        }            
            
        



                
        // Vector3 camPos = cam.transform.position;
        // camPos.y = transform.position.y;
        // transform.LookAt(camPos); //,Vector3.up
        // transform.Rotate(90,0,0);



    }
}