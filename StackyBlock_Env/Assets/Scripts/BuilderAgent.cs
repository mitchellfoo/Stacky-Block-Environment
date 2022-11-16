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
    public enum BlockType { Stack, Compact };
    public enum ObsType { SwitchGrid, HeightCast };

    [Header("Agent Variables")]
    [Range(0.0f, 50.0f)]
    public int goalHeight = 10;
    public int dropBlockHeight = 30;
    public int stackSeed = 0;
    public int stackSize = 200;

    [Header("Scene Objects")]
    public HeightCast heightCast;
    public int gridDim = 5;
    public GameObject blockParent;
    public GameObject switchParent;
    public ObsType obsType;

    private SwitchParent sp;

    [Header("Blocks Variables")]
    public BlockType blockType;
    public float blockGravity = 9.8f;
    [Tooltip("List of Block Prefabs")]
    public List<GameObject> blocks = new();

    [Header("Reward Variables")]
    public float victoryReward = 100f;
    public float dropBlockReward = 0f;
    public float actionReward = 0f;
    public float heightRewardFactor = 1f;
    public float switchReward = 1f;

    // Tile Stack
    private int[] initialStack;
    private int[] tileStack;

    // Block counting for reward
    private int blockCounter = 0;

    /// <summary>
    /// Init the agent
    /// </summary> 
    public override void Initialize()
    {
        // Initialize seed
        Random.InitState(stackSeed);

        // Initialize tile stack
        tileStack = new int[stackSize];

        for (int i = 0; i < stackSize; i++)
        {
            tileStack[i] = Random.Range(0, blocks.Count);
        }

        initialStack = (int[])tileStack.Clone();

        // Place the height cast at the right height
        heightCast.gameObject.transform.localPosition = new(0, goalHeight+10, 0f);

        // Initialize switch parent
        sp = Instantiate(switchParent, transform).GetComponent<SwitchParent>();
        sp.InitializeSwitches(gridDim, goalHeight);
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

        // Reset switches
        sp.ResetSwitches();

        // Reset tile stack
        tileStack = (int[])initialStack.Clone();

        // Reset currrent heighest height
        heightCast.highestHeight = 0f;

        // Reset block counter
        blockCounter = 0;
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

        // Get action vector
        ActionSegment<int> vectorAction = actions.DiscreteActions;

        // Place from stack given row/col if doing so
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

        // Observe the current 3d grid space (a lot obs)
        if (obsType == ObsType.SwitchGrid) sensor.AddObservation(sp.switchStates);

        // Observe the current board heights (25 obs)
        if (obsType == ObsType.HeightCast) sensor.AddObservation(heightCast.heights);

        // 1 + obsType total observations
        /// 251 with current SwitchGrid setup (1 + 5*5*10)
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
    }

    private void DropBlock(int row, int col)
    {
        // Rreward for taking a step
        AddStepReward();

        // Check new height
        heightCast.CheckHighestHeight();

        // Get current block from stack;
        GameObject block = blocks[tileStack[0]];

        // Send block idx to end of stack
        tileStack = ShiftArray(tileStack);

        // Instantiate block
        Vector3 pos = new(row, dropBlockHeight, col);
        GameObject newBlock = Instantiate(block, pos, Quaternion.identity, blockParent.transform);
        Block currBlock = newBlock.transform.GetComponent<Block>();
        currBlock.blockType = (Block.BlockType)blockType;
        currBlock.gravity = blockGravity;
    }

    public void AddDropBlockReward()
    {
        AddReward(dropBlockReward);
    }

    public void AddVictoryReward()
    {
        AddReward(victoryReward);
    }

    public void AddSwitchReward(float height)
    {
        AddReward(switchReward/(height+1)+0.5f);
    }

    public void AddStepReward()
    {
        blockCounter++;
        AddReward(actionReward * blockCounter);
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
