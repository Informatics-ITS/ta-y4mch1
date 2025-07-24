using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cognitive3D;

public class CustomEventTrigger : MonoBehaviour
{
    public void SendCustomEvent(string eventName){
        new Cognitive3D.CustomEvent(eventName)
            .Send();
    }
}
