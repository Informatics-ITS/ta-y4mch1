using UnityEngine;
using UnityEngine.InputSystem;

public class Sprite : MonoBehaviour
{
    public InputActionReference xButtonAction; // Drag dari XR Controller input map
    public GameObject spriteObject;            // Sprite yang ingin ditampilkan

    private void OnEnable()
    {
        xButtonAction.action.performed += OnXButtonPressed;
        xButtonAction.action.canceled += OnXButtonReleased;
        xButtonAction.action.Enable();
    }

    private void OnDisable()
    {
        xButtonAction.action.performed -= OnXButtonPressed;
        xButtonAction.action.canceled -= OnXButtonReleased;
        xButtonAction.action.Disable();
    }

    private void OnXButtonPressed(InputAction.CallbackContext context)
    {
        spriteObject.SetActive(true);
    }

    private void OnXButtonReleased(InputAction.CallbackContext context)
    {
        spriteObject.SetActive(false);
    }
}
