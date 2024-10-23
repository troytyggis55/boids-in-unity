using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public Camera mainCamera;

    public int initalSpawnAmount;

    private void Start()
    {
        for (int i = 0; i < initalSpawnAmount; i++)
        {
            Instantiate(boidPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit))
            {
                // Get the hit point and normal of the plane
                Vector3 hitPosition = hit.point;
                Vector3 hitNormal = hit.normal;

                // Instantiate prefab at hit position
                Instantiate(boidPrefab, hitPosition, Quaternion.LookRotation(hitNormal));
            }
        }
    }
}
