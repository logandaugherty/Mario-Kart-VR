using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

    // ----- Forward
    [Header("Forward Movement")]
    [SerializeField]
    [Tooltip("Maximum velocity possible")]
    private float forwardMaxVelocity;
    [SerializeField]
    [Tooltip("X axis: % velocity/max | Y axis: % torque")]
    private AnimationCurve forwardVelocity;
    [SerializeField]
    [Tooltip("Default: ' Vertical ' | Keyboard Input for moving forward")]
    private string forwardAccelInputAxis;
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
    private SteeringWheel steeringWheel;
    [SerializeField]
    [Tooltip("Default: ' Horizontal ' | Keyboard Input for steering")]
    private string steeringInputAxis;
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
    private float turnVelocity;
    private float turnRotation;
    private float turnRotationLast;

    // ----- Drifting
    [SerializeField]
    private Transform chassis;
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

    // Car chassis physics rigidbody
    private Rigidbody rb;


    private void Start()
    {
        // Grab the rigidbody from the car
        rb = GetComponent<Rigidbody>();

        turnDistance = turnDistanceMax;
    }

    private void SpeedBoost(RaycastHit hit)
    {
        if(hit.collider.CompareTag("Speed Boost"))
        {

            Debug.Log("Speed Boost!");
            rb.velocity = transform.forward * 20;
        }
    }

    private void FixedUpdate()
    {
        // Loop through each wheel position in the vehicle
        foreach (Transform wheelTransform in wheelCollection)
        {
            // Print the name of the currently selected wheel
            if(developer)
                Debug.Log($"Operating on Wheel: {wheelTransform.name}");

            // Create a ray that faces the ground.
            // This is used for determining the distance from the ground
            Ray ray = new Ray(wheelTransform.position, Vector3.down);

            // Create a raycast hit. This will be used for dermining the distance from the car to ground
            RaycastHit hit;

            Vector3 velocity = rb.GetPointVelocity(wheelTransform.position);


            float steeringInput = steeringWheel.GetSteeringWheel();

            // Find the right-facing direction of the wheel, in terms of world-space (not relative to the car)
            Vector3 steeringDirection = wheelTransform.right;


            // Find the X component of this velocity
            float steeringVelocity = Vector3.Dot(velocity, steeringDirection);

            if (wheelTransform.name[0] == 'F')
            {
                // ------------ Drifting
                if (Input.GetKey(driftButton))
                {
                    if (!driftStarted)
                    {
                        driftStarted = true;
                        if(steeringInput > 0)
                            driftRotationTarget = driftRotationBase;
                        else 
                            driftRotationTarget = -driftRotationBase;

                        turnDistance = driftTurnRotationMax;
                    }
                }
                else if(driftStarted)
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

                // ------ Steering

                float targetAngle = turnDistance * steeringInput;

                float deltaAngle = targetAngle - turnRotation;

                float wheelGrip = wheelTransform.name[0] == 'F' ? frontWheelGrip.Evaluate(steeringVelocity) : backWheelGrip.Evaluate(steeringVelocity);

                float wheelRadius = wheelDiameter / 2f;

                float turnAccelleration = (turnStrength * (Time.fixedDeltaTime * deltaAngle * wheelGrip * wheelRadius)) - (turnVelocity * turnDampen);

                turnVelocity += turnAccelleration;

                turnRotation += turnVelocity;

                wheelTransform.Rotate(Vector3.up, turnRotation - turnRotationLast);

                turnRotationLast = turnRotation;
            }


            // If the ground is below the car
            if (Physics.Raycast(ray, out hit)){

                // ---------

                if (hit.distance < 0.75f)
                {
                    SpeedBoost(hit);


                    // ---------- General -----
                    // Print the distance from the vehicle to the ground
                    if (developer)
                        Debug.Log($"Line Found at Value: {hit.distance}");

                    // Grab Universal information

                    // Refer to the wheel line. This will be used for development
                    LineRenderer IndicationLine = wheelTransform.GetComponent<LineRenderer>();

                    // Print the right-hand direction of the wheel
                    if (developer)
                        Debug.Log($"Wheel Right Direction: {wheelTransform.right}");

                    // Determine the amount of traction the wheel should have on the ground
                    // If its a front wheel, give it front traction

                    // ---------- Forward -----
                    // Apply the suspension upwards
                    Vector3 forwardDirection = wheelTransform.forward;

                    forwardDirection *= wheelTransform.name.Contains('R') ? 1 : -1;

                    // Find out the current velocity of the body, at the wheel
                    float forwardVelocity = Vector3.Dot(forwardDirection, velocity);

                    float forwardInput = Input.GetAxis(forwardAccelInputAxis) + steeringWheel.GetForwardPower();

                    float forwardTarget = forwardInput * forwardMaxVelocity;

                    float forwardAccel = (forwardVelocity - forwardLastVelocity) * (Time.fixedDeltaTime/0.015f);

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


                    // ---------- Steering -----

                    // Display this velocity to the line. Commented out if another direction line is used
                    IndicationLine.SetPosition(1, Vector3.right * steeringVelocity);

                    float steeringGrip = wheelTransform.name[0] == 'F' ? frontWheelGrip.Evaluate(steeringVelocity) : backWheelGrip.Evaluate(steeringVelocity);

                    // Determine how much this should change
                    float desiredVelocityChange = -steeringVelocity * steeringGrip;

                    // Apply the accelleration to the object
                    float steeringAccel = desiredVelocityChange / (Time.fixedDeltaTime / 0.15f);

                    // Apply this accelleration in the direction
                    Vector3 steeringForce = rb.mass * steeringAccel * steeringDirection;


                    // ---------- Suspension -----
                    // Apply the suspension upwards
                    Vector3 forceDirection = wheelTransform.up;

                    // Determine the offset
                    // This value will be 0 if the distance from the ground is the wheel diameter
                    float offset = wheelDiameter - hit.distance;

                    // Find out the current velocity of the body, at the wheel
                    float upwardsVelocity = Vector3.Dot(forceDirection, velocity);

                    // Print the velocity to the logger
                    if (developer)
                        Debug.Log($"Current Supporting Velocity: {upwardsVelocity}");

                    // Show the current suspension velocity
                    //IndicationLine.SetPosition(1, Vector3.up * upwardsVelocity);

                    // Determine the force it will take to reach the target
                    Vector3 upwardForce = forceDirection * ((offset * strength) - (upwardsVelocity * dampen));


                    // ---------- Result -----
                    // Summate all forces and apply to vehicle
                    Vector3 finalForce = forwardForce + steeringForce + upwardForce;

                    // Apply forces to the car, at the wheel's position
                    rb.AddForceAtPosition(finalForce, wheelTransform.position);

                    Vector3 wheelShowcase = Vector3.up * (-hit.distance + 0.1f);

                    wheelTransform.GetChild(0).position = wheelTransform.position + wheelShowcase;
                }
            }
        }
                
    }

}
