using UnityEngine;

public class DampFollowerCanvas : MonoBehaviour
{
    public Transform target; // The transform to follow
    public float positionDamping = 0.1f; // Damping for position following
    public float rotationDamping = 720f; // Damping for rotation following

    private Vector3 velocity = Vector3.zero; // Velocity used by SmoothDamp for position

    private void Update()
    {
        if (target == null) return;

        // Smoothly interpolate the position of the object towards the target's position
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, positionDamping);

        // Get the direction to the main camera
        Vector3 directionToCamera = Camera.main.transform.position - transform.position;
        directionToCamera.y = 0f; // Optional: Lock rotation around the Y-axis

        // Calculate the rotation to face the main camera
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);

        // Smoothly interpolate the rotation of the object towards the calculated rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
    }
}
