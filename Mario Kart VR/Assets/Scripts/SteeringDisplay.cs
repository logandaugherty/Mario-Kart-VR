using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class SteeringDisplay : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI forwardNumber;

    [SerializeField]
    TextMeshProUGUI steeringNumber;

    // Update is called once per frame
    public void UpdateDisplay(float forward, float steering)
    {
        forwardNumber.text = $"{System.Math.Round(forward, 1)}";

        steeringNumber.text = $"{System.Math.Round(steering, 1)}";
    }
}
