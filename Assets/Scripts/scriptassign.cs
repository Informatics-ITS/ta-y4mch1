using UnityEngine;
using UnityEngine.InputSystem;

public class ScriptAssign : MonoBehaviour
{
    public InputActionReference toggleAction; // Assign Input Action (e.g. X button)
    public GameObject targetObject;           // GameObject yang ingin ditampilkan/disembunyikan

    private bool state = false;

    private void OnEnable()
    {
        toggleAction.action.performed += OnToggle;
        toggleAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleAction.action.performed -= OnToggle;
        toggleAction.action.Disable();
    }

    private void OnToggle(InputAction.CallbackContext context)
    {
        state = !state;
        if (targetObject != null)
        {
            targetObject.SetActive(state);
        }
    }
}
