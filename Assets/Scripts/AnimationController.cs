using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance { get; private set; }

    private Animator animator;
    [SerializeField] private AudioSource audioSource;

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

        animator = GetComponent<Animator>();
    }

    public void OnPlayerResponse(string input)
    {
        StartCoroutine(PlayTTSAndAnimate(input.ToLower()));
    }

    private IEnumerator PlayTTSAndAnimate(string input)
    {
        // Tunggu TTS selesai generate audio
        Task<bool> ttsTask = TTSManager.Instance.SendReplyTTS(input);
        yield return new WaitUntil(() => ttsTask.IsCompleted);

        if (ttsTask.Result)
        {
            string path = Path.Combine(Application.persistentDataPath, "Resources/audioIn.wav");

            using (var www = new WWW("file://" + path))
            {
                yield return www;

                AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);

                if (clip != null)
                {
                    audioSource.clip = clip;

                    if (input == "good")
                        animator.SetTrigger("GoodTrigger");
                    else if (input == "bad")
                        animator.SetTrigger("BadTrigger");

                    audioSource.Play();
                }
                else
                {
                    Debug.LogError("AudioClip is null, failed to load from: " + path);
                }
            }
        }
        else
        {
            Debug.LogError("TTS failed to generate audio");
        }
    }
}