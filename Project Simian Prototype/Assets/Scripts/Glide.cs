using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Manipulates the speed and gravity of the player to glide.
// glidespeed/thrust, lift, gravity, no drag

// glide is set to toggle. left mouse button.
// glide has a duration (cant glide forever)

public class Glide : MonoBehaviour
{
    Movement_Juicer juice;
    Rigidbody rb;
    [SerializeField] public float glideSpeed;       // speed change when gliding (thrust)
    [SerializeField] public float lift;             // works against gravity
    private float initialSpeed;                     // stores original speed from Movement_Juicer.cs
    private bool isGliding;                         // check if player is gliding
    private bool zeroVelLock;                       // used in FixedUpdate, makes the velocity zero once. velocity in the y-direction
    private int glideCounter;                       // used so player cant spam glide ability to reset duration

    // Duration for glide
    [SerializeField] public float glideAbilityDuration;
    private float remainingTime;

    void Awake()
    {
        juice = GetComponent<Movement_Juicer>();
        rb = GetComponent<Rigidbody>();
        initialSpeed = juice.speed;
    }

    // user input, if player press ability button (set to left mouse button for now)
    public void GlidingInput(InputAction.CallbackContext context){
        // Toggle Mechanic
        if(context.performed){
            isGliding = !isGliding;
            glideCounter++;
        }
        // Hold Mechanic
        // isGliding = context.ReadValueAsButton();
    }

    // timer for the duration of gliding
    // delete if we are not implementing it
    private void Update() {
        if (remainingTime > 0 && glideCounter > 0 && !juice.OnGround()){
            remainingTime -= Time.deltaTime;
        }else if (remainingTime <= 0){
            resetGlide();
        }
    }

    private void FixedUpdate() {
        // check if player is in the air, falling, and ability pressed
        if(isGliding && !juice.OnGround() && juice.GetState() == Movement_Juicer.State.falling){
            if(!zeroVelLock){
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                zeroVelLock = true;
            }
            juice.speed = 0;
            rb.AddForce(Vector3.up * lift);         // lift
            rb.AddRelativeForce(0f,0f,glideSpeed);  // thrust (horizontal speed constant)
        }
        
        // if player is on the ground then speed back to normal
        if(juice.OnGround()){
            resetGlide();
        }
    }

    // glide will reset if player touches ground
    private void resetGlide(){
            remainingTime = glideAbilityDuration;
            juice.speed = initialSpeed;
            isGliding = false;
            glideCounter = 0;
            zeroVelLock = false;
    }

}
