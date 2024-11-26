using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class BoidBehavior : MonoBehaviour
{
    private static readonly List<BoidBehavior> AllBoids = new List<BoidBehavior>();
    
    public float cohesionWeight = 1.0f;
    public float separationWeight = 1.0f;
    public float alignmentWeight = 1.0f;
    
    public float senseRadius = 1.0f;
        
    public float speed = 1.0f;
    public float reflectBySpeedPercentage = 0.05f;
    public float collisionWeight = 1.0f;
    
    public int rayAmount = 20;
    public float rayStart = 0.5f;

    private readonly int[] _gridPosition = new int[]{0, 0, 0};
    
    public float randomMovement = 0.1f;
    
    private Vector3[] _initialRayDirections;
    private Transform _transform;
    private List<BoidBehavior> _detectedBoids;
    
    void Awake()
    {
        
        AllBoids.Add(this);
        _initialRayDirections = DefineRays(rayAmount, rayStart);
        _transform = transform;
        UpdateCubeGridPosition();
        _detectedBoids = new List<BoidBehavior>();
    }
    
    void OnDestroy()
    {
        AllBoids.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        _detectedBoids.Clear();
        
        foreach (BoidBehavior boid in AllBoids)
        {
            if (boid == this) continue;

            // Check only neighboring boids
            if (_gridPosition[0] - 1 > boid._gridPosition[0] || _gridPosition[0] + 1 < boid._gridPosition[0]) continue;
            if (_gridPosition[1] - 1 > boid._gridPosition[1] || _gridPosition[1] + 1 < boid._gridPosition[1]) continue;
            if (_gridPosition[2] - 1 > boid._gridPosition[2] || _gridPosition[2] + 1 < boid._gridPosition[2]) continue;

            //if (AllBoids[0].Equals(this))
            //{
            //    Debug.DrawRay(_transform.position, boid._transform.position - _transform.position, Color.red);
            //}
            
            float distance = Vector3.Distance(_transform.position, boid._transform.position);
            if (distance < senseRadius)
            {
                //if (AllBoids[0].Equals(this))
                //{
                //    Debug.DrawRay(_transform.position,
                //        boid._transform.position - _transform.position, Color.green);
                //}

                _detectedBoids.Add(boid);
            }
        }
        
        Cohesion(_detectedBoids);
        Separation(_detectedBoids);
        Alignment(_detectedBoids);
        
        RandomMovement();
        DetectEnvironment();
        _transform.position += _transform.forward * (speed * Time.deltaTime);
        UpdateCubeGridPosition();
        
        //Debug.DrawRay(_transform.position, _transform.forward, Color.blue);
    }

    void Cohesion(List<BoidBehavior> detectedBoids)
    {
        if (detectedBoids.Count == 0) return;
        
        Vector3 averagePosition = Vector3.zero;
        foreach (BoidBehavior boid in detectedBoids) averagePosition += boid._transform.position;
        
        averagePosition /= detectedBoids.Count;
        
        Vector3 averagePositionVector = averagePosition - _transform.position;
        _transform.forward = Vector3.Lerp(_transform.forward, averagePositionVector, cohesionWeight * Time.deltaTime);
        //Debug.DrawRay(_transform.position, direction, Color.blue);
    }
    
    void Separation(List<BoidBehavior> detectedBoids)
    {
        if (detectedBoids.Count == 0) return;
        
        Vector3 separationVector = Vector3.zero;
        foreach (BoidBehavior boid in detectedBoids)
        {
            Vector3 direction = _transform.position - boid._transform.position;
            float magnitude = (senseRadius - direction.magnitude) / senseRadius;
            
            separationVector += direction.normalized * magnitude; 
            //Debug.DrawRay(_transform.position, direction, Color.black);
        }
        
        separationVector /= detectedBoids.Count;
        
        _transform.forward = Vector3.Lerp(_transform.forward, separationVector, separationWeight * Time.deltaTime);
        //Debug.DrawRay(_transform.position, separationVector, Color.yellow);
    }
    
    void Alignment(List<BoidBehavior> detectedBoids)
    {
        if (detectedBoids.Count == 0) return;
        
        Vector3 averageDirection = Vector3.zero;
        foreach (BoidBehavior boid in detectedBoids) averageDirection += boid._transform.forward;
        
        averageDirection.Normalize();
        
        _transform.forward = Vector3.Lerp(_transform.forward, averageDirection, alignmentWeight * Time.deltaTime);
        //Debug.DrawRay(_transform.position, averageDirection, Color.green);
    }
    
    void RandomMovement()
    {
        _transform.forward = Vector3.Lerp(_transform.forward, Random.insideUnitSphere, randomMovement * Time
            .deltaTime);
    }
    
    void DetectEnvironment()
    {
        bool hitDetected = false;
        Vector3 avoidVector = Vector3.zero;
        
        foreach (var direction in _initialRayDirections)
        {
            Vector3 transformedDirection = _transform.rotation * direction;
            if (Physics.Raycast(_transform.position, transformedDirection, out var hit, senseRadius))
            {
                //Debug.DrawRay(_transfor.position, transformedDirection * hit.distance, Color.red);
                //Debug.DrawRay(hit.point, hit.normal, Color.blue);
                
                if (hit.distance < reflectBySpeedPercentage * speed)
                {
                    _transform.forward = Vector3.Reflect(_transform.forward, hit.normal);
                    continue;
                }
                

                float magnitude = (senseRadius - hit.distance) / senseRadius;

                avoidVector += hit.normal * magnitude;
                hitDetected = true;
            }
            else
            {
                //Debug.DrawRay(_transform.position, transformedDirection * senseRadius, Color.green);
            }
        }

        if (hitDetected)
        {
            //_transfor.forward += avoidVector * (collisionWeight * Time.deltaTime);
            _transform.forward = Vector3.Lerp(_transform.forward, avoidVector, collisionWeight * Time.deltaTime);
        }
    }
    
    
    Vector3[] DefineRays(int rays, float angle)
    {
        float phi = (1 + Mathf.Sqrt(5)) / 2;

        int iEnd = (int)(rays / angle);
        int iStart = (int)(iEnd * (1 - angle));

        Vector3[] rayDirections = new Vector3[iEnd - iStart];

        for (int i = iStart; i < iEnd; i++)
        {
            float theta = 2 * Mathf.PI * i / phi;

            float z = 1 - (2.0f * i) / (iEnd - 1);
            float radius = Mathf.Sqrt(1 - z * z);

            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);

            rayDirections[i - iStart] = new Vector3(x, y, -z);
        }

        return rayDirections;
    }
    
    private void UpdateCubeGridPosition()
    {
        _gridPosition[0] = (int)Math.Floor(_transform.position.x / senseRadius);
        _gridPosition[1] = (int)Math.Floor(_transform.position.y / senseRadius);
        _gridPosition[2] = (int)Math.Floor(_transform.position.z / senseRadius);
    }
}
