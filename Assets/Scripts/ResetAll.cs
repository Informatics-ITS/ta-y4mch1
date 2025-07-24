using UnityEngine;

public class ResetAll : MonoBehaviour
{
    [Header("Reset PlayerPrefs juga?")]
    public bool resetScorePrefs = true;

    [Header("Reset Threshold juga?")]
    public bool resetThreshold = true;

    void Start()
    {
        Debug.Log($"[ResetAll] Scene: {gameObject.scene.name}");

        if (resetScorePrefs)
        {
            PlayerPrefs.SetInt("Score", 5);
            PlayerPrefs.SetInt("GoodCounter", 0);
            PlayerPrefs.SetInt("AllCounter", 0);
            Debug.Log("[ResetAll] Score direset ke 5 (Prefs).");

            // Panggil ScoreManager biar heart & UI ikut balik
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetHeartScore();
                Debug.Log("[ResetAll] ScoreManager.Instance.ResetHeartScore() dipanggil.");
            }
            else
            {
                Debug.LogWarning("[ResetAll] ScoreManager.Instance belum aktif! Pastikan ScoreManager ada di scene & sudah aktif.");
            }
        }

        if (resetThreshold)
        {
            if (ThresholdManager.Instance != null)
            {
                ThresholdManager.Instance.ResetThreshold();
                Debug.Log("[ResetAll] ThresholdManager.Instance.ResetThreshold() dipanggil.");
            }
            else
            {
                Debug.LogWarning("[ResetAll] ThresholdManager.Instance belum aktif! Pastikan ThresholdManager ada di scene & sudah aktif.");
            }
        }
    }
}
