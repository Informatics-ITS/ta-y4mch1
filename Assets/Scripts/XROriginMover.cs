using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XROriginMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;

    [Header("Input")]
    public InputActionProperty leftJoystickMove;

    [Header("References")]
    public Transform xrOriginTransform;
    public Transform headTransform;

    void Update()
    {
        Vector2 input = leftJoystickMove.action.ReadValue<Vector2>();

        if (input.sqrMagnitude > 0.01f)
        {
            // Flatten head's forward to avoid vertical movement
            Vector3 forward = headTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = headTransform.right;
            right.y = 0;
            right.Normalize();

            Vector3 direction = forward * input.y + right * input.x;
            xrOriginTransform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
