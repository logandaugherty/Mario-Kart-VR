using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSteeringLine : MonoBehaviour
{
    [SerializeField]
    private Transform hand;

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        Vector3 lineStart = transform.position;

        lineRenderer.SetPosition(0, lineStart);

        Vector3 lineEnd = hand.position;

        lineRenderer.SetPosition(1, lineEnd);
    }
}
