using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{
    public Cinemachine.CinemachineFreeLook thirdpersoncam;

    public Camera cam;
    public Transform floor;

    private Quaternion camFace; 

    private Quaternion standing; 

    void Awake()
    {
        thirdpersoncam = GetComponent<CinemachineFreeLook>();
    }


    void Start()
    {
        standing = new Quaternion();
    }

    void Update()
    {

        
        //transform.right = Camera.main.transform.right;
        //transform.up = Camera.main.transform.up + standing;
        //transform.right = standing;



        
        //transform.rotation = Quaternion.LookRotation(-cam.transform.up);
        
        //looks at camera then rotates 90 degrees on x axis (this works)

        Vector3 a = cam.transform.position;
        a.y = transform.position.y;
        transform.LookAt(a); //,Vector3.up
        transform.Rotate(90,0,0);
        //transform.rotation;
        
        


        //transform.RotateAround();
/*
    rotates towards rotation of camera (camera doesn't rotate)
        transform.rotation = camFace; 
        camFace = cam.transform.rotation;
        */

        //transform.Rotate(cam.eulerAngles);

        

        // transform.rotation = Quaternion.RotateTowards(transform.up, cam.transform.up);
        // transform.LookAt(floor.transform.position);
    }
}
