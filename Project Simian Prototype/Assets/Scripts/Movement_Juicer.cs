using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement_Juicer : MonoBehaviour
{
    Vector2 rawInput;
    bool jumpInput;
    Rigidbody rb;
    int mask;
    bool onGround;
    
    [SerializeField]
    State currentState = State.idle;

    Vector3 currentVec;
    Vector3 targetVec;

    [Header("Movement Sauce")]
    public float acceleration;
    public float airAcceleration;
    public float speed;
    public float jumpForce;

    [Header("Movement Spice")]
    public float turnSmoothTime = 0.1f;
    float dampHolder;
    public Transform cam;

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

    public void JumpInput(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }

    void doIdle()
    {
        //functions
        
        //switch states
        if (rawInput.magnitude > 0)
        {
            currentState = State.walking;
        }

        if (jumpInput)
        {
            currentState = State.jumping;
        }
    }

    void doMove()
    {
        //functionality
        Vector3 juicedVec = new Vector3();

        targetVec.Set(rawInput.x, 0f, rawInput.y);

        /*targetVec.Set(rawInput.x * speed, 0f, rawInput.y * speed);
        currentVec.Set(rb.velocity.x, 0f, rb.velocity.z);*/

        float targetAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref dampHolder, turnSmoothTime);

        //targetVec = Quaternion.Euler(0f, targetAngle, 0f) * targetVec;
        
        /*juicedVec = targetVec - currentVec;*/
        
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        
        if (targetVec != Vector3.zero)
        {
            rb.AddForce(moveDir.normalized * acceleration);
        }
            

        //switch states
        if(rb.velocity.magnitude <= 0.1f) 
        {
            currentState = State.idle;
        }

        if (jumpInput)
        {
            currentState = State.jumping;
        }

        if (!onGround)
        {
            currentState = State.falling;
        }
    }

    void doJump()
    {
        //functionality
        rb.AddForce(0f, jumpForce, 0f, ForceMode.Impulse);

        //switch states
        currentState = State.falling;
    }

    void doFall()
    {
        //functionality
            //Rotates Player based on camera angle and input
        float targetAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref dampHolder, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //moves character along a vector that is rotated to be pointing the direction of the target angle trajectory
        targetVec.Set(rawInput.x, 0f, rawInput.y);
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            //checks if user has stopped inputting values because transfering back to idle state is dependent on speed not input
        if (targetVec != Vector3.zero)
        {
            rb.AddForce(moveDir.normalized * airAcceleration);
        }

        //switch states
        if (onGround && rb.velocity.magnitude < 0.1f)
        {
            currentState = State.idle;
        }
        else if(onGround && rb.velocity.magnitude >= 0.1f)
        {
            currentState = State.walking;
        }
    }

    public void Start()
    {
        currentVec = new Vector3(0f, 0f, 0f);
        targetVec = new Vector3(0f, 0f, 0f);

        mask = ~LayerMask.GetMask("Player");
    }

    private void Update()
    {
        
    }

    void FixedUpdate()
    {
        //Debug.Log(currentState);

        //on ground detector its gross and stupid and i hate it. thanks roxy <3
        Collider[] colliders = Physics.OverlapSphere(transform.position - Vector3.up * 0.55f, 0.49f, mask);
        if (colliders.Length > 0)
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }

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
                doFall();
                break;
        }
    }



}
