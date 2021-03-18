using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float thresh;

    [SerializeField] float springForce;

    new Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(target.position, transform.position) < thresh * rigidbody.velocity.sqrMagnitude) {
            rigidbody.velocity = Vector3.zero;
        } else {
            rigidbody.AddForce((target.position - transform.position) * springForce, ForceMode.Acceleration);
        }
    }
}
