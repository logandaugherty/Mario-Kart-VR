using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
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
    private float decellerateSpeed;

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
    [Tooltip("Default: ' Horizontal ' | Keyboard Input for steering")]
    private string steeringInputAxis;
    [SerializeField]
    [Tooltip("X axis: velocity | Y axis: grip")]
    private AnimationCurve frontWheelGrip;
    [SerializeField]
    [Tooltip("X axis: velocity | Y axis: grip")]
    private AnimationCurve backWheelGrip;
    private float turnVelocity;
    private float turnRotation;
    private float turnRotationLast;
    [SerializeField]
    private float turnStrength;
    [SerializeField]
    private float turnDampen;

    // Car chassis physics rigidbody
    private Rigidbody rb;


    private void Start()
    {
        // Grab the rigidbody from the car
        rb = GetComponent<Rigidbody>();
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

            // If the ground is below the car
            if (Physics.Raycast(ray, out hit)){
                if(hit.distance < 0.75f)
                {
                    // ---------- General -----
                    // Print the distance from the vehicle to the ground
                    if (developer)
                        Debug.Log($"Line Found at Value: {hit.distance}");

                    // Grab Universal information
                    Vector3 velocity = rb.GetPointVelocity(wheelTransform.position);

                    // Refer to the wheel line. This will be used for development
                    LineRenderer IndicationLine = wheelTransform.GetComponent<LineRenderer>();

                    // Print the right-hand direction of the wheel
                    if (developer)
                        Debug.Log($"Wheel Right Direction: {wheelTransform.right}");

                    // Determine the amount of traction the wheel should have on the ground
                    // If its a front wheel, give it front traction

                    // ---------- Forward -----
                    // For
                    Vector3 forwardDirection = transform.forward;

                    float forwardVelocityCurrent = Vector3.Dot(forwardDirection, velocity);

                    if (developer)
                        Debug.Log($"Forward Current Velocity: {forwardVelocityCurrent}");

                    // Determine how much this should change
                    float forwardDesiredVelocityChange = -forwardVelocityCurrent * decellerateSpeed;

                    // Apply the accelleration to the object
                    float forwardAccel = forwardDesiredVelocityChange / (Time.fixedDeltaTime / 0.15f);

                    Vector3 forwardForce = rb.mass * forwardAccel * forwardDirection;

                    float accelInput = Input.GetAxis(forwardAccelInputAxis);

                    if (developer)
                        Debug.Log($"Forward Keyboard Input: {accelInput}");

                    if (Mathf.Abs(accelInput) > 0f)
                    {
                        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(forwardVelocityCurrent) / forwardMaxVelocity);

                        if (developer)
                            Debug.Log($"Forward Normalized Speed: {normalizedSpeed}");

                        float percentageTorque = forwardVelocity.Evaluate(normalizedSpeed) * accelInput;

                        if (developer)
                            Debug.Log($"Forward Percentage Torque: {percentageTorque}");

                        forwardForce = forwardDirection * percentageTorque;

                        if (developer)
                            Debug.Log($"Forward Final Force: {forwardDirection}");
                    }


                    // ---------- Steering -----
                    float steeringInput = Input.GetAxis(steeringInputAxis);

                    // Find the right-facing direction of the wheel, in terms of world-space (not relative to the car)
                    Vector3 steeringDirection = wheelTransform.right;

                    // Find the X component of this velocity
                    float steeringVelocity = Vector3.Dot(velocity, steeringDirection);

                    if (wheelTransform.name[0] == 'F')
                    {
                        float targetAngle = 45 * steeringInput;

                        float deltaAngle = targetAngle - turnRotation;

                        float wheelGrip = wheelTransform.name[0] == 'F' ? frontWheelGrip.Evaluate(steeringVelocity) : backWheelGrip.Evaluate(steeringVelocity);

                        float wheelRadius = wheelDiameter / 2f;

                        float turnAccelleration = (turnStrength * (Time.fixedDeltaTime * deltaAngle * wheelGrip * wheelRadius)) - (turnVelocity * turnDampen);

                        turnVelocity += turnAccelleration;

                        turnRotation += turnVelocity;

                        wheelTransform.Rotate(Vector3.up, turnRotation - turnRotationLast);

                        turnRotationLast = turnRotation;

                    }

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
