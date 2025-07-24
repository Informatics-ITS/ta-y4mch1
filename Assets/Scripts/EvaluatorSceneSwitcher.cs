using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cognitive3D;

public class EvaluatorSceneSwitcher : MonoBehaviour
{
    public static EvaluatorSceneSwitcher Instance { get; private set; }

    [SerializeField] private bool isLinear;
    [SerializeField] private string goodRoute;
    [SerializeField] private string badRoute;
    [SerializeField] private float changeDelay = 3f;
    [SerializeField] private float initialDelay = 6f;
    [SerializeField] public Image fadeImage;
    [SerializeField] public Image initialPanel;
    [SerializeField] public GameObject initialCanvas;
    public TextMeshProUGUI initialBriefTxt;
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (fadeImage != null) fadeImage.canvasRenderer.SetAlpha(0);
        if (initialPanel != null && initialBriefTxt != null)
        {
            initialPanel.canvasRenderer.SetAlpha(1);
            initialBriefTxt.alpha = 1;
        }

        SaveSceneScore();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.heart = 5;

        if (IsFinalScene())
        {
            if (initialCanvas != null) initialCanvas.SetActive(true);
            if (initialBriefTxt != null) initialBriefTxt.text = GenerateAllScoresText();

            SendAllScoresToCognitive();
            ResetAllScores();
        }
        else
        {
            StartCoroutine(FadeOutAfterDelay());
        }
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        SwitchSceneEntry();
    }

    public void SwitchSceneEntry()
    {
        if (isLinear)
            SwitchScene(goodRoute);
        else
            SwitchScene((ScoreManager.Instance.heart >= 3) ? goodRoute : badRoute);
    }

    public void SwitchScene(string sceneName)
    {
        StartCoroutine(FadeToBlackAndLoad(sceneName));
    }

    private IEnumerator FadeToBlackAndLoad(string sceneName)
    {
        yield return new WaitForSeconds(changeDelay);
        FadeTo(1f);
        yield return new WaitForSeconds(fadeDuration);
        SceneManager.LoadScene(sceneName);
    }

    private void FadeTo(float alpha)
    {
        if (fadeImage != null)
            fadeImage.CrossFadeAlpha(alpha, fadeDuration, false);
    }

    private void SaveSceneScore()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        int good = ScoreManager.Instance != null ? ScoreManager.Instance.heartGain : 0;
        int bad = ScoreManager.Instance != null ? ScoreManager.Instance.heartLoss : 0;
        int all = good + bad;

        PlayerPrefs.SetInt($"SceneScore_{sceneIndex}", ScoreManager.Instance.heart);
        PlayerPrefs.SetInt($"SceneGood_{sceneIndex}", good);
        PlayerPrefs.SetInt($"SceneBad_{sceneIndex}", bad);
        PlayerPrefs.SetInt($"SceneAll_{sceneIndex}", all);
        PlayerPrefs.SetInt($"ScenePlayed_{sceneIndex}", 1);
        PlayerPrefs.Save();
    }

    private string GenerateAllScoresText()
    {
        string result = "";
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int totalHeart = 0, totalGood = 0, totalAll = 0;
        int sessionNumber = 1;

        for (int i = 0; i < sceneCount; i++)
        {
            if (!PlayerPrefs.HasKey($"SceneScore_{i}")) continue;

            int score = PlayerPrefs.GetInt($"SceneScore_{i}");
            int good = PlayerPrefs.GetInt($"SceneGood_{i}", 0);
            int all = PlayerPrefs.GetInt($"SceneAll_{i}", 0);
            if (i == 0)
            {
                result += $"Hati Default: {score} hati\n";
                // Jangan naikkan sessionNumber
            }
            else
            {
                result += $"Scene {sessionNumber++}: {score} hati\n";
                totalHeart += score;
                totalGood += good;
                totalAll += all;
            }

        }

        float ratio = totalAll > 0 ? (float)totalGood / totalAll : 0f;
        int percent = Mathf.RoundToInt(ratio * 100);
        result += $"\nSkor akhir: {percent}%";

        return result;
    }

        private void SendAllScoresToCognitive()
    {
        string playerID = PlayerPrefs.GetString("PlayerID", "Unknown");
        Cognitive3D_Manager.SetParticipantId(playerID);

        new CustomEvent("PlayerID").SetProperty("PlayerID", playerID).Send();

        int totalGood = 0, totalAll = 0;
        int sessionNumber = 1;
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            if (!PlayerPrefs.HasKey($"SceneScore_{i}")) continue;

            int score = PlayerPrefs.GetInt($"SceneScore_{i}");
            int good = PlayerPrefs.GetInt($"SceneGood_{i}", 0);
            int bad = PlayerPrefs.GetInt($"SceneBad_{i}", 0);
            int all = PlayerPrefs.GetInt($"SceneAll_{i}", 0);

            // Ambil nama scene dari build index
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

            string label;
            if (i == 0)
            {
                label = $"Tutorial: {sceneName}";
            }
            else
            {
                label = $"Session_{sessionNumber++}: {sceneName}";
            }

            new CustomEvent("SceneDetail")
                .SetProperty("SceneIndex", i)
                .SetProperty("Label", label)
                .SetProperty("SceneName", sceneName)
                .SetProperty("HeartScore", score)
                .SetProperty("HeartUp", good)
                .SetProperty("HeartDown", bad)
                .SetProperty("Interactions", all)
                .Send();

            if (i != 0)
            {
                totalGood += good;
                totalAll += all;
            }
        }

        float ratio = totalAll > 0 ? (float)totalGood / totalAll : 0f;
        int finalPercent = Mathf.RoundToInt(ratio * 100);

        new CustomEvent("FinalScoreSummary")
            .SetProperty("TotalGood", totalGood)
            .SetProperty("TotalAll", totalAll)
            .SetProperty("ScoreRatio", $"{totalGood}/{totalAll}")
            .SetProperty("FinalScorePercent", finalPercent)
            .Send();

        Cognitive3D_Manager.SetParticipantProperty("PlayerID", playerID);
        Cognitive3D_Manager.SetParticipantProperty("GoodCounter", totalGood);
        Cognitive3D_Manager.SetParticipantProperty("AllCounter", totalAll);
        Cognitive3D_Manager.SetParticipantProperty("ScoreRatio", $"{totalGood}/{totalAll}");
        Cognitive3D_Manager.SetParticipantProperty("FinalScorePercent", finalPercent);

        Debug.Log($"[Cognitive3D] Summary sent: {totalGood} naik, {totalAll} interaksi, final {finalPercent}%");
    }


    private void ResetAllScores()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            PlayerPrefs.DeleteKey($"SceneScore_{i}");
            PlayerPrefs.DeleteKey($"SceneGood_{i}");
            PlayerPrefs.DeleteKey($"SceneBad_{i}");
            PlayerPrefs.DeleteKey($"SceneAll_{i}");
        }

        PlayerPrefs.DeleteKey("GoodCounter");
        PlayerPrefs.DeleteKey("BadCounter");
        PlayerPrefs.DeleteKey("AllCounter");

        PlayerPrefs.Save();
    }

    private bool IsFinalScene()
    {
        return SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1;
    }
}