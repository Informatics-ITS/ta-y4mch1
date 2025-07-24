using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 directionToCamera = mainCamera.transform.position - transform.position;

        transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
    }
}
