using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaleBlink : MonoBehaviour
{
    public RectTransform rectTransform; // The RectTransform to scale
    public float scaleUpMultiplier = 1.1f; // The multiplier for scaling up
    public float scaleSpeed = 2f; // Speed of the scaling transition

    private Vector3 initialScale; // The original scale of the RectTransform
    private Vector3 targetScale; // The target scale for scaling up
    private bool scalingUp = true; // Whether the RectTransform is scaling up
    private float t = 0f; // Lerp parameter

    private void Start()
    {
        // Store the initial scale
        initialScale = rectTransform.localScale;
        // Calculate the target scale
        targetScale = initialScale * scaleUpMultiplier;
    }

    private void Update()
    {
        // Increment or decrement the lerp parameter based on scaling direction
        t += (scalingUp ? 1 : -1) * scaleSpeed * Time.deltaTime;

        // Clamp the lerp parameter between 0 and 1
        t = Mathf.Clamp01(t);

        // Lerp between the initial scale and target scale
        rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, t);

        // Reverse the scaling direction when reaching either end
        if (t >= 1f)
        {
            scalingUp = false;
        }
        else if (t <= 0f)
        {
            scalingUp = true;
        }
    }
}