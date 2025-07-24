using UnityEngine;

public class ScoreResetOnSceneLoad : MonoBehaviour
{
    [Header("Reset hati otomatis di scene ini")]
    public bool resetScore = true;

    void Start()
    {
        if (resetScore)
        {
            Debug.Log("[ScoreResetOnSceneLoad] Reset hati dan data scene di scene: " + gameObject.scene.name);

            // Reset heart
            PlayerPrefs.SetInt("Score", 5);
            PlayerPrefs.SetInt("GoodCounter", 0);
            PlayerPrefs.SetInt("AllCounter", 0);

            // Reset ScoreManager langsung
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetHeartScore();
            }
            else
            {
                Debug.LogWarning("ScoreManager.Instance is NULL! Pastikan ScoreManager ada di scene ini.");
            }

            // Reset semua data scene
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                PlayerPrefs.DeleteKey($"SceneScore_{i}");
                PlayerPrefs.DeleteKey($"SceneGood_{i}");
                PlayerPrefs.DeleteKey($"SceneBad_{i}");
                PlayerPrefs.DeleteKey($"SceneAll_{i}");
                PlayerPrefs.DeleteKey($"ScenePlayed_{i}");
            }

            PlayerPrefs.Save();
        }
    }
}
