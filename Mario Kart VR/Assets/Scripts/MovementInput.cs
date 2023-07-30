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
    private float maxDistanceSteering;

    [SerializeField]
    private float maxDistanceForward;

    [SerializeField]
    private InputActionProperty leftHandSelect;

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
        return leftHand.action.ReadValue<Vector3>();
    }
    private bool GetHandSelect()
    {
        return leftHandSelect.action.ReadValue<bool>();
    }

    private void CalculateSteering()
    {
        if(VRMode)
        {
            float leftHandDistance = Vector3.Dot(GetHandPosition() - transform.position, transform.right);

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
            float leftHandDistance = Vector3.Dot(GetHandPosition() - transform.position, transform.forward);

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
