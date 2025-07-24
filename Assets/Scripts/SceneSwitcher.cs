using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneSwitcher Instance { get; private set; }

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

    public InputActionReference rightTriggerButton;

    private bool triggerPressed = false;

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
    }

    private void Start()
    {
        SaveSceneScore();
        if (rightTriggerButton != null)
        {
            rightTriggerButton.action.Enable();
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.canvasRenderer.SetAlpha(0); // Optional but safe
        }

        if (initialPanel != null && initialBriefTxt != null)
        {
            initialPanel.canvasRenderer.SetAlpha(1);
            initialBriefTxt.alpha = 1;

            StartCoroutine(SkipCanvasListener());
            StartCoroutine(FadeOutAfterDelay());
        }
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(initialDelay);

        initialPanel.CrossFadeAlpha(0f, fadeDuration, false);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            if (initialBriefTxt != null)
            {
                initialBriefTxt.alpha = alpha;
            }
            yield return null;
        }

        if (initialCanvas != null)
        {
            initialCanvas.SetActive(false);
        }
    }

    public void SwitchSceneEntry()
    {
        if (isLinear)
        {
            SwitchScene(goodRoute);
        }
        else
        {
            SwitchScene((ScoreManager.Instance.heart >= 3) ? goodRoute : badRoute);
        }
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

        if (isLinear)
        {
            SceneManager.LoadScene(goodRoute, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("masuk target scene: " + ((ScoreManager.Instance.heart >= 3) ? goodRoute : badRoute));
            SceneManager.LoadScene((ScoreManager.Instance.heart >= 3) ? goodRoute : badRoute, LoadSceneMode.Single);
        }

        yield return null;
        FadeTo(0f);
        yield return new WaitForSeconds(fadeDuration);
    }

    private void FadeTo(float targetAlpha)
    {
        if (fadeImage != null)
        {
            fadeImage.CrossFadeAlpha(targetAlpha, fadeDuration, false);
        }
    }
    private void SaveSceneScore()
{
    int sceneIndex = SceneManager.GetActiveScene().buildIndex;
    string key = $"SceneScore_{sceneIndex}";
    PlayerPrefs.SetInt(key, ScoreManager.Instance.heart);
    PlayerPrefs.Save();
}


    private IEnumerator SkipCanvasListener()
{
    float elapsed = 0f;

    GameObject skipTextGO = new GameObject("SkipText");
    skipTextGO.transform.SetParent(initialCanvas.transform, false);

    TextMeshProUGUI skipText = skipTextGO.AddComponent<TextMeshProUGUI>();
    skipText.fontSize = 36;
    skipText.alignment = TextAlignmentOptions.Center;
    skipText.color = Color.white;

    RectTransform rect = skipText.GetComponent<RectTransform>();
    rect.sizeDelta = new Vector2(600, 100);
    rect.anchoredPosition = new Vector2(0, -200);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    while (elapsed < initialDelay + fadeDuration)
    {
        bool triggerStat = rightTriggerButton.action.ReadValue<float>() > 0.5f;
        bool mKeyStat = Keyboard.current != null && Keyboard.current.mKey.isPressed;

        bool isTriggered = triggerStat || mKeyStat;

        if (isTriggered && !triggerPressed)
        {
            triggerPressed = true;
            initialCanvas.SetActive(false);
            Destroy(skipTextGO);
            yield break;
        }

        if (!isTriggered && triggerPressed)
        {
            triggerPressed = false;
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    Destroy(skipTextGO);
}
}
