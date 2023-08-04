using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREnable : MonoBehaviour
{
    [SerializeField] private bool enableVR;

    [SerializeField] private MovementInput steeringWheel; 

    [SerializeField] private List<CoreGrabInteractable> coreGrab;

    [SerializeField] private MovementInput input;

    [SerializeField] private MovementInputVisual lever;

    [SerializeField] private GameObject XROrigin;

    [SerializeField] private GameObject CoreCamera;

    // Start is called before the first frame update
    void Start()
    {
        XROrigin.SetActive(enableVR);
        CoreCamera.SetActive(!enableVR);
        
        steeringWheel.SetVREnabled(enableVR);
        foreach (var grab in coreGrab)
        {
            grab.SetVREnabled(enableVR);
        }
        input.SetVREnabled(enableVR);
        lever.SetVREnabled(enableVR);
    }
}
