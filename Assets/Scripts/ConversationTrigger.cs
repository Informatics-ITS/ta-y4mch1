using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConversationTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent<bool> switchMovement;
    [SerializeField] private UnityEvent additionalEvent;
    private bool hasEntered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasEntered && ConversationManager.Instance.CheckNextTriggerType())
        {
            switchMovement.Invoke(false);
            additionalEvent.Invoke();

            
            ConversationManager.Instance.TriggerConversation();
            
            hasEntered = true;
        }
    }
}
