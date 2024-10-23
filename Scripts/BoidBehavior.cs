using System.Collections.Generic;
using UnityEngine;

public class BoidBehavior : MonoBehaviour
{
    public static readonly List<BoidBehavior> AllBoids = new List<BoidBehavior>();
    public float senseRadius = 3.0f;
    public float speed = 1.0f;
    public float turnSpeed = 1.0f;
    
    public int rayAmount = 20;
    public float rayStart = 0.5f;

    public float randomMovement = 0.1f;
    
    void Awake()
    {
        AllBoids.Add(this);
    }
    
    void OnDestroy()
    {
        AllBoids.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (BoidBehavior boid in AllBoids)
        {
            float distance = Vector3.Distance(transform.position, boid.transform.position);
            if (distance < senseRadius)
            {
                Debug.DrawLine(transform.position, boid.transform.position, Color.red);
            }
        }
        
        RandomMovement();
        transform.position += transform.forward * (speed * Time.deltaTime);
        DetectEnvironmentArray();
    }
    
    
    
    void RandomMovement()
    {
        transform.forward = Vector3.Lerp(transform.forward, Random.insideUnitSphere, randomMovement * Time
            .deltaTime);
    }
    
    void DetectEnvironmentArray()
    {
        RaycastHit hit;
        Vector3[] directions = DefineRays(rayAmount, rayStart, transform.forward);

        bool hitDetected = false;
        Vector3 avoidVector = Vector3.zero;

        foreach (var direction in directions)
        {
            if (Physics.Raycast(transform.position, direction, out hit, senseRadius))
            {
                // Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
                // Debug.DrawRay(hit.point, hit.normal, Color.blue);
            
                float magnitude = (senseRadius - hit.distance) / senseRadius;

                avoidVector += hit.normal * magnitude;
                hitDetected = true;
            }/*
            else
            {
                Debug.DrawRay(transform.position, direction * senseRadius, Color.green);
            }*/
        }

        if (hitDetected)
        {
            transform.forward += avoidVector * (turnSpeed * Time.deltaTime);
        }
    }
    
    
    Vector3[] DefineRays(int rays, float angle, Vector3 forward)
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

            rayDirections[i - iStart] = Quaternion.LookRotation(forward) * new Vector3(x, y, -z);
        }

        return rayDirections;
    }

}
