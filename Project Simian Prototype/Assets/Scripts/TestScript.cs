using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(GetComponent<MeshRenderer>().renderingLayerMask);
    }

    // Update is called once per frame
    void Update() {
        transform.rotation = Quaternion.AngleAxis(20f * Time.deltaTime, Vector3.up) * transform.rotation;
    }
}
