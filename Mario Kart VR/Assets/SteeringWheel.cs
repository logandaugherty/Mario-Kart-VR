using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SteeringWheel : MonoBehaviour
{
    [SerializeField]
    private Transform leftHand;
    [SerializeField]
    private Transform xrOrigin;

    [SerializeField]
    private float maxDistanceSteering;

    [SerializeField]
    private float maxDistanceForward;

    public float GetSteeringWheel()
    {
        float leftHandDistance = Vector3.Dot(leftHand.position - transform.position, transform.right);

        float cappedDistance = Mathf.Clamp(leftHandDistance, -maxDistanceSteering, maxDistanceSteering);

        float scale = cappedDistance / maxDistanceSteering;

        return scale;
    }

    public float GetForwardPower()
    {
        float leftHandDistance = Vector3.Dot(leftHand.position - transform.position, transform.forward);

        float cappedDistance = Mathf.Clamp(leftHandDistance, -maxDistanceForward, maxDistanceForward);

        float scale = cappedDistance / maxDistanceForward;

        return scale;
    }
}
