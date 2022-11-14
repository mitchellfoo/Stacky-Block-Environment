using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceSwitch : MonoBehaviour
{
    public bool collided = false;
    public int index;
    

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Ground") && !collided)
        {
            // Update collision
            collided = true;
            transform.parent.GetComponent<SwitchParent>().UpdateSwitch(index);

            // Give reward for covering a switch
            float height = transform.position.y;
            transform.parent.parent.GetComponent<BuilderAgent>().AddSwitchReward(height);
        }
    }
}
