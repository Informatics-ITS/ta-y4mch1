using UnityEngine;

public class DampFollower : MonoBehaviour
{
    public Transform target; // The transform to follow
    public float positionDamping = 0.1f; // Damping for position following
    public float rotationDamping = 0.1f; // Damping for rotation following

    private Vector3 velocity = Vector3.zero; // Velocity used by SmoothDamp for position

    private void Update()
    {
        if (target == null) return;

        // Smoothly interpolate the position of the object towards the target's position
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, positionDamping);

        // Smoothly interpolate the rotation of the object towards the target's rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationDamping * Time.deltaTime);
    }
}
