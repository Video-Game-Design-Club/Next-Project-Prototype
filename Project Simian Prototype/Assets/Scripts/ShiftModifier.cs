using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftModifier : MonoBehaviour
{
    [SerializeField]
    Movement_Juicer player;
    float sprint;
    float walk;
    bool lockCursor = true;

    // Start is called before the first frame update
    void Start()
    {
        sprint = player.speed * 2;
        walk = player.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            player.speed = sprint;
        }
        else
            player.speed = walk;
        if(Input.GetMouseButton(0) && lockCursor)
        {
            CursorLock();
            lockCursor = false;
        }
        else if(Input.GetKey(KeyCode.Escape) && !lockCursor)
        {
            CursorUnlock();
            lockCursor = true;
        }
    }

    void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void CursorUnlock()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
