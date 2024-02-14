using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{
    // public Cinemachine.CinemachineFreeLook cam;
    public Transform cam;
    public Transform floor;

    private Quaternion camFace; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // transform.rotation = Quaternion.LookRotation(-cam.transform.up);

        camFace = cam.rotation;
        transform.LookAt(cam,-cam.up);
        //transform.Rotate(cam.eulerAngles);

        

        // transform.rotation = Quaternion.RotateTowards(transform.up, cam.transform.up);
        // transform.LookAt(floor.transform.position);
    }
}
