using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public float gravity = 9.8f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.AddForce(gravity * rb.mass * new Vector3(0, -1.0f, 0));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.tag = collision.gameObject.tag;
            gameObject.layer = LayerMask.NameToLayer("Default");

            rb.constraints = RigidbodyConstraints.None;
            //rb.constraints = RigidbodyConstraints.FreezeRotationY
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
