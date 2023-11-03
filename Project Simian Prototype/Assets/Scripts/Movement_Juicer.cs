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
    Vector3 inputVec;
    Vector3 currentVel;

    [Header("Movement Sauce")]
    public float acceleration;
    public float airAcceleration;
    public float speed;
    public float jumpForce;
    public float maxAccForce;

    [Header("Movement Spice")]
    public float turnSmoothTime = 0.1f;
    float dampHolder;
    public Transform cam;

    [Header("Floating settings")]
    public float springLength;
    public float rideHeight;
    public float springConstant;
    public float dampingConstant;
    RaycastHit hitInfo;
    bool floatlock;

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

    IEnumerator FloatLock()
    {
        floatlock = true;
        yield return new WaitForSeconds(0.3f);
        floatlock = false;
    } 

    public void MoveInput(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValueAsButton();
    }


    void doFloat()
    {
        if (!floatlock)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, springLength, mask))
            {
                Vector3 vel = rb.velocity;
                Vector3 rayDownDir = transform.TransformDirection(Vector3.down);

                float rayDownDirVel = Vector3.Dot(rayDownDir, vel);

                float x = hitInfo.distance - rideHeight;

                float springForce = (x * springConstant) - (rayDownDirVel * dampingConstant);

                rb.AddForce(rayDownDir * springForce);
            }
        }
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
        inputVec.Set(rawInput.x, 0f, rawInput.y);
        currentVel.Set(rb.velocity.x, 0f, rb.velocity.z);
        
        float targetAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        Vector3 unitGoal = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        Vector3 moveGoal = unitGoal * speed;

        currentVec = rb.velocity;

        float magDiff = moveGoal.magnitude - currentVec.magnitude;
        magDiff = Mathf.Clamp(magDiff, 0f, acceleration);

        float angleBetween = Vector3.SignedAngle(moveGoal, currentVec, Vector3.up);
        angleBetween = Mathf.Clamp(angleBetween, 0f, maxAccForce);

        moveGoal = (currentVec.magnitude + magDiff) * unitGoal;

        moveGoal = Quaternion.Euler(0f, angleBetween, 0f) * moveGoal;

        Vector3 diff = moveGoal - currentVec;

        rb.AddForce(diff, ForceMode.VelocityChange);

        /*currentVec = Vector3.MoveTowards(currentVec, moveGoal + currentVel, acceleration * Time.fixedDeltaTime);*/
        
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref dampHolder, turnSmoothTime);
        
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        

        /*Vector3 neededForce = (currentVec - rb.velocity) / Time.fixedDeltaTime;

        neededForce = Vector3.ClampMagnitude(neededForce, maxAccForce);
        
        if (inputVec != Vector3.zero)
        {
            rb.AddForce(neededForce);
        }*/
            

        //switch states
        if(rawInput == Vector2.zero) 
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
        StartCoroutine(FloatLock());
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
        inputVec.Set(rawInput.x, 0f, rawInput.y);
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            //checks if user has stopped inputting values because transfering back to idle state is dependent on speed not input
        if (inputVec != Vector3.zero)
        {
            rb.AddForce(moveDir.normalized * airAcceleration);
        }

        //switch states
        if (onGround && rawInput == Vector2.zero)
        {
            currentState = State.idle;
        }
        else if(onGround && rawInput != Vector2.zero)
        {
            currentState = State.walking;
        }
    }

    public void Start()
    {
        currentVec = new Vector3(0f, 0f, 0f);
        inputVec = new Vector3(0f, 0f, 0f);

        mask = ~LayerMask.GetMask("Player");
    }

    private void Update()
    {
        
    }

    void FixedUpdate()
    {
        //Debug.Log(currentState);

        //on ground detector its gross and stupid and i hate it. thanks roxy <3
        if (!floatlock)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position - Vector3.up * rideHeight, 0.49f, mask);
            if (colliders.Length > 0)
            {
                onGround = true;
            }
            else
            {
                onGround = false;
            }
        }else onGround = false;

        switch (currentState)
        {
            case State.idle:
                doMove();
                doFloat();
                break;

            case State.walking:
                doMove();
                doFloat();
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
