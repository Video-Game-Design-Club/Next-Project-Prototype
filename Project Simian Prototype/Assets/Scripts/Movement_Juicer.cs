using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement_Juicer : MonoBehaviour
{
    Vector2 rawInput;
    Rigidbody rb;
    State currentState = State.idle;

    Vector3 currentVec;
    Vector3 targetVec;

    public float acceleration;
    public float speed;

    public enum State
    {
        idle,
        walking,
        jumping,
        falling
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();
    }

    void doIdle()
    {
        //functions
        
        //switch states
        if (rawInput.magnitude > 0)
        {
            currentState = State.walking;
        }
    }

    void doMove()
    {
        //functionality
        targetVec.Set(rawInput.x * speed, 0f, rawInput.y * speed);
        currentVec.Set(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 juicedVec = new Vector3();
        juicedVec = targetVec - currentVec;

        rb.AddForce(juicedVec * acceleration);

        //switch states
        if(rb.velocity.magnitude <= 0.1f) 
        {
            currentState = State.idle;
        }
    }

    void doJump()
    {

    }

    public void Start()
    {
        currentVec = new Vector3(0f, 0f, 0f);
        targetVec = new Vector3(0f, 0f, 0f);
    }
    
    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.idle:
                doIdle();
                break;

            case State.walking:
                doMove();
                break;

            case State.jumping:
                doJump();
                break;

            case State.falling: 
                break;
        }
    }



}
