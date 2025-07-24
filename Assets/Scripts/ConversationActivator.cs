using UnityEngine;
using UnityEngine.InputSystem;

public class ConversationActivator : MonoBehaviour
{
    public GameObject conversationManagerObject;

    private ConversationManager conversationManager;
    private bool hasActivated = false;

    public InputActionReference triggerButton; // Input dari controller

    private bool triggerPressed = false;

    void Start()
    {
        if (conversationManagerObject != null)
        {
            conversationManager = conversationManagerObject.GetComponent<ConversationManager>();
            if (conversationManager != null)
            {
                conversationManager.enabled = false; // Nonaktifkan script ConversationManager
            }
        }
    }

    void Update()
    {
        bool triggerStat = triggerButton.action.ReadValue<float>() > 0.5f;
        bool mKeyStat = Keyboard.current != null && Keyboard.current.mKey.isPressed;

        bool isTriggered = triggerStat || mKeyStat;

        if (!hasActivated && isTriggered && !triggerPressed)
        {
            triggerPressed = true;

            if (conversationManager != null)
            {
                conversationManager.enabled = true; // Aktifkan script ConversationManager
                hasActivated = true;
            }
        }

        if (!isTriggered && triggerPressed)
        {
            triggerPressed = false;
        }
    }
}
