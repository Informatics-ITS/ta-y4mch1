using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.InputSystem;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; }
    [SerializeField] public Stage stage;
    public Stage currentStage;
    private int currentStageIdx = 0;
    public Message currentMessage;
    private int currentMessageIdx = -1;

    [SerializeField] private TextMeshProUGUI subtitle;
    private float typingSpeed = 0.01f;

    [SerializeField] public GameObject assistantPrompt;

    private bool waitForManualTrigger = false;

    [Header("Input Actions")]
    public InputActionReference triggerRight;

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
        subtitle.text = ". . . .";
        currentStage = stage;

        if (!ConversationManager.Instance.CheckNextTriggerType())
        {
            ConversationManager.Instance.NextMessage();
        }
        else
        {
            waitForManualTrigger = true;
            StartCoroutine(WaitForTriggerRight());
        }
    }

    public void TriggerConversation()
    {
        NextMessage();
    }

    public bool CheckNextTriggerType()
    {
        if (currentMessageIdx + 1 >= currentStage.stage.Count) return false;
        return currentStage.stage[currentMessageIdx + 1].externalTrigger;
    }

    public async void NextMessage()
    {
        if (currentMessageIdx + 1 >= currentStage.stage.Count)
        {
            SceneSwitcher.Instance.SwitchSceneEntry();
            return;
        }

        currentMessageIdx++;
        currentMessage = currentStage.stage[currentMessageIdx];

        assistantPrompt.SetActive(false);

        if (currentMessage.isAssistance)
        {
            if (LanguageCentral.Instance.language == "ja")
            {
                AssistantManager.Instance.PlayAssistant(currentMessage.assistanceAudioJp, currentMessage.assistanceLineJp, currentMessage.initialDelay);
            }
            else
            {
                AssistantManager.Instance.PlayAssistant(currentMessage.assistanceAudioId, currentMessage.assistanceLineId, currentMessage.initialDelay);
            }

            if (currentMessage.promptJp != "" && currentMessage.promptId != "")
            {
                TextMeshProUGUI assistantTxt = assistantPrompt.GetComponentInChildren<TextMeshProUGUI>();

                if (assistantTxt != null)
                {
                    assistantPrompt.SetActive(true);

                    if (LanguageCentral.Instance.language == "ja")
                    {
                        assistantTxt.text = currentMessage.promptJp;
                    }
                    else
                    {
                        assistantTxt.text = currentMessage.promptId;
                    }
                }
            }
        }
        else
        {
            if (currentMessage.isContext)
            {
                bool success = await ProceedContext();
                if (success)
                {
                    VoiceRecorder.Instance.PlayRemote();
                }
                else
                {
                    Debug.Log("FAIL CONTEXT");
                }
            }
            else
            {
                TextMeshProUGUI assistantTxt = assistantPrompt.GetComponentInChildren<TextMeshProUGUI>();

                if (assistantTxt != null)
                {
                    assistantPrompt.SetActive(true);

                    if (LanguageCentral.Instance.language == "ja")
                    {
                        assistantTxt.text = currentMessage.promptJp;
                    }
                    else
                    {
                        assistantTxt.text = currentMessage.promptId;
                    }
                }

                VoiceRecorder.Instance.isAllowedToRecord = true;
            }
        }
    }

    public async Task<bool> ProceedContext()
    {
        subtitle.text = ". . . .";

        var response = await ChatCompletionManager.Instance.SendContext(currentMessage);
        Debug.Log("context:" + response);

        bool voiceDone = await TTSManager.Instance.SendReplyTTS(response);

        StartCoroutine(TypeText(response));

        return voiceDone;
    }

    public async Task<bool> ProceedJudgement(string stt)
    {
        subtitle.text = ". . . .";

        var response = await ChatCompletionManager.Instance.SendJudgement(currentMessage, stt);
        Debug.Log("input:" + response);

        string judge, message;
        ParseString(response, out judge, out message);

        if (judge.ToLower() != "bad" && judge.ToLower() != "good")
        {
            return false;
        }

        Debug.Log("judgement:" + judge);

        ScoreManager.Instance.InputScoreHandler(judge);
        bool voiceDone = await TTSManager.Instance.SendReplyTTS(message);
        StartCoroutine(TypeText(message));

        return voiceDone;
    }

    public async Task<bool> ProceedJudgementWithContext(string stt)
    {
        subtitle.text = ". . . .";

        var response = await ChatCompletionManager.Instance.SendJudgementWithContext(currentMessage, stt);
        Debug.Log("input:" + response);

        string judge, message;
        ParseString(response, out judge, out message);

        if (judge.ToLower() != "bad" && judge.ToLower() != "good")
        {
            return false;
        }

        Debug.Log("judgement:" + judge);

        ScoreManager.Instance.InputScoreHandler(judge);
        bool voiceDone = await TTSManager.Instance.SendReplyTTS(message);
        StartCoroutine(TypeText(message));

        return voiceDone;
    }

    public async Task<bool> ProceedJudgementNoReply(string stt)
    {
        subtitle.text = ". . . .";

        var response = await ChatCompletionManager.Instance.SendJudgementNoReply(currentMessage, stt);
        Debug.Log("input:" + response);

        string judge, message;
        ParseString(response, out judge, out message);

        if (judge.ToLower() != "bad" && judge.ToLower() != "good")
        {
            return false;
        }

        Debug.Log("judgement:" + judge);
        ScoreManager.Instance.InputScoreHandler(judge);

        return true;
    }

    public async Task<bool> ProceedJudgementNoJudge(string stt)
    {
        subtitle.text = ". . . .";

        var response = await ChatCompletionManager.Instance.SendNoJudgementInput(currentMessage, stt);
        Debug.Log("input:" + response);

        bool voiceDone = await TTSManager.Instance.SendReplyTTS(response);
        StartCoroutine(TypeText(response));

        return voiceDone;
    }

    private void ParseString(string input, out string judge, out string message)
    {
        Match match = Regex.Match(input, @"\((.*?)\)");

        if (match.Success)
        {
            judge = match.Groups[1].Value;
            message = input.Remove(match.Index, match.Length).Trim();
        }
        else
        {
            judge = string.Empty;
            message = input;
        }
    }

    private IEnumerator TypeText(string input)
    {
        subtitle.text = "";
        foreach (char character in input)
        {
            subtitle.text += character;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator WaitForTriggerRight()
    {
        while (triggerRight != null && triggerRight.action.ReadValue<float>() <= 0.5f)
        {
            yield return null;
        }

        if (waitForManualTrigger)
        {
            waitForManualTrigger = false;
            NextMessage();
        }
    }
}

[System.Serializable]
public class Stage
{
    [SerializeField] public List<Message> stage;
}
