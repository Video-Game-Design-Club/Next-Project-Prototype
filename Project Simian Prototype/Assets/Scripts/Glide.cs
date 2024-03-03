using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Manipulates the speed and gravity of the player to glide.
public class Glide : MonoBehaviour
{
    Movement_Juicer juice;
    [SerializeField] public float glideSpeed;
    [SerializeField] public float glideGravity;
    private float initialSpeed;                     // stores original speed from Movement_Juicer.cs
    private float normalGravity;                    // stores original gravity
    private bool isGliding;                         // check if player is gliding

    // Duration for glide
    [SerializeField] public float glideAbilityDuration;
    private float remainingTime;

    void Awake()
    {
        juice = GetComponent<Movement_Juicer>();
        normalGravity = juice.gravityV3.y;
        initialSpeed = juice.speed;
    }

    // user input, if player press ability button (set to left mouse button for now)
    public void GlidingInput(InputAction.CallbackContext context){
        isGliding = context.ReadValueAsButton();    // true if button is pressed and held. false when button is let go
        // Debug.Log(isGliding);
        // Debug.Log(context);
    }

    // timer for the duration of gliding
    // delete if we are not implementing it
    private void Update() {
        if (remainingTime > 0 && isGliding){
            remainingTime -= Time.deltaTime;
        }else{
            remainingTime = glideAbilityDuration;
            juice.gravityV3.y = normalGravity;
            juice.speed = initialSpeed;
            isGliding = false;
        }
    }

    private void FixedUpdate() {
        // check if player is in the air, falling, and ability pressed
        if(isGliding && !juice.OnGround() && juice.GetState() == Movement_Juicer.State.falling){
            juice.gravityV3.y = glideGravity;
            juice.speed = glideSpeed;
        }

        // uncomment and delete this line: Change mechanic (hold ability to keep gliding)
        // else{
        //     juice.gravityV3.y = normalGravity;
        //     juice.speed = initialSpeed;
        //     isGliding = false;
        // }

        // if player is on the ground, gravity and speed back to normal
        if(juice.OnGround()){
            juice.gravityV3.y = normalGravity;
            juice.speed = initialSpeed;
            isGliding = false;
        }
    }

}
