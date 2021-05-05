using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancySphere : MonoBehaviour
{
    Rigidbody r;
    public float radius =1f;
    

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        r.AddForce(-Physics.gravity * 4/3 * Mathf.PI * radius * radius * radius);
    }
}
