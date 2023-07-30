using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CoreGrabInteractable : MonoBehaviour
{
    private bool VREnabled;

    [SerializeField]
    private Transform collectibleAnchor;

    [SerializeField]
    private Rigidbody chassisRb;

    private Rigidbody rb;

    private bool activated;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetVREnabled(bool enable)
    {
        VREnabled = enable;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Chassis") && !VREnabled)
        {
            Debug.Log($"Collectable picked up!");
            activated = true;
        }
    }

    private void FixedUpdate()
    {
        if(activated)
        {
            // Move collider hand position to real hand position
            rb.velocity = (collectibleAnchor.position - transform.position) / Time.fixedDeltaTime;

            // Rotate collider hand to real hand rotation
            Quaternion rotationDifference = collectibleAnchor.rotation * Quaternion.Inverse(transform.rotation);
            rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

            // Apply difference in angle
            Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
            rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);


            if (Input.GetKeyDown(KeyCode.E))
            {
                activated = false;
                rb.velocity = chassisRb.velocity + collectibleAnchor.forward * 15f;
            }
        }
    }
}
