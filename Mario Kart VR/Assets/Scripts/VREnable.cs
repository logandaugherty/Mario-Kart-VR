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
    private List<CoreGrabInteractable> coreGrab;

    [SerializeField]
    private MovementInputVisual lever;



    // Start is called before the first frame update
    void Start()
    {
        steeringWheel.SetVREnabled(enableVR);
        foreach (var grab in coreGrab)
        {
            grab.SetVREnabled(enableVR);
        }
        lever.SetVREnabled(enableVR);
    }
}
