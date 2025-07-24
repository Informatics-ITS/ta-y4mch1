using UnityEngine;

public class FaceCameraGlobal : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Cache the main camera
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Get the direction from the object to the camera
        Vector3 directionToCamera = (mainCamera.transform.position - transform.position).normalized;

        // Calculate the rotation to face the camera
        Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);

        // Apply the global rotation to the object
        transform.rotation = lookRotation;
    }
}
