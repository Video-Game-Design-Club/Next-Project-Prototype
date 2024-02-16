using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{

    public Camera cam;

    void Update()
    {
                //looks at camera then rotates 90 degrees on x axis (this works)

        Vector3 camPos = cam.transform.position;
        camPos.y = transform.position.y;
        transform.LookAt(camPos); //,Vector3.up
        transform.Rotate(90,0,0);

    }
}