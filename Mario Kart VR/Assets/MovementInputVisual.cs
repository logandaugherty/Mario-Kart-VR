using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovementInputVisual : MonoBehaviour
{
    bool VREnabled;

    [SerializeField]
    Transform hand;

    MovementInput input;

    [SerializeField]
    TextMeshProUGUI forwardText;

    [SerializeField]
    TextMeshProUGUI steeringText;

    [SerializeField]
    Transform leverVisual;

    public void SetVREnabled(bool enable)
    {
        VREnabled = enable;
    }

    private void Start()
    {
        input = GetComponent<MovementInput>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        forwardText.text = $"{System.Math.Round(input.GetForward(), 1)}";

        steeringText.text = $"{System.Math.Round(input.GetSteering(), 1)}";

        if (VREnabled)
        {
            leverVisual.LookAt(hand);
        }
    }
}
