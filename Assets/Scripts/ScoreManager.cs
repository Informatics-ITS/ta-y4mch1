using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Cognitive3D;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public HideObject HideObjectInstance;

    public int heart = 5;
    public float fadeDuration = 1f;
    [SerializeField] private List<Image> heartsImg;

    // Track naik turun hati per scene (runtime only)
    public int heartGain = 0;
    public int heartLoss = 0;
    public int TotalInteractions => heartGain + heartLoss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        heart = PlayerPrefs.GetInt("Score", 5);
        SetHeartUI();
    }

    private void SetHeartUI()
    {
        for (int i = 5; i > heart; i--)
        {
            Color color = heartsImg[i - 1].color;
            heartsImg[i - 1].color = new Color(color.r, color.g, color.b, 0f);
        }
    }

    public void InputScoreHandler(string input)
    {
        BlendShapeController.TriggerEmotion(input);

        if (AnimationController.Instance != null)
            AnimationController.Instance.OnPlayerResponse(input);

        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        switch (input.ToLower())
        {
            case "good":
                heartGain++;

                // Global counter
                PlayerPrefs.SetInt("GoodCounter", PlayerPrefs.GetInt("GoodCounter", 0) + 1);
                PlayerPrefs.SetInt("AllCounter", PlayerPrefs.GetInt("AllCounter", 0) + 1);

                // Per scene counter
                PlayerPrefs.SetInt($"SceneGood_{sceneIndex}", PlayerPrefs.GetInt($"SceneGood_{sceneIndex}", 0) + 1);
                PlayerPrefs.SetInt($"SceneAll_{sceneIndex}", PlayerPrefs.GetInt($"SceneAll_{sceneIndex}", 0) + 1);

                // Event Cognitive3D
                new CustomEvent("GoodResponse")
                    .SetProperty("sceneIndex", sceneIndex)
                    .SetProperty("heartBefore", heart)
                    .Send();

                IncrementHeart();
                break;

            case "bad":
                heartLoss++;

                // Global counter
                PlayerPrefs.SetInt("BadCounter", PlayerPrefs.GetInt("BadCounter", 0) + 1);
                PlayerPrefs.SetInt("AllCounter", PlayerPrefs.GetInt("AllCounter", 0) + 1);

                // Per scene counter
                PlayerPrefs.SetInt($"SceneBad_{sceneIndex}", PlayerPrefs.GetInt($"SceneBad_{sceneIndex}", 0) + 1);
                PlayerPrefs.SetInt($"SceneAll_{sceneIndex}", PlayerPrefs.GetInt($"SceneAll_{sceneIndex}", 0) + 1);

                // Event Cognitive3D
                new CustomEvent("BadResponse")
                    .SetProperty("sceneIndex", sceneIndex)
                    .SetProperty("heartBefore", heart)
                    .Send();

                DecrementHeart();
                break;
        }
    }

    private void IncrementHeart()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        if (heart + 1 > 5)
        {
            new CustomEvent("Increment score but maximum")
                .SetProperty("score", heart)
                .Send();
            return;
        }

        heart += 1;
        PlayerPrefs.SetInt("Score", heart);

        // Log to Cognitive3D
        new CustomEvent("Increment score")
            .SetProperty("score", heart)
            .Send();

        // Optional: log jumlah total naik hati per scene
        new CustomEvent("SceneHeartGain")
            .SetProperty("sceneIndex", sceneIndex)
            .SetProperty("totalHeartGain", heartGain)
            .Send();

        StartCoroutine(FadeIn(heart - 1));
    }

    private void DecrementHeart()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        if (heart - 1 < 0)
        {
            new CustomEvent("Decrement score but minimum")
                .SetProperty("score", heart)
                .Send();
            return;
        }

        int prevIdx = heart;
        heart -= 1;
        PlayerPrefs.SetInt("Score", heart);

        // Log to Cognitive3D
        new CustomEvent("Decrement score")
            .SetProperty("score", heart)
            .Send();

        // Optional: log jumlah total turun hati per scene
        new CustomEvent("SceneHeartLoss")
            .SetProperty("sceneIndex", sceneIndex)
            .SetProperty("totalHeartLoss", heartLoss)
            .Send();

        StartCoroutine(FadeOut(prevIdx - 1));
    }

    public IEnumerator FadeOut(int index)
    {
        Color color = heartsImg[index].color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            heartsImg[index].color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        heartsImg[index].color = new Color(color.r, color.g, color.b, 0f);
    }

    public IEnumerator FadeIn(int index)
    {
        Color color = heartsImg[index].color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            heartsImg[index].color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        heartsImg[index].color = new Color(color.r, color.g, color.b, 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetHeartScore();
    }

    public void ResetHeartScore()
    {
        heart = 5;
        heartGain = 0;
        heartLoss = 0;

        PlayerPrefs.SetInt("Score", heart);
        PlayerPrefs.SetInt("GoodCounter", 0);
        PlayerPrefs.SetInt("BadCounter", 0);
        PlayerPrefs.SetInt("AllCounter", 0);

        foreach (var img in heartsImg)
        {
            Color c = img.color;
            img.color = new Color(c.r, c.g, c.b, 1f);
        }
    }
}
