using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchParent : MonoBehaviour
{
    // List to keep track of switches
    public List<float> switchStates = new();

    // Prefavb for a switch
    public GameObject switchPrefab;

    public void InitializeSwitches(int gridDim, int height)
    {
        int count = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < gridDim; j++)
            {
                for(int k = 0; k < gridDim; k++)
                {
                    // Extend list
                    switchStates.Add(0);

                    // Create new switch prefab
                    Vector3 pos = new(j, i, k);
                    GameObject s = Instantiate(switchPrefab, pos, Quaternion.identity, transform);

                    // Assign switch its index
                    s.GetComponent<SpaceSwitch>().index = count;
                    count++;
                }
            }
        }
    }

    public void UpdateSwitch(int index)
    {
        switchStates[index] = 1;
    }

    public void ResetSwitches()
    {
        // Reset switch states
        for (int i = 0; i < switchStates.Count; i++)
        {
            switchStates[i] = 0;
        }

        // Reset children
        foreach (GameObject childSwitch in transform)
        {
            childSwitch.GetComponent<SpaceSwitch>().collided = false;
        }
    }
}
