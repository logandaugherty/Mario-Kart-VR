using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor.UIElements;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CarPhysics : MonoBehaviour
{

    [SerializeField]
    private bool developer;

    // ----- Chassis
    [Header("Wheels")]
    [SerializeField]
    [Tooltip("Complete collection of car wheels")]
    private List<Transform> wheelCollection;
    [SerializeField]
    [Tooltip("Diameter of the wheels :)")]
    private float wheelDiameter;
    private Transform wheelTransform;
    private Vector3 wheelVelocity;
    private RaycastHit hit;

    // ----- Forward
    [Header("Forward Movement")]
    [SerializeField]
    private float maxCastDistance;
    [SerializeField]
    [Tooltip("Maximum velocity possible")]
    private float forwardMaxVelocity;
    [SerializeField]
    [Tooltip("X axis: % velocity/max | Y axis: % torque")]
    private AnimationCurve forwardVelocity;
    [SerializeField]
    [Tooltip("Idely slowing down speed")]
    private float forwardStrength;
    [SerializeField]
    [Tooltip("Idely slowing down speed")]
    private float forwardDampen;
    [Tooltip("Idely slowing down speed")]
    private float forwardLastVelocity;

    // ----- Suspension
    [Header("Suspension")]
    [SerializeField]
    [Tooltip("Pushing power of suspension")]
    private float strength;
    [SerializeField]
    [Tooltip("Friction power in the suspension")]
    private float dampen;

    // ----- Steering
    [Header("Steering")]
    [SerializeField]
    private GameObject steeringSystem;
    private ScriptableSteering steering_input;
    [SerializeField]
    [Tooltip("X axis: velocity | Y axis: grip")]
    private AnimationCurve frontWheelGrip;
    [SerializeField]
    [Tooltip("X axis: velocity | Y axis: grip")]
    private AnimationCurve backWheelGrip;
    [SerializeField]
    private float turnDistanceMax;
    [SerializeField]
    private float turnStrength;
    [SerializeField]
    private float turnDampen;


    private float turnDistance;
    private float turnRotation;
    private float turnVelocity;
    private float turnRotationLast;

    private float steeringVelocity;
    private Vector3 steeringDirection;

    // ----- Drifting
    [Header("Drifting")]
    [SerializeField]
    private KeyCode driftButton;
    [SerializeField]
    private float driftRotationBase;
    [SerializeField]
    private float driftStrength;
    [SerializeField]
    private float driftDampen;
    [SerializeField]
    private float driftTurnRotationMax;

    private float driftRotationTarget;
    private float driftRotation;
    private float driftVelocity;
    private float driftRotationLast;
    private bool driftStarted;

    // ------ Jumping
    [Header("Jumping")]
    [SerializeField]
    private float jumpSpeed;
    [SerializeField]
    private float jumpChassisOffTime;

    private bool chassisEnabled = true;

    // Car chassis physics rigidbody
    private Rigidbody rb;
    private LineRenderer indicationLine;

    private void Start()
    {
        // Grab the rigidbody from the car
        rb = GetComponent<Rigidbody>();

        foreach(var steering_type in GetComponentsInChildren<ScriptableSteering>())
        {
            if (steering_type.enabled)
            {
                this.steering_input = steering_type;
                Debug.Log($"Steering System: {steering_input.gameObject.name}");
            }
        }

        turnDistance = turnDistanceMax;
    }

    private void SpeedBoost()
    {
        if (hit.collider.CompareTag("Speed Boost"))
        {

            Debug.Log("Speed Boost!");
            rb.velocity = transform.forward * 20;
        }
    }

    private void EnableChassis()
    {
        chassisEnabled = true;
    }

    private void IterateDrift()
    {
        /*
        // ------------ Drifting
        if (Input.GetKey(driftButton))
        {
            if (!driftStarted)
            {
                driftStarted = true;
                if (movementInput.GetSteering() > 0)
                    driftRotationTarget = driftRotationBase;
                else
                    driftRotationTarget = -driftRotationBase;

                turnDistance = driftTurnRotationMax;
            }
        }
        else if (driftStarted)
        {
            driftStarted = false;
            driftRotationTarget = 0;
            turnDistance = turnDistanceMax;
        }
        */
        float targetDriftAngle = driftRotationTarget;

        float deltaDriftAngle = targetDriftAngle - driftRotation;

        float driftAccelleration = (driftStrength * (Time.fixedDeltaTime * deltaDriftAngle)) - (driftVelocity * driftDampen);

        driftVelocity += driftAccelleration;

        driftRotation += driftVelocity;

        wheelTransform.Rotate(Vector3.up, driftRotation - driftRotationLast);

        driftRotationLast = driftRotation;
    }

    private void IterateSteeringTarget()
    {
        float targetAngle = turnDistance * steering_input.GetSteering();
        //float targetAngle = turnDistance * 0;

        /*
        float deltaAngle = targetAngle - turnRotation;

        float wheelGrip = wheelTransform.name[0] == 'F' ? frontWheelGrip.Evaluate(steeringVelocity) : backWheelGrip.Evaluate(steeringVelocity);

        float wheelRadius = wheelDiameter / 2f;

        float turnAccelleration = (turnStrength * (Time.fixedDeltaTime * deltaAngle * wheelGrip * wheelRadius)) - (turnVelocity * turnDampen);

        turnVelocity += turnAccelleration;

        turnRotation += turnVelocity;

        wheelTransform.Rotate(Vector3.up, turnRotation - turnRotationLast);

        turnRotationLast = turnRotation;
        */

        turnRotation = targetAngle;

        wheelCollection[0].Rotate(Vector3.up, turnRotation - turnRotationLast);
        wheelCollection[1].Rotate(Vector3.up, turnRotation - turnRotationLast);

        turnRotationLast = turnRotation;
    }

    private void IterateJump()
    {
        rb.velocity += transform.up * jumpSpeed;
        chassisEnabled = false;
        Invoke(nameof(EnableChassis), jumpChassisOffTime);
    }

    private void IterateDebugLine()
    {
        // ---------- General -----
        // Print the distance from the vehicle to the ground
        if (developer)
            Debug.Log($"Line Found at Value: {hit.distance}");

        // Grab Universal information

        // Print the right-hand direction of the wheel
        if (developer)
            Debug.Log($"Wheel Right Direction: {wheelTransform.right}");

        // Determine the amount of traction the wheel should have on the ground
        // If its a front wheel, give it front traction
    }

    private Vector3 IterateForward()
    {
        // ---------- Forward -----
        // Apply the suspension upwards
        Vector3 forwardDirection = wheelTransform.forward;

        forwardDirection *= wheelTransform.name.Contains('R') ? 1 : -1;

        // Find out the current velocity of the body, at the wheel
        float forwardVelocity = Vector3.Dot(forwardDirection, wheelVelocity);

        float forwardTarget = steering_input.GetForward() * forwardMaxVelocity;

        float forwardAccel = (forwardVelocity - forwardLastVelocity) * (Time.fixedDeltaTime / 0.015f);

        forwardLastVelocity = forwardVelocity;

        // Determine the offset
        // This value will be 0 if the distance from the ground is the wheel diameter
        float forwardVelocityOffset = forwardTarget - forwardVelocity;

        // Print the velocity to the logger
        if (developer)
            Debug.Log($"Current Supporting Velocity: {forwardVelocity}");

        // Show the current suspension velocity
        //IndicationLine.SetPosition(1, Vector3.up * upwardsVelocity);

        // Determine the force it will take to reach the target
        Vector3 forwardForce = forwardDirection * ((forwardVelocityOffset * forwardStrength) - (forwardAccel * forwardDampen));

        return forwardForce;
    }

    private Vector3 IterateSteeringForce()
    {
        // Display this velocity to the line. Commented out if another direction line is used
        indicationLine.SetPosition(1, Vector3.right * steeringVelocity);

        float steeringGrip = wheelTransform.name[0] == 'F' ? frontWheelGrip.Evaluate(steeringVelocity) : backWheelGrip.Evaluate(steeringVelocity);

        // Determine how much this should change
        float desiredVelocityChange = -steeringVelocity * steeringGrip;
        //float desiredVelocityChange = -steeringVelocity;

        // Apply the accelleration to the object
        float steeringAccel = desiredVelocityChange / (Time.fixedDeltaTime / 0.15f);

        // Apply this accelleration in the direction
        Vector3 steeringForce = rb.mass * steeringAccel * steeringDirection;

        return steeringForce;
    }

    private Vector3 IterateSuspension()
    {
        // Apply the suspension upwards
        Vector3 forceDirection = wheelTransform.up;

        // Determine the offset
        // This value will be 0 if the distance from the ground is the wheel diameter
        float offset = wheelDiameter - hit.distance;

        // Find out the current velocity of the body, at the wheel
        float upwardsVelocity = Vector3.Dot(forceDirection, wheelVelocity);

        // Print the velocity to the logger
        if (developer)
            Debug.Log($"Current Supporting Velocity: {upwardsVelocity}");

        // Show the current suspension velocity
        //IndicationLine.SetPosition(1, Vector3.up * upwardsVelocity);

        // Determine the force it will take to reach the target
        Vector3 upwardForce = forceDirection * ((offset * strength) - (upwardsVelocity * dampen));

        return upwardForce;
    }

    bool reseting;
    bool waitingToSettle;
    [SerializeField] GameObject resetObject;
    void BeginReset()
    {
        resetObject.transform.position = transform.position + Vector3.up * 1f;
        reseting = true;
        waitingToSettle = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Invoke(nameof(EndReset),1f);
    }
    void EndReset()
    {
        reseting = false;
    }

    void IterateReset()
    {
        // Move collider hand position to real hand position
        rb.velocity = (resetObject.transform.position - transform.position) / Time.fixedDeltaTime / 10f;

        // Rotate collider hand to real hand rotation
        Quaternion rotationDifference = resetObject.transform.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);

        // Apply difference in angle
        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }

    void PassiveWheelDistance()
    {
        foreach (var wheelTransform in wheelCollection)
        {
            this.wheelTransform = wheelTransform;

            // Create a ray that faces the ground.
            // This is used for determining the distance from the ground
            Ray ray = new(wheelTransform.position, Vector3.down);

            // If the ground is below the car
            if (Physics.Raycast(ray, out var hit, maxCastDistance))
            {
                wheelTransform.GetChild(0).localPosition = Vector3.down * (hit.distance + wheelDiameter / 2f);
            }
            else
            {
                wheelTransform.GetChild(0).localPosition = Vector3.down * maxCastDistance;
            }
        }
    }

    private void FixedUpdate()
    {
        if (chassisEnabled)
        {

            IterateSteeringTarget();

            Ray upsideDownRay = new(transform.position, Vector3.up);

            LayerMask road = LayerMask.GetMask("Default");

            if (reseting)
            {
                IterateReset();
                PassiveWheelDistance();
            }
            else if (Physics.Raycast(transform.position, transform.up, out var hit2, 100f, road))
            {
                if (!waitingToSettle)
                {
                    Debug.Log("Upside down");
                    waitingToSettle = true;
                    Invoke(nameof(BeginReset), 2.5f);
                }
                PassiveWheelDistance();
            }
            else if (!waitingToSettle)
            {
                // Loop through each wheel position in the vehicle
                foreach (var wheelTransform in wheelCollection)
            {
                this.wheelTransform = wheelTransform;

                wheelVelocity = rb.GetPointVelocity(wheelTransform.position);

                // Print the name of the currently selected wheel
                if (developer)
                    Debug.Log($"Operating on Wheel: {wheelTransform.name}");

                // Find the right-facing direction of the wheel, in terms of world-space (not relative to the car)
                steeringDirection = wheelTransform.right;


                // Find the X component of this velocity
                steeringVelocity = Vector3.Dot(wheelVelocity, steeringDirection);

                if (wheelTransform.name[0] == 'F')
                {
                    IterateDrift();
                }


                // Create a ray that faces the ground.
                // This is used for determining the distance from the ground
                Ray ray = new(wheelTransform.position, Vector3.down);
                indicationLine = wheelTransform.GetComponent<LineRenderer>();

                // If the ground is below the car
                if (Physics.Raycast(ray, out var hit, maxCastDistance))
                {
                    this.hit = hit;

                        /*

                    if (movementInput.GetJump())
                    {
                        IterateJump();
                        break;
                    }*/

                    SpeedBoost();

                    IterateDebugLine();

                    Vector3 forwardForce = IterateForward();

                    Vector3 steeringForce = IterateSteeringForce();

                    Vector3 upwardForce = IterateSuspension();

                    // ---------- Result -----
                    // Summate all forces and apply to vehicle
                    Vector3 finalForce = forwardForce + steeringForce + upwardForce;

                    // Apply forces to the car, at the wheel's position
                    rb.AddForceAtPosition(finalForce, wheelTransform.position);

                    Vector3 wheelShowcase = Vector3.up * (-hit.distance + 0.1f);

                    wheelTransform.GetChild(0).localPosition = wheelShowcase;
                }
                else
                {
                    wheelTransform.GetChild(0).localPosition = Vector3.down * maxCastDistance;
                }
            }
            }
        }
    }
}
