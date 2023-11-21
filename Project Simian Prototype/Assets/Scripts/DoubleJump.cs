using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleJump : MonoBehaviour {
    
    Movement_Juicer juice;
    Rigidbody rb;
    bool hasJumped;

    public float doubleJumpForce;

    bool doubleJumpLock;

    public void JumpInput(InputAction.CallbackContext context) {
        if (!hasJumped) {
        hasJumped = context.started;
        }
    }

    private void Awake() {
        juice = GetComponent<Movement_Juicer>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        if(hasJumped && juice.GetState() == Movement_Juicer.State.falling && !doubleJumpLock) { 
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);

            doubleJumpLock = true;
        }

        if (juice.OnGround()) {
            doubleJumpLock = false;
        }

        hasJumped = false;
    }
}
