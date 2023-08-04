using System.Collections.Generic;
using UnityEngine;

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
    private MovementInput movementInput;
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
        float targetAngle = turnDistance * movementInput.GetSteering();

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

        float forwardTarget = movementInput.GetForward() * forwardMaxVelocity;

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
    bool firstFrameOfAxisReset;
    Vector3 resetPosition;
    short axisIndexReset;

    void BeginReset()
    {
        resetPosition = transform.position + Vector3.up * 2;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        reseting = true;
        firstFrameOfAxisReset = true;
    }

    void IterateReset()
    {
        
        if(firstFrameOfAxisReset)
        {
            Debug.Log("First Frame!");
            firstFrameOfAxisReset = false;
            Invoke(nameof(AxisComplete), 1);
        }
        
        Vector3 targetEuler = transform.eulerAngles;

        if (axisIndexReset == 0)
            targetEuler.x = 0;
        if (axisIndexReset == 1)
            targetEuler.y = 0;
        if (axisIndexReset == 2)
            targetEuler.z = 0;

        if (axisIndexReset == 3)
        {
            waitingToSettle = false;
            reseting = false;
            axisIndexReset = 0;
            rb.isKinematic = false;
            return;
        }

        transform.localEulerAngles = Vector3.Lerp(transform.eulerAngles, targetEuler, 0.1f);
        transform.position = Vector3.Lerp(transform.position, resetPosition, 0.1f);
    }

    void AxisComplete()
    {
        if (reseting)
        {
            axisIndexReset++;
            firstFrameOfAxisReset = true;
        }
    }

    private void FixedUpdate()
    {
        if (chassisEnabled)
        {

            IterateSteeringTarget();

            Ray upsideDownRay = new(transform.position, Vector3.up);
            
            int layerMask = 1 << 6;

            // This would cast rays only against colliders in layer 8.
            // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
            layerMask = ~layerMask;


            if (reseting)
            {
                IterateReset();
            }
            else if (Physics.Raycast(upsideDownRay, out var hit2, Mathf.Infinity, layerMask))
            {
                if (hit2.distance < 0.4f && !waitingToSettle)
                {
                    Debug.Log("Upside down");
                    waitingToSettle = true;
                    Invoke(nameof(BeginReset), 2);
                }
            }
            else if(!waitingToSettle)

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


                // If the ground is below the car
                if (Physics.Raycast(ray, out var hit))
                {
                    this.hit = hit;

                    indicationLine = wheelTransform.GetComponent<LineRenderer>();

                    // ---------

                    if (hit.distance < maxCastDistance)
                    {
                        if (movementInput.GetJump())
                        {
                            IterateJump();
                            break;
                        }

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

                        //wheelTransform.GetChild(0).position = wheelTransform.position + wheelShowcase;
                        wheelTransform.GetChild(0).position = ray.GetPoint(hit.distance);
                    }
                }
            }
        }
    }
}
