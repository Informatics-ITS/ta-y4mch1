using UnityEngine;
using System.Collections;
public class ScoreReactionHandlerStay : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private string goodBlendshapeName = "Smile";
    [SerializeField] private string badBlendshapeName = "Frown";
    [SerializeField] private float transitionDuration = 0.5f;

    private int goodBlendshapeIndex;
    private int badBlendshapeIndex;
    private int previousHeart;
    private int currentActiveIndex = -1;

    private Coroutine transitionCoroutine;

    private void Start()
    {
        previousHeart = PlayerPrefs.GetInt("Score", 5);

        if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
        {
            Debug.LogError("SkinnedMeshRenderer atau Mesh tidak tersedia.");
            enabled = false;
            return;
        }

        goodBlendshapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(goodBlendshapeName);
        badBlendshapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(badBlendshapeName);

        if (goodBlendshapeIndex == -1 || badBlendshapeIndex == -1)
        {
            Debug.LogError("Blendshape name tidak ditemukan. Periksa nama di Inspector.");
            enabled = false;
            return;
        }

        InvokeRepeating(nameof(CheckScore), 0.5f, 0.5f);
    }

    private void CheckScore()
    {
        int currentHeart = PlayerPrefs.GetInt("Score", 5);

        if (currentHeart > previousHeart)
        {
            SwitchBlendshapeSmooth(goodBlendshapeIndex);
        }
        else if (currentHeart < previousHeart)
        {
            SwitchBlendshapeSmooth(badBlendshapeIndex);
        }

        previousHeart = currentHeart;
    }

    private void SwitchBlendshapeSmooth(int targetIndex)
    {
        if (targetIndex == currentActiveIndex)
            return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(BlendTransition(currentActiveIndex, targetIndex));
        currentActiveIndex = targetIndex;
    }

    private IEnumerator BlendTransition(int fromIndex, int toIndex)
    {
        float elapsed = 0f;

        float fromStart = (fromIndex != -1) ? skinnedMeshRenderer.GetBlendShapeWeight(fromIndex) : 0f;
        float toStart = skinnedMeshRenderer.GetBlendShapeWeight(toIndex);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            if (fromIndex != -1)
                skinnedMeshRenderer.SetBlendShapeWeight(fromIndex, Mathf.Lerp(fromStart, 0f, t));

            skinnedMeshRenderer.SetBlendShapeWeight(toIndex, Mathf.Lerp(toStart, 100f, t));

            yield return null;
        }

        if (fromIndex != -1)
            skinnedMeshRenderer.SetBlendShapeWeight(fromIndex, 0f);

        skinnedMeshRenderer.SetBlendShapeWeight(toIndex, 100f);
    }
}
