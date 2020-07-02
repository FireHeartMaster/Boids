using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSettings : MonoBehaviour
{
    public float minSpeed = 4f;
    public float maxSpeed = 10f;
    public float visionRadius = 10f;
    public float visionAngle = 240f;

    [Space]
    [Range(0f, 1f)] public float avoidRadiusPercent = 0.3f;

    [Space]
    public bool avoidOthers = true;
    public bool alignWithOthers = true;
    public bool followCenter = true;
    public bool avoidObstacles = true;

    [Space]
    public float avoidOthersWeight = 1f;
    public float alignmentWeight = 1f;
    public float followCenterWeight = 1f;
    public float avoidObstaclesWeight = 50f;

    [Space]
    public int raycastNumPoints = 100;

    public LayerMask obstacleMask;
}
