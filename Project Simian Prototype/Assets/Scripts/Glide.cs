using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Glide : MonoBehaviour
{
    private Rigidbody rb;
    Movement_Juicer juice;
    [SerializeField] public float glideSpeed;
    [SerializeField] public float glideGravity;
    private float normalGravity;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        juice = GetComponent<Movement_Juicer>();
        normalGravity = juice.gravityV3.y;
    }

    public void Gliding(InputAction.CallbackContext context){
        if(context.performed && !juice.OnGround()){
            juice.gravityV3.y = glideGravity;
        }else{
            juice.gravityV3.y = normalGravity;
        }
    }

}
