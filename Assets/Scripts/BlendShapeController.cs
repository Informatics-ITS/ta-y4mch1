using System.Collections;
using UnityEngine;

public class BlendShapeController : MonoBehaviour
{
    [Header("Blendshape Setup")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private string happyBlendShapeName = "Happy";
    [SerializeField] private string sadBlendShapeName = "SAD";
    [SerializeField] private string blinkBlendShapeName = "MEREM";
    
    [Header("Timing")]
    [SerializeField] private float animSpeed = 2f;
    [SerializeField] private float holdTime = 1.5f;

    private void Start()
    {
        // Listen to ScoreManager input
        if (ScoreManager.Instance != null)
        {
            // Hook into the existing InputScoreHandler method
            StartCoroutine(ListenToScoreManager());
        }
    }

    private IEnumerator ListenToScoreManager()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            // This will be triggered by our custom method
        }
    }

    public void OnPlayerResponse(string response)
    {
        if (response.ToLower() == "good")
        {
            StartCoroutine(PlayEmotionSequence(happyBlendShapeName));
        }
        else if (response.ToLower() == "bad")
        {
            StartCoroutine(PlayEmotionSequence(sadBlendShapeName));
        }
    }

    private IEnumerator PlayEmotionSequence(string emotionBlendShape)
    {
        // Blink
        yield return StartCoroutine(AnimateBlendShape(blinkBlendShapeName, 100f));
        yield return StartCoroutine(AnimateBlendShape(blinkBlendShapeName, 0f));
        
        // Show emotion
        yield return StartCoroutine(AnimateBlendShape(emotionBlendShape, 100f));
        yield return new WaitForSeconds(holdTime);
        
        // Blink and reset
        yield return StartCoroutine(AnimateBlendShape(blinkBlendShapeName, 100f));
        yield return StartCoroutine(AnimateBlendShape(emotionBlendShape, 0f));
        yield return StartCoroutine(AnimateBlendShape(blinkBlendShapeName, 0f));
    }

    private IEnumerator AnimateBlendShape(string blendShapeName, float targetValue)
    {
        int index = GetBlendShapeIndex(blendShapeName);
        if (index == -1) yield break;

        float currentValue = skinnedMeshRenderer.GetBlendShapeWeight(index);
        float time = 0f;
        float duration = 1f / animSpeed;

        while (time < duration)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(currentValue, targetValue, time / duration);
            skinnedMeshRenderer.SetBlendShapeWeight(index, value);
            yield return null;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(index, targetValue);
    }

    private int GetBlendShapeIndex(string name)
    {
        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            if (mesh.GetBlendShapeName(i) == name) return i;
        }
        return -1;
    }

    // Call this from ScoreManager.InputScoreHandler
    public static void TriggerEmotion(string input)
    {
        BlendShapeController controller = FindObjectOfType<BlendShapeController>();
        if (controller != null)
        {
            controller.OnPlayerResponse(input);
        }
    }
}