using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementInputVisual : MonoBehaviour
{
    private bool VREnabled;

    [SerializeField]
    private Transform hand;

    public void SetVREnabled(bool enable)
    {
        VREnabled = enable;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(VREnabled)
        {
            transform.LookAt(hand);
        }
    }
}
