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
    Vector3 targetVec;
    Vector3 velToAdd;
    Vector3 forceToAdd;

    [Header("Movement Sauce")]
    public float acceleration;  
    public float speed;
    public float jumpForce;
    public float maxAccForce;
    public float maxDecForce;
    public float maxAirAccForce;
    public float maxAirDecForce;

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
        currentVec.Set(rb.velocity.x, 0f, rb.velocity.z);
        
        
        float targetAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        Vector3 unitGoal = Quaternion.Euler(0f, targetAngle, 0f) * (Vector3.forward * inputVec.magnitude);
        targetVec = unitGoal * speed;

        velToAdd = targetVec - currentVec;

        forceToAdd = rb.mass * (velToAdd / Time.fixedDeltaTime);

        if(Vector3.Dot(currentVec.normalized, forceToAdd.normalized) < 0.9f && inputVec.magnitude == 0)
        {
            forceToAdd = Vector3.ClampMagnitude(forceToAdd, maxDecForce);
        }
        else
        {
            forceToAdd = Vector3.ClampMagnitude(forceToAdd, maxAccForce);
        }
        
        //rotate model
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref dampHolder, turnSmoothTime);
        if(rawInput.magnitude > 0.05f)
        {
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        /*Debug.DrawRay(rb.position, targetVec, Color.blue, Time.deltaTime);
        Debug.DrawRay(rb.position, currentVec, Color.red, Time.deltaTime);
        Debug.DrawRay(rb.position, forceToAdd, Color.green, Time.deltaTime);*/
        //add force
        rb.AddForce(forceToAdd);

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
        inputVec.Set(rawInput.x, 0f, rawInput.y);
        currentVec.Set(rb.velocity.x, 0f, rb.velocity.z);


        float targetAngle = Mathf.Atan2(rawInput.x, rawInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
        Vector3 unitGoal = Quaternion.Euler(0f, targetAngle, 0f) * (Vector3.forward * inputVec.magnitude);
        targetVec = unitGoal * speed;

        velToAdd = targetVec - currentVec;

        forceToAdd = rb.mass * (velToAdd / Time.fixedDeltaTime);

        if (Vector3.Dot(currentVec.normalized, forceToAdd.normalized) < 0.9f && inputVec.magnitude == 0)
        {
            forceToAdd = Vector3.ClampMagnitude(forceToAdd, maxAirDecForce);
        }
        else
        {
            forceToAdd = Vector3.ClampMagnitude(forceToAdd, maxAirAccForce);
        }



        //rotate model
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref dampHolder, turnSmoothTime);
        if (rawInput.magnitude > 0.05f)
        {
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        /*Debug.DrawRay(rb.position, targetVec, Color.blue, Time.deltaTime);
        Debug.DrawRay(rb.position, currentVec, Color.red, Time.deltaTime);
        Debug.DrawRay(rb.position, forceToAdd, Color.green, Time.deltaTime);*/
        //add force
        rb.AddForce(forceToAdd);

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
