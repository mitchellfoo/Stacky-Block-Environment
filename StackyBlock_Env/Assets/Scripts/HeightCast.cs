using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HeightCast : MonoBehaviour
{
    // Agent
    public BuilderAgent agent;

    // List of ray heights
    public List<float> heights = new();

    // Grid measurements
    public int gridDim = 5;

    // Current highest height
    public float highestHeight = 0f;

    // Display Rays
    public bool debugRays = true;

    // List of raycasts measurements
    private List<Ray> rays = new();

    void Awake()
    {
        // Create grid array of raycasts
        for (int row = 0; row < gridDim; row++)
        {
            for (int col = 0; col < gridDim; col++)
            {
                // Create ray
                int rowAdj = row + (int)transform.position.x;
                int colAdj = col + (int)transform.position.z;

                Vector3 pos = new(rowAdj, transform.position.y, colAdj);
                Ray r = new(pos, -transform.up);
                rays.Add(r);

                // Create empty height value
                heights.Add(0f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            agent.AddVictoryReward();
            agent.EndEpisode();
        }
    }

    public void CheckHighestHeight()
    {
        // Save current highest
        float currentHigh = highestHeight;

        // Update heights list
        for (int i = 0; i < rays.Count; i++)
        {
            // Get ray
            Ray r = rays[i];
            if (debugRays) Debug.DrawRay(r.origin, r.direction * transform.position.y);

            // Get Ray contact
            RaycastHit hitData;
            Physics.Raycast(r, out hitData);
            Vector3 hitPoint = hitData.point;

            // Update
            float height = (float)Math.Round(hitPoint.y, 2);
            heights[i] = height;

            // Check if highest
            if (height > currentHigh) currentHigh = height;
        }

        // Give reward based on the difference between current and old high
        // Update highest high
        if (currentHigh > highestHeight)
        {
            float heightDiff = currentHigh - highestHeight;
            agent.AddHeightReward(heightDiff);

            highestHeight = currentHigh;
            Debug.Log(transform.root.name + " New Highest Height: " + highestHeight);
        }
    }
}
