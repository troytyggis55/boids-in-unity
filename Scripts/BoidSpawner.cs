using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Get the hit point and normal of the plane
                Vector3 hitPosition = hit.point;
                Vector3 hitNormal = hit.normal;

                // Instantiate prefab at hit position
                GameObject spawnedPrefab = Instantiate(boidPrefab, hitPosition, Quaternion.LookRotation(hitNormal));
            }
        }
    }
}
