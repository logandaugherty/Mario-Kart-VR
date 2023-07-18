using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [SerializeField]
    private List<Transform> wheelCollection;

    private Rigidbody rb;

    [SerializeField]
    private float strength;

    [SerializeField]
    private float dampen;

    [SerializeField]
    private float wheelDiameter;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        foreach (Transform wheelTransform in wheelCollection)
        {
            Debug.Log($"Operating on Wheel: {wheelTransform.name}");

            RaycastHit hit;
            Ray ray = new Ray(wheelTransform.position, Vector3.down);

            if(Physics.Raycast(ray, out hit)){

                Debug.Log($"Line Found at Value: {hit.distance}");


                LineRenderer YAxis = wheelTransform.GetComponent<LineRenderer>();


                // Add Suspension
                Vector3 forceDirection = transform.up.normalized;

                float offset = wheelDiameter - hit.distance;

                Vector3 velocity = rb.GetPointVelocity(wheelTransform.position);

                float upwardsVelocity = Vector3.Dot(forceDirection, velocity);

                Debug.Log($"Current Supporting Velocity: {upwardsVelocity}");

                YAxis.SetPosition(1, Vector3.up * upwardsVelocity);

                Vector3 upwardForce = forceDirection * ((offset * strength) - (upwardsVelocity * dampen));

                rb.AddForceAtPosition(upwardForce,wheelTransform.position);

            }
        }
    }

}
