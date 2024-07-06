using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class effectStatus : MonoBehaviour
{

    Movement_Juicer nonMovementJuicer;

        IEnumerator Delay(float duration)
    {
        yield return new WaitForSeconds(duration);   
    }

    public void speedEffect(float amount, float duration)
    {
        nonMovementJuicer.speed = nonMovementJuicer.speed * amount;
        //StartCoroutine(Delay(duration));
        Invoke("reset", duration);
    }

    public void Reset()
    {
        nonMovementJuicer.speed = 9.4f;
    }

    // <double jump function idea>
    // MAKE NEW double jump function this will serve as a place holder for future reference


    private void Awake()
    {
        nonMovementJuicer = GetComponent<Movement_Juicer>();
    }

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        
    }
}