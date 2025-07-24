using UnityEngine;
using UnityEngine.XR;

public class SmoothMovement : MonoBehaviour
{
    public float speed = 2.0f;      // Movement speed multiplier
    [SerializeField] public Transform cameraTransform; // Reference to the camera's transform

    private Vector2 leftJoystickInput;
    void Start()
    {
        // If no camera is assigned, try to find the main camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("No camera assigned, and no main camera found in the scene.");
            }
        }
    }

    void Update()
    {
        // Get the input from the left joystick (X and Y)
        InputDevice leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftJoystickInput))
        {
            // Calculate the camera-relative forward and right directions
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Ignore vertical component of forward and right vectors
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // Map joystick input to a camera-relative movement vector
            Vector3 moveDirection = (right * leftJoystickInput.x + forward * leftJoystickInput.y);

            // Move the character based on joystick input and speed
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }
}
