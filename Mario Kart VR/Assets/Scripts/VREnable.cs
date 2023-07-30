using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREnable : MonoBehaviour
{
    [SerializeField]
    private bool enableVR;

    [SerializeField]
    private MovementInput steeringWheel;

    [SerializeField]
    private CoreGrabInteractable coreGrab;

    // Start is called before the first frame update
    void Start()
    {
        steeringWheel.SetVREnabled(enableVR);
        coreGrab.SetVREnabled(enableVR);
    }
}
