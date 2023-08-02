using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementInput : MonoBehaviour
{
    private bool VRMode;

    [SerializeField]
    private InputActionProperty leftHand;

    [SerializeField]
    private Transform leftHandTransform;

    [SerializeField]
    private float maxDistanceSteering;

    [SerializeField]
    private float maxDistanceForward;

    [SerializeField]
    private InputActionProperty leftHandSelect;

    [SerializeField]
    private Transform inputReference;

    private float steering;

    private float forward;

    private bool jump;

    public void SetVREnabled(bool enable)
    {
        VRMode = enable;
    }

    public float GetSteering()
    {
        return steering;
    }

    private Vector3 GetHandPosition()
    {
        //return leftHand.action.ReadValue<Vector3>();
        return leftHandTransform.position;
    }
    private bool GetHandSelect()
    {
        return leftHandSelect.action.triggered;
    }

    private void CalculateSteering()
    {
        if(VRMode)
        {
            float leftHandDistance = Vector3.Dot(GetHandPosition() - inputReference.position, inputReference.right);

            float cappedDistance = Mathf.Clamp(leftHandDistance, -maxDistanceSteering, maxDistanceSteering);

            float scale = cappedDistance / maxDistanceSteering;

            steering = scale;
        }
        else
        {
            steering = Input.GetAxis("Horizontal");
        } 
    }

    public float GetForward()
    {
        return forward;
    }

    private void CalculateForward()
    {
        if(VRMode)
        {
            float leftHandDistance = Vector3.Dot(GetHandPosition() - inputReference.position, inputReference.forward);

            float cappedDistance = Mathf.Clamp(leftHandDistance, -maxDistanceForward, maxDistanceForward);

            float scale = cappedDistance / maxDistanceForward;

            forward = scale;
        }
        else
        {
            forward = Input.GetAxis("Vertical");
        }
    }

    public bool GetJump()
    {
        return jump;
    }

    private void CalculateJump()
    {
        jump = VRMode ? GetHandSelect() : Input.GetKeyDown(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        CalculateSteering();
        CalculateForward();
        CalculateJump();
    }
}
