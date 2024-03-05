using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineSampler : MonoBehaviour
{
    [SerializeField]
    private SplineContainer m_splineContainer;

    [SerializeField]
    private int m_splineIndex;
    [SerializeField]
    [Range(0f, 1f)]
    private float m_time;

    float3 position;
    float3 tangent;
    float3 upVector;

    private void Update()
    {
        m_splineContainer.Evaluate(m_splineIndex, m_time, out position, out tangent, out upVector);
    }

    private void OnDrawGizmos()
    {
        Handles.matrix = transform.localToWorldMatrix;
        Handles.SphereHandleCap(0,position, Quaternion.identity, 1f, EventType.Repaint);
    }
}
