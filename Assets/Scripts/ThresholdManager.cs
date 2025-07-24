using UnityEngine;

public class ThresholdManager : MonoBehaviour
{
    public static ThresholdManager Instance { get; private set; }

    public float AmplitudeThreshold { get; private set; } = 0.3f; // default
    public bool IsThresholdSaved { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // pastikan hanya satu instance
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // tetap hidup di semua scene
        }
    }

    public void SetThreshold(float threshold)
    {
        if (IsThresholdSaved) return; // jika sudah kalibrasi, abaikan

        AmplitudeThreshold = threshold;
        IsThresholdSaved = true;

        Debug.Log($" Threshold disimpan : {threshold}");
    }

    public void ResetThreshold()
{
    AmplitudeThreshold = 0.3f;
    IsThresholdSaved = false;
    Debug.Log(" Threshold direset, kalibrasi bisa dilakukan ulang.");
}

}
