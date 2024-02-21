using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Glider : MonoBehaviour
{
    void Awake()
        {
        rb = GetComponent<Rigidbody>();
        gliderOn = false;
        }
    //initialize variables
    bool gliderOn;
    Rigidbody rb;
    public void GlideInput(InputAction.CallbackContext context)
    {
        gliderOn = context.ReadValueAsButton();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(gliderOn);
        if (gliderOn)
            {

            }
    }
}
