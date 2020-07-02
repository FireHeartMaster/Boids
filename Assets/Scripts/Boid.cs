using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [HideInInspector] public Vector3 avoidOthersDirection;
    [HideInInspector] public Vector3 alignmentDirection;
    [HideInInspector] public Vector3 followCenterDirection;
    Vector3 avoidObstaclesDirection;

    [HideInInspector] public int numberOfNearBoids = 0;

    Vector3 velocity;
    Vector3 currentDirection;

    [SerializeField] BoidSettings settings;
    [SerializeField] BoidManager boidManager;

    bool showPoints = false;

    private void Awake()
    {
        avoidOthersDirection = transform.forward;
        alignmentDirection = transform.forward;
        followCenterDirection = transform.forward;
        avoidObstaclesDirection = transform.forward;

        velocity = transform.forward * (settings.minSpeed + settings.maxSpeed) * 0.5f;
        currentDirection = transform.forward;

        if (BoidHelper.rayDirections == null)
        {
            BoidHelper.Initialize(settings.raycastNumPoints, (1 + Mathf.Sqrt(5f)) * 0.5f);
            showPoints = true;
        }
    }

    Vector3 resultingDirection;

    void SetAvoidOthersDirection()
    {
        float radius = settings.visionRadius * settings.avoidRadiusPercent;
        List<Boid> nearBoids = GetNearBoids(radius);

        if (nearBoids.Count == 0)
        {
            avoidOthersDirection = transform.forward;
            return;
        }

        Vector3 avoidDirection = Vector3.zero;

        foreach (Boid boid in nearBoids)
        {
            float sqrdDistance = (transform.position - boid.transform.position).sqrMagnitude;
            avoidDirection += (transform.position - boid.transform.position) / sqrdDistance;
        }

        avoidOthersDirection = avoidDirection;
    }

    void SetAlignmentDirection()
    {
        Vector3 othersAlignment = Vector3.zero;

        List<Boid> nearBoids = GetNearBoids(settings.visionRadius);
        foreach (Boid boid in nearBoids)
        {
            othersAlignment += boid.transform.forward;
        }

        if (othersAlignment == Vector3.zero)
        {            
            alignmentDirection = transform.forward;
            return;
        }

        alignmentDirection = othersAlignment;
    }

    void SetFollowCenterDirection()
    {
        List<Boid> nearBoids = GetNearBoids(settings.visionRadius);
        
        if (nearBoids.Count == 0)
        {
            followCenterDirection = transform.forward;
            return;
        }

        Vector3 center = Vector3.zero;
        foreach (Boid boid in nearBoids)
        {
            center += boid.transform.position;
        }

        center /= nearBoids.Count;
        Vector3 followDirection = center - transform.position;

        followCenterDirection = followDirection;
    }

    void SetAvoidObstaclesDirection()
    {
        Vector3 avoidDirection = FindUnobstructedDirection();

        avoidObstaclesDirection = avoidDirection;
    }

    Vector3 TurnToDirection(Vector3 currentDirection, Vector3 wishedDirection, float maxAngularSpeed, float turningFactor)
    {        
        if (Vector3.Angle(currentDirection, wishedDirection) > maxAngularSpeed * Time.fixedDeltaTime)
        {
            wishedDirection = Quaternion.AngleAxis(maxAngularSpeed * Time.fixedDeltaTime, Vector3.Cross(currentDirection, wishedDirection)) * currentDirection;
        }

        float angle = Vector3.Angle(currentDirection, wishedDirection);
        float t = ExtensionMethods.Remap(angle, 0f, maxAngularSpeed * Time.fixedDeltaTime, 0f, turningFactor);
        currentDirection = Vector3.Lerp(currentDirection, wishedDirection, t);

        return currentDirection;
    }

    Vector3 TurnTowards(Vector3 newDirection)
    {
        Vector3 offset = newDirection.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(offset, 10f);
    }

    void HandleAllDirections()
    {
        Vector3 acceleration = Vector3.zero;

        if(numberOfNearBoids > 0)
        {
            acceleration += settings.avoidOthers ? TurnTowards(avoidOthersDirection) * settings.avoidOthersWeight : Vector3.zero;
            acceleration += settings.alignWithOthers ? TurnTowards(alignmentDirection) * settings.alignmentWeight : Vector3.zero;
            acceleration += settings.followCenter ? TurnTowards(followCenterDirection) * settings.followCenterWeight : Vector3.zero;
        }
        if(distanceToWall >= 0)
        {
            acceleration += settings.avoidObstacles ? TurnTowards(avoidObstaclesDirection) * settings.avoidObstaclesWeight : Vector3.zero;
        }

        velocity += acceleration * Time.fixedDeltaTime;
        float currentSpeed = velocity.magnitude;
        currentDirection = velocity / currentSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, settings.minSpeed, settings.maxSpeed);
        velocity = currentDirection * currentSpeed;

    }

    void Move()
    {
        transform.position += velocity * Time.fixedDeltaTime;

        transform.forward = currentDirection;
    }

    List<Boid> GetNearBoids(float radius)
    {
        List<Boid> nearBoids = new List<Boid>();

        for (int i = 0; i < boidManager.allBoids.Length; i++)
        {
            if(boidManager.allBoids[i] != this && (boidManager.allBoids[i].transform.position - transform.position).sqrMagnitude < radius * radius && Vector3.Angle(transform.forward, boidManager.allBoids[i].transform.position - transform.position) <= settings.visionAngle)
            {
                nearBoids.Add(boidManager.allBoids[i]);
            }
        }

        return nearBoids;
    }


    private void Update()
    {
        //SetAvoidOthersDirection();
        //SetAlignmentDirection();
        //SetFollowCenterDirection();
        SetAvoidObstaclesDirection();

        HandleAllDirections();

        Move();
    }

    private void OnDrawGizmosSelected()
    {
        if(BoidHelper.rayDirections != null && showPoints)
        {
            for (int i = 0; i < BoidHelper.rayDirections.Length; i++)
            {
                Gizmos.DrawLine(transform.position, transform.position + transform.TransformDirection(BoidHelper.rayDirections[i] * settings.visionRadius));
            }
        }
    }

    float distanceToWall = 0f;
    Vector3 FindUnobstructedDirection()
    {
        Vector3 bestDir = transform.forward;
        float furthestUnobstructedDist = 0f;
        RaycastHit hit;

        distanceToWall = -1f;

        for (int i = 0; i < BoidHelper.rayDirections.Length; i++)
        {
            Vector3 dir = transform.TransformDirection(BoidHelper.rayDirections[i]);
            if(Physics.SphereCast(transform.position, 0.2f, dir, out hit, settings.visionRadius, settings.obstacleMask))
            {
                if (i == 0) distanceToWall = hit.distance;
                if (hit.distance > furthestUnobstructedDist)
                {
                    bestDir = dir;
                    furthestUnobstructedDist = hit.distance;
                }
            }
            else
            {
                return dir;
            }
        }

        return bestDir;
    }

}

public class ExtensionMethods
{
    public static float Remap(float value, float from1, float from2, float to1, float to2)
    {
        return ((value - from1) / (from2 - from1)) * (to2 - to1) + to1;
    }
}


