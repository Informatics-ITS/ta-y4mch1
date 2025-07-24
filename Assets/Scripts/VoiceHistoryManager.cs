using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

[System.Serializable]
public class VoiceHistoryEntry
{
    public string text;
    public DateTime timestamp;
    public string formattedTime;

    public VoiceHistoryEntry(string voiceText)
    {
        text = voiceText;
        timestamp = DateTime.Now;
        formattedTime = timestamp.ToString("HH:mm:ss");
    }
}

public class VoiceHistoryManager : MonoBehaviour
{
    public static VoiceHistoryManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject historyCanvas;
    [SerializeField] private TextMeshProUGUI historyTextUI;
    [SerializeField] private float scrollStep = 50f;

    [Header("History Settings")]
    [SerializeField] private int maxHistoryItems = 100;

    [Header("Controller Input (Input System)")]
    public InputActionReference toggleHistoryAction;  // trigger kiri
    public InputActionReference scrollUpAction;       // Y
    public InputActionReference scrollDownAction;     // X

    private List<VoiceHistoryEntry> voiceHistory = new List<VoiceHistoryEntry>();
    private bool isVisible = false;
    private RectTransform textRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        if (historyCanvas != null)
            historyCanvas.SetActive(false);

        // Controller input setup
        if (toggleHistoryAction != null)
        {
            toggleHistoryAction.action.performed += ctx => ToggleHistory();
            toggleHistoryAction.action.Enable();
        }

        if (scrollUpAction != null)
        {
            scrollUpAction.action.performed += ctx => ScrollText(up: true);
            scrollUpAction.action.Enable();
        }

        if (scrollDownAction != null)
        {
            scrollDownAction.action.performed += ctx => ScrollText(up: false);
            scrollDownAction.action.Enable();
        }

        if (historyTextUI != null)
            textRect = historyTextUI.GetComponent<RectTransform>();
    }

    private void Update()
    {
   
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHistory();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ScrollText(true); // scroll up
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ScrollText(false); // scroll down
        }
    }

    public void AddVoiceHistory(string voiceText)
    {
        if (string.IsNullOrEmpty(voiceText)) return;

        VoiceHistoryEntry newEntry = new VoiceHistoryEntry(voiceText);
        voiceHistory.Add(newEntry);

        if (voiceHistory.Count > maxHistoryItems)
            voiceHistory.RemoveAt(0);

        RefreshHistoryText();
    }

    private void RefreshHistoryText()
    {
        if (historyTextUI == null) return;

        historyTextUI.text = "";

        foreach (var entry in voiceHistory)
        {
            historyTextUI.text += $"[{entry.formattedTime}] {entry.text}\n";
        }

        if (textRect != null)
            textRect.anchoredPosition = Vector2.zero;
    }

    public void ToggleHistory()
    {
        isVisible = !isVisible;

        if (historyCanvas != null)
            historyCanvas.SetActive(isVisible);

        if (isVisible)
            RefreshHistoryText();

        Debug.Log($"Voice history {(isVisible ? "shown" : "hidden")}");
    }

    private void ScrollText(bool up)
    {
        if (!isVisible || textRect == null) return;

        Vector2 pos = textRect.anchoredPosition;
        pos.y += up ? -scrollStep : scrollStep;
        textRect.anchoredPosition = pos;
    }

    private void OnDestroy()
    {
        if (toggleHistoryAction != null) toggleHistoryAction.action.Disable();
        if (scrollUpAction != null) scrollUpAction.action.Disable();
        if (scrollDownAction != null) scrollDownAction.action.Disable();
    }

    public void ClearHistory()
    {
        voiceHistory.Clear();
        if (historyTextUI != null)
            historyTextUI.text = "";
    }
}
