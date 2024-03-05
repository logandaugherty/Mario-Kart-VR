using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringLever : ScriptableSteering
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float delta_steering = Input.GetAxis("Horizontal") * 5;
        float delta_forward = Input.GetAxis("Vertical") * 0.01f;

        transform.Rotate(0, delta_steering, 0);
        steering += delta_steering;
        forward += delta_forward;
    }
}
