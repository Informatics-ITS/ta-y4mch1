using UnityEngine;

public class BlinkOnlyController : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("Blink Blendshape Name (write exact name)")]
    public string blendshapeNameBlink = "MEREM"; // nama blendshape untuk kedip

    [Header("Blink Settings")]
    public float minBlinkInterval = 3f;
    public float maxBlinkInterval = 7f;
    public float blinkDuration = 0.15f;

    private int blendshapeIndexBlink = -1;

    private float nextBlinkTime;
    private bool isBlinking = false;
    private float blinkTimer = 0f;

    private void Start()
    {
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer belum di-assign!");
            enabled = false;
            return;
        }

        blendshapeIndexBlink = GetBlendshapeIndexByName(blendshapeNameBlink);

        if (blendshapeIndexBlink == -1)
        {
            Debug.LogWarning($"Blendshape '{blendshapeNameBlink}' tidak ditemukan!");
            enabled = false;
            return;
        }

        ScheduleNextBlink();
    }

    private int GetBlendshapeIndexByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;

        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        if (mesh == null) return -1;

        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            if (mesh.GetBlendShapeName(i) == name)
                return i;
        }
        return -1;
    }

    private void Update()
    {
        HandleBlinking();
    }

    private void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(minBlinkInterval, maxBlinkInterval);
    }

    private void HandleBlinking()
    {
        if (blendshapeIndexBlink == -1) return;

        if (isBlinking)
        {
            blinkTimer += Time.deltaTime;
            float halfDuration = blinkDuration / 2f;

            if (blinkTimer < halfDuration)
            {
                float weight = Mathf.Lerp(0f, 100f, blinkTimer / halfDuration);
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexBlink, weight);
            }
            else if (blinkTimer < blinkDuration)
            {
                float weight = Mathf.Lerp(100f, 0f, (blinkTimer - halfDuration) / halfDuration);
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexBlink, weight);
            }
            else
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndexBlink, 0f);
                isBlinking = false;
                blinkTimer = 0f;
                ScheduleNextBlink();
            }
        }
        else
        {
            if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;
                blinkTimer = 0f;
            }
        }
    }
}
