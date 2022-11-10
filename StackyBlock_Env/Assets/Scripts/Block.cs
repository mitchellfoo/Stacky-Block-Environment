using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [Header("Block Variables")]
    public float gravity = 9.8f;

    public enum BlockType { Stack, Compact };
    public BlockType blockType;

    private Rigidbody rb;
    private bool collided = false;

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
        // Initial ground contact behavior
        if (collision.gameObject.CompareTag("Ground"))
        {
            gameObject.tag = collision.gameObject.tag;
            gameObject.layer = LayerMask.NameToLayer("Default");

            // Physics based on block type
            if (blockType == BlockType.Stack) { rb.constraints = RigidbodyConstraints.None; }
            if (blockType == BlockType.Compact)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                gravity = 0f;

                if (!collided)
                {
                    collided = true;

                    // round to int position
                    Vector3 contact = collision.contacts[0].point;
                    Vector3 tmp = transform.position;
                    tmp.y = Mathf.Round(contact.y);
                    transform.position = tmp;
                }
            }
        }

        // General collision behavior
        if (blockType == BlockType.Stack)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
