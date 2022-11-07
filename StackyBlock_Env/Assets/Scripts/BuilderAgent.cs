using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BuilderAgent : Agent
{
    [Header("Agent Variables")]
    [Range(0.0f, 50.0f)]
    public int goalHeight = 25;
    public int dropBlockHeight = 30;
    public int stackSeed = 0;
    public int stackSize = 200;

    [Header("Scene Objects")]
    public HeightCast heightCast;
    public int gridDim = 5;
    public GameObject blockParent;

    [Header("Possible Blocks")]
    [Tooltip("List of Block Prefabs")]
    public List<GameObject> blocks = new();

    [Header("Reward Variables")]
    public float victoryReward = 100f;
    public float dropBlockReward = -1f;
    public float stepReward = -.1f;
    public float heightRewardFactor = 1f;

    // Tile Stack
    private int[] initialStack;
    private int[] tileStack;

    /// <summary>
    /// Init the agent
    /// </summary> 
    public override void Initialize()
    {
        // Initialize seed
        Random.InitState(stackSeed);

        // Initialize tile stack
        tileStack = new int[stackSize];
        initialStack = tileStack;

        for (int i = 0; i < stackSize; i++)
        {
            tileStack[i] = Random.Range(0, blocks.Count);
        }
    }

    /// <summary>
    /// Reset agent and scene when the episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Clear the board
        foreach (Transform block in blockParent.transform)
        {
            GameObject.Destroy(block.gameObject);
        }

        // Reset tile stack
        tileStack = initialStack;

        // Reset currrent heighest height
        heightCast.highestHeight = 0f;
    }

    /// <summary>
    /// Called when action is recieved from either player or neural network
    ///
    /// vectorAction[i] represents:
    /// Index 0: take action or not 
    /// Index 1: column to place the block
    /// Index 2: row to place the block
    /// </summary>
    /// <param name="actions">Actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Don't take actions if frozen
        //if (frozen) return;

        // Penalty for taking a step
        AddStepReward();

        // Get action vector
        ActionSegment<int> vectorAction = actions.DiscreteActions;

        // Place from stack given row/col if doing so
        //if (vectorAction[0] == 1) { DropBlock(vectorAction[1], vectorAction[2]); }

        int row = vectorAction[0] + (int)transform.position.x;
        int col = vectorAction[1] + (int)transform.position.z;

        DropBlock(row, col);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">Vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe agent's next block to place (1 obs)
        sensor.AddObservation(tileStack[0]);

        // Observe the current boared (25 obs)
        sensor.AddObservation(heightCast.heights);

        // 26 total observations
        Debug.Log("Sensor obs: " + sensor.ToString());

    }

    public override void Heuristic(in ActionBuffers actionsOutBuffer)
    {
        // Get action vector
        ActionSegment<int> actionsOut = actionsOutBuffer.DiscreteActions;

        /*
        // On key press drop a block
        if (Input.GetKey(KeyCode.Space)) actionsOut[0] = 1;
        else actionsOut[0] = 0;
        */

        // For now, pick random row/col values
        int row = Random.Range(0, gridDim);
        int col = Random.Range(0, gridDim);

        actionsOut[0] = row;
        actionsOut[1] = col;

        Debug.Log("Heuristic Action: " + actionsOut.ToString());
    }

    private void DropBlock(int row, int col)
    {
        // (Negative) reward for taking a step
        AddStepReward();

        // Get current block from stack;
        GameObject block = blocks[tileStack[0]];

        // Send block idx to end of stack
        tileStack = ShiftArray(tileStack);

        // Instantiate block
        Vector3 pos = new(row, dropBlockHeight, col);
        Instantiate(block, pos, Quaternion.identity, blockParent.transform);
    }

    public void AddDropBlockReward()
    {
        AddReward(dropBlockReward);
    }

    public void AddVictoryReward()
    {
        AddReward(victoryReward);
    }

    public void AddStepReward()
    {
        // Deduct a small reward if goal has not been reached
        if (heightCast.highestHeight < goalHeight) AddReward(stepReward);
        else EndEpisode();

        // Check new height
        heightCast.CheckHighestHeight();
    }

    public void AddHeightReward(float heightDiff)
    {
        AddReward(heightDiff * heightRewardFactor);
    }

    private int[] ShiftArray(int[] numbers)
    {
        int size = numbers.Length;
        int[] shiftNums = new int[size];

        for (int i = 0; i < size; i++)
        {
            shiftNums[i] = numbers[(i + 1) % size];
        }
        return shiftNums;
    }
}