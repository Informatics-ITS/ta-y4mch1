using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OculusToggler : MonoBehaviour
{
    public bool state = false;
    [SerializeField] public UnityEvent<bool> unityEvent;
    private void Update()
    {
        //if (OVRInput.GetDown(OVRInput.Button.Three))
        //{
        //    state = !state;
        //    ToggleEvent();
        //}
    }


    public void ToggleEvent() {
        unityEvent.Invoke(state);
    }
}
