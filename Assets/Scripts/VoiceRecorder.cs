using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Cognitive3D;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


public class VoiceRecorder : MonoBehaviour{
    public static VoiceRecorder Instance { get; private set; }
    private AudioClip recording;
    private AudioClip trimmedClip;
    private string microphone;
    private bool isRecording = false;
    private string directoryPath;
    public string audioInPath;
    private string audioOutPath;
    private int sampleRate = 44100;
    public bool isAllowedToRecord {get; set;} = false;
    [SerializeField] private AudioSource audioSource;
    private AudioClip playedClip;

    private bool isButtonOnePressed = false;
    private CancellationTokenSource cancellationTokenSource;

    private float recordTimer = 0f;
    [SerializeField] private GameObject recordUI;
    [SerializeField] private Image radialTimer;

    public InputActionReference aButton; 

    private float[] tempData = new float[128];
    private float currentAmplitude = 0f;
    public float CurrentAmplitude => currentAmplitude;


    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        }

        directoryPath = Path.Combine(Application.persistentDataPath, "Resources");
        audioInPath = Path.Combine(directoryPath, "audioIn.wav");
        audioOutPath = Path.Combine(directoryPath, "audioOut.wav");
    }

    void Start(){

        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }
    }


    async void Update(){

        bool isCurrentlyPressed = aButton.action.ReadValue<float>() > 0.5f;

        if ((isCurrentlyPressed || Input.GetKeyDown(KeyCode.LeftArrow)) && isAllowedToRecord && !isButtonOnePressed)
        {
            Debug.Log("Masuk rekam");
            recordUI.SetActive(true);

            StartRecording();
            isButtonOnePressed = true;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            StartCoroutine(StartTimerSpeakAsync(cancellationTokenSource.Token));
        }
        else if ((!isCurrentlyPressed || Input.GetKeyDown(KeyCode.RightArrow)) && isAllowedToRecord && isButtonOnePressed)
        {
            Debug.Log("Masuk lepas");
            await StopRecording();
            isButtonOnePressed = false;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }
    }

    private IEnumerator StartTimerSpeakAsync(CancellationToken token)
    {
        float elapsedTime = 0f;
        float duration = 20f;

        while (elapsedTime < duration)
        {
            if (token.IsCancellationRequested)
            {
                Debug.Log("Timer canceled.");
                yield break;
            }

            yield return null; // Wait for the next frame

            // Increment elapsed time by the actual time that has passed since the last frame
            elapsedTime += Time.deltaTime;

            // Calculate the progress (0 to 1) and update the radial timer fill
            recordTimer = elapsedTime / duration;
            radialTimer.fillAmount = recordTimer;
        }

        // Reset the timer once the countdown is complete
        recordTimer = 0f;
        radialTimer.fillAmount = recordTimer;

        StopRecording();
        isButtonOnePressed = false;
    }

    public void StartRecording()
    {
        if (isRecording || !isAllowedToRecord) return;

        new Cognitive3D.CustomEvent("Player start recording voice").Send();

        microphone = Microphone.devices[0];
        recording = Microphone.Start(microphone, false, 60, 44100);
        isRecording = true;
        Debug.Log("Recording started...");
        ConversationManager.Instance.assistantPrompt.SetActive(false);
    }

    public async Task StopRecording(){
        if (!isRecording) return;

        new Cognitive3D.CustomEvent("Player stop recording voice").Send();

        recordUI.SetActive(false);
        // Get the recording length
        int recordingLength = Microphone.GetPosition(microphone);
        Microphone.End(microphone);
        isRecording = false;
        Debug.Log("Recording stopped...");

        float[] samples = new float[recordingLength * recording.channels];
        recording.GetData(samples, 0);
        trimmedClip = AudioClip.Create(recording.name, recordingLength, recording.channels, sampleRate, false);
        trimmedClip.SetData(samples, 0);

        // return await SaveRecording(audioOutPath);
        await SaveRecording(audioOutPath);
    }
    public async void StopButton(){
        await StopRecording();
    }

    private async Task SaveRecording(string path){
        var samples = new float[trimmedClip.samples * trimmedClip.channels];
        trimmedClip.GetData(samples, 0);

        byte[] wavData = ConvertToWav(samples, trimmedClip.channels, trimmedClip.frequency);

        await WriteAllBytesAsync(path, wavData);

        string recvText = await STTManager.Instance.SendReplySTT(path);

        ////tambahan untuk hidtory
        if (VoiceHistoryManager.Instance != null && !string.IsNullOrEmpty(recvText))
    {
        VoiceHistoryManager.Instance.AddVoiceHistory(recvText);
    }

        new Cognitive3D.CustomEvent("Received text to speech from OpenAI")
           .SetProperty("content", recvText)
            .Send();

        if(ConversationManager.Instance.currentMessage.noReplyJudgement){
            bool success = await ConversationManager.Instance.ProceedJudgementNoReply(recvText);

            if (!success)
            {                
                await AssistantManager.Instance.PlayRetryPrompt();
                
                isAllowedToRecord = true;
                ConversationManager.Instance.assistantPrompt.SetActive(true);

                return;
            }

            isAllowedToRecord = false;

            if(!ConversationManager.Instance.CheckNextTriggerType())
                ConversationManager.Instance.NextMessage();

        }else if(ConversationManager.Instance.currentMessage.noJudgementInput){

            bool success = await ConversationManager.Instance.ProceedJudgementNoJudge(recvText);

            if (!success)
            {
                
                await AssistantManager.Instance.PlayRetryPrompt();

                isAllowedToRecord = true;
                ConversationManager.Instance.assistantPrompt.SetActive(true);
                return;
            }

            isAllowedToRecord = false;
            StartCoroutine(LoadAndPlayWav());

        }
        else if(ConversationManager.Instance.currentMessage.judgementWithContext){
            bool success = await ConversationManager.Instance.ProceedJudgementWithContext(recvText);

            if (!success)
            {
                await AssistantManager.Instance.PlayRetryPrompt();
                
                isAllowedToRecord = true;
                ConversationManager.Instance.assistantPrompt.SetActive(true);
                return;
            }

            isAllowedToRecord = false;
            StartCoroutine(LoadAndPlayWav());
        }
        else{
            bool success = await ConversationManager.Instance.ProceedJudgement(recvText);

            if (!success)
            {
                await AssistantManager.Instance.PlayRetryPrompt();
                
                isAllowedToRecord = true;
                ConversationManager.Instance.assistantPrompt.SetActive(true);
                return;
            }

            isAllowedToRecord = false;
            StartCoroutine(LoadAndPlayWav());
        }
    }

    private async Task WriteAllBytesAsync(string path, byte[] data){
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
        {
            await fs.WriteAsync(data, 0, data.Length);
        }
    }

    private async Task<byte[]> ReadFileAsync(string path)
    {
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            byte[] fileData = new byte[fs.Length];
            await fs.ReadAsync(fileData, 0, fileData.Length);
            return fileData;
        }
    }

    private byte[] ConvertToWav(float[] samples, int channels, int sampleRate){
        int sampleCount = samples.Length;
        int byteCount = sampleCount * sizeof(short);
        int headerSize = 44;
        int fileSize = headerSize + byteCount;

        byte[] wav = new byte[fileSize];

        Buffer.BlockCopy(new byte[] { 0x52, 0x49, 0x46, 0x46 }, 0, wav, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(fileSize - 8), 0, wav, 4, 4);
        Buffer.BlockCopy(new byte[] { 0x57, 0x41, 0x56, 0x45 }, 0, wav, 8, 4);

        Buffer.BlockCopy(new byte[] { 0x66, 0x6d, 0x74, 0x20 }, 0, wav, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(16), 0, wav, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, wav, 20, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)channels), 0, wav, 22, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(sampleRate), 0, wav, 24, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(sampleRate * channels * sizeof(short)), 0, wav, 28, 4);
        Buffer.BlockCopy(BitConverter.GetBytes((short)(channels * sizeof(short))), 0, wav, 32, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)16), 0, wav, 34, 2);

        Buffer.BlockCopy(new byte[] { 0x64, 0x61, 0x74, 0x61 }, 0, wav, 36, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(byteCount), 0, wav, 40, 4);

        int offset = 44;
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(samples[i] * short.MaxValue);
            Buffer.BlockCopy(BitConverter.GetBytes(sample), 0, wav, offset, sizeof(short));
            offset += sizeof(short);
        }

        return wav;
    }

    public void PlayRemote(){
        StartCoroutine(LoadAndPlayWav());
    }

    private IEnumerator LoadAndPlayWav()
    {
        // Load the WAV file into a byte array
        byte[] wavFileBytes = File.ReadAllBytes(audioInPath);

        // Create an AudioClip from the WAV file bytes
        AudioClip audioClip = WavUtility.ToAudioClip(wavFileBytes);

        // Assign the AudioClip to the AudioSource and play it
        audioSource.clip = audioClip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        if(!ConversationManager.Instance.CheckNextTriggerType())
            ConversationManager.Instance.NextMessage();
    }
        void LateUpdate()
    {
        if (isRecording && recording != null && Microphone.IsRecording(microphone))
        {
            int micPos = Microphone.GetPosition(microphone) - 128;
            if (micPos < 0 || micPos + 128 > recording.samples) return;

            recording.GetData(tempData, micPos);

            float sum = 0f;
            for (int i = 0; i < tempData.Length; i++)
            {
                sum += Mathf.Abs(tempData[i]);
            }

            currentAmplitude = sum / tempData.Length;
        }
        else
        {
            currentAmplitude = 0f;
        }
    }

}