using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePrompting : MonoBehaviour
{
    public CapsuleCollider player;
    public String debugLine1 = "NPC says \"Hi!\" (Press F again to stop)";
    public String debugLine2 = "NPC says \"Bye!\"";
    bool inConvo = false;
    bool canConvo = false;


/*
    //THIS CRASHES THE GAME WHEN YOU TRY TO TALK TO AN NPC
    void OnTriggerEnter(Collider Other)
    {
        if(Other==player)
        {
            canConvo = true;
        }

    }

    void OnTriggerStay(Collider Other)
    {
        if(canConvo&&!inConvo&&Input.GetKeyDown(KeyCode.F))
        {
            inConvo = true;
            Debug.Log(debugLine1);
            bool fKeyPressed = true;
            while (fKeyPressed)
            {
                if (Input.GetKeyUp(KeyCode.F))
                {
                    fKeyPressed=false;
                }
                if (!inConvo||!canConvo)
                {
                    break;
                }
            }
            
            if(canConvo&&inConvo&&Input.GetKeyDown(KeyCode.F))
            {
            Debug.Log(debugLine2);
            while (fKeyPressed)
            {
                if (Input.GetKeyUp(KeyCode.F))
                {
                    fKeyPressed=false;
                }
                if (!inConvo||!canConvo)
                {
                    break;
                }
            }
            inConvo = false;
            }
        }
        
        
    }

    void OnTriggerLeave(Collider Other)
    {
        if (Other==player)
        {
            inConvo = false;
            canConvo = false;
            
        }
    }

*/

}
