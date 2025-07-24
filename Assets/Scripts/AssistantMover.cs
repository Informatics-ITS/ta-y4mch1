using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssistantMover : MonoBehaviour
{
    public RectTransform targetRectTransform;  // Assign the RectTransform in the Inspector
    public float maxYOffset = 20f;             // Maximum offset for the Y-axis
    public float maxXOffset = 10f;             // Maximum offset for the X-axis
    public float speed = 1f;                   // Speed of translation

    private Vector2 initialPosition;
    private Vector2 targetPosition;

    void Start()
    {
        // Ensure we have a target RectTransform
        if (targetRectTransform == null)
            targetRectTransform = GetComponent<RectTransform>();

        // Store the initial position and set the first target position
        initialPosition = targetRectTransform.anchoredPosition;
        StartCoroutine(SmoothTranslate());
    }

    IEnumerator SmoothTranslate()
    {
        while (true)
        {
            // Set a new random target position within the allowed range
            float randomX = Random.Range(-maxXOffset, maxXOffset);
            float randomY = Random.Range(-maxYOffset, maxYOffset);
            targetPosition = initialPosition + new Vector2(randomX, randomY);

            // Smoothly move towards the target position
            while (Vector2.Distance(targetRectTransform.anchoredPosition, targetPosition) > 0.1f)
            {
                targetRectTransform.anchoredPosition = Vector2.Lerp(
                    targetRectTransform.anchoredPosition,
                    targetPosition,
                    Time.deltaTime * speed
                );
                yield return null;  // Wait for the next frame
            }
        }
    }
}
