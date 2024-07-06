using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpeedPotion : MonoBehaviour
{

    effectStatus effectStatus;
    

    // Start is called before the first frame update
    void Start()
    {
        effectStatus = GetComponent<effectStatus>();

    }


    public void drinkPotion(InputAction.CallbackContext context)
    {
        // call effect status: speed. 
    }
    
        
    // Update is called once per frame
    void Update()
    {
        
    }
}
