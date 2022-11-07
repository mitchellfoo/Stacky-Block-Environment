using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Danger : MonoBehaviour
{
    public BuilderAgent agent;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Block")) agent.AddDropBlockReward();
        Debug.Log("Block Dropped");
        Destroy(collision.gameObject);
    }
}
