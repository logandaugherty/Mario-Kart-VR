using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableSteering : MonoBehaviour
{

    [SerializeField]
    SteeringDisplay SteeringDisplay;

    protected float steering;
    protected float forward;

    public float GetSteering()
    {
        return steering;
    }

    public float GetForward()
    {
        return forward;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SteeringDisplay.UpdateDisplay(forward, steering);
    }
}
