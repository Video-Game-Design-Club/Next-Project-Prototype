using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraBillboarding : MonoBehaviour
{

     public Transform cam;
    // public CinemachineFreeLook cam;

    void Awake()
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // transform.(cam.transform.eulerAngles);

        transform.LookAt(cam.transform.position, Vector3.up);
        // transform.Rotate(Vector3.down);
        // transform.LookAt(cam.transform.position, Vector3.right);
        // transform.LookAt(cam.transform.position, Vector3.forward); 
    }
}
