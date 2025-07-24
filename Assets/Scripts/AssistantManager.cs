using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using Cognitive3D;

public class AssistantManager : MonoBehaviour
{
    
    public static AssistantManager Instance { get; private set; }
    [SerializeField] private AudioSource audioSource;
    [SerializeField] public TextMeshProUGUI dialogue;

    private string currenLine;
    private Coroutine playCoroutine;
    private Coroutine typeCoroutine;

    [SerializeField] private AudioClip retryClipId;
    [SerializeField] private AudioClip retryClipJp;
    [SerializeField] private string retryLineId;
    [SerializeField] private string retryLineJp;

    private float typingSpeed = 0.01f;

    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        }
    }

    public void PlayAssistant(AudioClip audioClip, string line, float initialDelay){
        playCoroutine = StartCoroutine(LoadAndPlay(audioClip, line, initialDelay));
    }
    
    private IEnumerator LoadAndPlay(AudioClip audioClip, string line, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
        currenLine = line;

        audioSource.clip = audioClip;
        typeCoroutine = StartCoroutine(TypeText(line));
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        if(!ConversationManager.Instance.CheckNextTriggerType())
            ConversationManager.Instance.NextMessage();
    }

    public async Task PlayRetryPrompt(){
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        string previousText = dialogue.text;

        audioSource.clip = (LanguageCentral.Instance.language == "ja") ? retryClipJp : retryClipId;
        typeCoroutine = StartCoroutine(TypeText((LanguageCentral.Instance.language == "ja") ? retryLineJp : retryLineId));
        audioSource.Play();

        while (audioSource.isPlaying)
        {
            await Task.Delay(50);
        }

        await Task.Delay(200);

        dialogue.text = previousText;

        return;
    }

    private IEnumerator PlayAwaiter(){
        yield return new WaitWhile(() => audioSource.isPlaying);
    }

    private IEnumerator TypeText(string input)
    {
        new Cognitive3D.CustomEvent("Assistant talking")
            .SetProperty("content", input)
            .Send();

        dialogue.text = "";
        foreach (char character in input)
        {
            dialogue.text += character; // Append the next character
            yield return new WaitForSeconds(typingSpeed); // Wait for the next character
        }
    }
}
