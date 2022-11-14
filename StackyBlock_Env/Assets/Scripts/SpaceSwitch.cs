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
            collided = true;
            transform.parent.GetComponent<SwitchParent>().UpdateSwitch(index);
        }
    }
}
