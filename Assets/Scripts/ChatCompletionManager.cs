using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.UI;
using System.Threading.Tasks;
using Cognitive3D;

public class ChatCompletionManager : OpenAIParent{
    public static ChatCompletionManager Instance { get; private set; }
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;
    [SerializeField] private GameObject thinkingIndicator;

    private float height;
    private List<ChatMessage> messages = new List<ChatMessage>();
    private string enPrompt = "This is a game chat simulation that will be converted as voice to play. This is just an education game to give experience to the player how to properly interact and take care of senior dementia patient. Act as a senior dementia patient in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";
    private string idPrompt = "Ini adalah simulasi obrolan permainan yang akan dikonversi menjadi suara untuk dimainkan. Ini hanyalah permainan edukasi untuk memberikan pengalaman kepada pemain tentang cara berinteraksi dan merawat pasien demensia senior dengan benar. Bertindaklah sebagai pasien demensia senior dalam ruang obrolan dan jawab pertanyaan-pertanyaan. Jangan keluar dari karakter. Jangan pernah menyebutkan bahwa Anda adalah model AI.";
    private string jpPrompt = "これはゲームチャットシミュレーションであり、音声に変換されて再生されます。これは、プレイヤーに高齢の認知症患者と適切に交流し、ケアする方法を経験させるための教育ゲームです。チャットルームで高齢の認知症患者として行動し、質問に答えてください。キャラクターを崩さないようにしてください。AIモデルであることを決して言わないでください。";


    private string bufPrompt;
    private int convoIndex = 0;

    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        bufPrompt = "This is a game chat simulation that will be converted as voice to play. This is just an education game to give experience to the player how to properly interact and take care of senior dementia patient. Don't break character. Don't ever mention that you are an AI model. Act as a CONFUSED AND JITTERY senior dementia patient in the chat room and follow the guidance. In the user role message, there will be 2 sub-role <developer> and <player>, the <developer> will give you context and guidance, while <player> will be the game player. If <developer> ask you to judge on <player> input, then give label '(good)', '(bad)', or '(out of context)' according to <player> input followed by your message response in " + /*LanguageCentral.Instance.GetFullLanguageName()*/"indonesian" + ". For example: '(good) oh dear you are so kind'. Please give short general response that is NOT too creative or NOT too narrow scoped, since the <developer> will give you the sub-topic for the next conversation.";
    }

    private void AppendMessage(ChatMessage message)
    {
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);

        item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
        item.anchoredPosition = new Vector2(0, -height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        height += item.sizeDelta.y;
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        scroll.verticalNormalizedPosition = 0;
    }

    public async Task Start(){
        var newMessage = new ChatMessage()
        {
            Role = "system",
            Content = ""
        };
        
        AppendMessage(newMessage);

        //if (messages.Count == 0) newMessage.Content = (language == "jp") ?  jpPrompt : idPrompt + "\n"; 
        if (messages.Count == 0) newMessage.Content = bufPrompt;
        
        messages.Add(newMessage);

        Debug.Log("BUF PROMPT: "+ messages[0].Content);
    }

    public async Task<string> SendReply(string text){
    var newMessage = new ChatMessage()
    {
        Role = "user",
        Content = text
    };
    
    AppendMessage(newMessage);
    messages.Add(newMessage);

    // TAMPILKAN THINKING INDICATOR
    thinkingIndicator?.SetActive(true);

    var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
    {
        Model = "gpt-4o",
        Messages = messages
    });

    // SEMBUNYIKAN THINKING INDICATOR
    thinkingIndicator?.SetActive(false);

    if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
    {
        var message = completionResponse.Choices[0].Message;
        message.Content = message.Content.Trim();
        
        messages.Add(message);
        AppendMessage(message);

        return message.Content;
    }
    else
    {
        Debug.LogWarning("No text was generated from this prompt.");
        return "";
    }
}


    public async Task<string> SendContext(Message input){
        Debug.Log("MASUK :" + input.intendedSubtopic);

        string bufLanguage = LanguageCentral.Instance.GetFullLanguageName();
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = $"<developer> : for this sub-conversation the intended sub-topic is '{input.intendedSubtopic}', please generate message with this sub-topic with {bufLanguage} language, tone, mood from player's previous messages;"
        };
        
        AppendMessage(newMessage);
        
        messages.Add(newMessage);
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o",
            Messages = messages
        });

        new Cognitive3D.CustomEvent("Send context prompt to OpenAI")
            .SetProperty("content", newMessage.Content)
            .Send();

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            messages.Add(message);
            AppendMessage(message);

            return message.Content;
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return "ERROR";
        }
    }

    public async Task<string> SendJudgement(Message input, string response){
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = $"<developer> : you will get the player response, if player response is in japanese understand it and also reply in japanese, the example good response from player is '{input.expectedGoodMessage}', and the bad example is '{input.expectedBadMessage}', please judge the player response is inclined as '(good)' or '(bad)' according to example followed by SHORT general response message, if you feel the player response is out of topic then just label it '(out of context)' without followed by response message; <player> : {response};"
        };

        AppendMessage(newMessage);
        
        messages.Add(newMessage);
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o",
            Messages = messages
        });

        new Cognitive3D.CustomEvent("Send judgement prompt to OpenAI")
            .SetProperty("content", newMessage.Content)
           .Send();

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            messages.Add(message);
            AppendMessage(message);

            return message.Content;
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return "ERROR";
        }
    }

        public async Task<string> SendJudgementWithContext(Message input, string response)
{
    var newMessage = new ChatMessage()
    {
        Role = "user",
        Content = $@"
<developer> :
Here is the brief context: '{input.intendedSubtopic}'.
You will get the player response.

Your job:
- Use the GOOD and BAD examples only as flexible references — do NOT do literal string match only.
- Always judge based on the overall intention, tone, and politeness in the given context.
- Judge from the perspective of a sensitive, suspicious dementia patient, not a normal adult.

Judgement rules:
- If the player’s response is respectful, supportive, and relevant to the situation, label it '(good)'.
- If the player’s response is rude, harsh, humiliating, or disrespectful, label it '(bad)'.
- If the player’s response is unrelated or makes no sense for the situation, label it '(out of context)' and do NOT generate any reply.

When to reply:
- If judged '(good)' or '(bad)', follow the label with a SHORT, natural NPC line in {LanguageCentral.Instance.GetFullLanguageName()} that fits the dementia character.
- If '(out of context)', do not add any response line.

Examples for reference:
- GOOD: '{input.expectedGoodMessage}'
- BAD: '{input.expectedBadMessage}'

Reminder:
- Always think about the meaning and tone, not exact words.
- Stay in character as a dementia NPC: suspicious, easily offended, or confused.
- If unsure, choose '(out of context)'.

<player>: {response}
"
    };

    Debug.Log("PROMPT:\n" + newMessage.Content);

    AppendMessage(newMessage);

    messages.Add(newMessage);

    thinkingIndicator?.SetActive(true);
    await Task.Yield();

    var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
    {
        Model = "gpt-4o",
        Messages = messages
    });

     thinkingIndicator?.SetActive(false);

    if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
    {
        var message = completionResponse.Choices[0].Message;
        message.Content = message.Content.Trim();
        messages.Add(message);
        AppendMessage(message);

        return message.Content;
    }
    else
    {
        Debug.LogWarning("No text was generated from this prompt.");
        return "ERROR";
    }
}


    public async Task<string> SendJudgementNoReply(Message input, string response){
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = $"<developer> : you will get the player response, if player response is in japanese understand it, the example good response from player is '{input.expectedGoodMessage}', and the bad example is '{input.expectedBadMessage}', please JUST judge the player response is inclined as '(good)' or '(bad)' according to example WITHOUT followed by response message, if you feel the player response is out of topic then just label it '(out of context)' without followed by response message; <player> : {response};"        
        };
        
        AppendMessage(newMessage);
        
        messages.Add(newMessage);
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o",
            Messages = messages
        });

        new Cognitive3D.CustomEvent("Send judgement with no reply prompt to OpenAI")
           .SetProperty("content", newMessage.Content)
            .Send();

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            messages.Add(message);
            AppendMessage(message);

            return message.Content;
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return "ERROR";
        }
    }

    public async Task<string> SendNoJudgementInput(Message input, string response){
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = $"<developer> : you will get the player response, if player response is in japanese understand it, please JUST reply with short response message; <player> : {response};"        
        };
        
        AppendMessage(newMessage);
        
        messages.Add(newMessage);
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o",
            Messages = messages
        });

        new Cognitive3D.CustomEvent("Send player input with no judgement prompt to OpenAI")
            .SetProperty("content", newMessage.Content)
            .Send();

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
            messages.Add(message);
            AppendMessage(message);

            return message.Content;
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return "ERROR";
        }
    }

}

[System.Serializable]
public class Message
{
    [SerializeField] public bool isContext;
    [SerializeField] public bool noReplyJudgement;
    [SerializeField] public bool noJudgementInput;
    [SerializeField] public bool judgementWithContext;
    [SerializeField] public string intendedSubtopic;
    [SerializeField] public string expectedGoodMessage;
    [SerializeField] public string expectedBadMessage;
    [SerializeField] public string outOfTopicHandling;

    [SerializeField] public bool isAssistance;
    [SerializeField] public AudioClip assistanceAudioId;
    [SerializeField] public AudioClip assistanceAudioJp;
    [SerializeField] public string assistanceLineId;
    [SerializeField] public string assistanceLineJp;
    [SerializeField] public float initialDelay;

    [SerializeField] public string promptId;
    [SerializeField] public string promptJp;

    [SerializeField] public bool externalTrigger;
}
