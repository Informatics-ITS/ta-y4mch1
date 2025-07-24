using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
public class MicInputVR : MonoBehaviour
{
    private AudioClip microphoneClip;
    private string micName;
    private int sampleWindow = 128;

    private float threshold;
    private bool hasShouted = false;
    private float cooldownTime = 2f;
    private float lastPenaltyTime = 0f;
    private bool micActive = false;

    // Hysteresis margin supaya gak toggle terus-menerus pas di sekitar threshold
    private float hysteresisMargin = 0.02f;

	public InputActionReference xButton;

	void Start()
    {
        threshold = PlayerPrefs.GetFloat("ShoutAmplitude", 0.1f);
        Debug.Log($"Threshold (Shout Amplitude) diambil di MicInputVR: {threshold:F4}");

        if (Microphone.devices.Length > 0)
        {
            micName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("Tidak ada mikrofon terdeteksi!");
        }
    }

    void Update()
    {
		bool isCurrentlyPressed = xButton.action.ReadValue<float>() > 0.5f;
		// Ketika tombol A ditekan (sekali)
		if (isCurrentlyPressed)
        {
            if (!micActive)
            {
                StartMicrophone();
                micActive = true;
                hasShouted = false; // Reset shout detection saat mulai
            }
        }

        // Ketika tombol A dilepas
        if (!isCurrentlyPressed)
        {
            if (micActive)
            {
                StopMicrophone();
                micActive = false;
                // Jangan reset hasShouted di sini supaya gak toggle selang-seling
            }
        }

        if (micActive && Microphone.IsRecording(micName))
        {
            float amplitude = GetLoudnessFromMic();
            Debug.Log($"Amplitudo: {amplitude:F4}, Threshold: {threshold:F4}");

            if (!hasShouted)
            {
                // Cek jika amplitude melewati threshold + margin (hysteresis naik)
                if (amplitude > threshold + hysteresisMargin && Time.time - lastPenaltyTime > cooldownTime)
                {
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.InputScoreHandler("bad");
                        Debug.Log("ScoreManager InputScoreHandler('bad') dipanggil karena shout terdeteksi");
                    }
                    else
                    {
                        Debug.LogWarning("ScoreManager.Instance belum terinisialisasi atau tidak ditemukan!");
                    }

                    hasShouted = true;
                    lastPenaltyTime = Time.time;
                }
            }
            else
            {
                // Reset hasShouted hanya kalau amplitude turun di bawah threshold - margin (hysteresis turun)
                if (amplitude < threshold - hysteresisMargin)
                {
                    hasShouted = false;
                }
            }
        }
    }

    void StartMicrophone()
    {
        if (microphoneClip == null || !Microphone.IsRecording(micName))
        {
            microphoneClip = Microphone.Start(micName, true, 10, 44100);
            Debug.Log("Microphone started...");
        }
        else
        {
            Debug.LogWarning("Microphone already recording.");
        }
    }

    void StopMicrophone()
    {
        if (Microphone.IsRecording(micName))
        {
            Microphone.End(micName);
            Debug.Log("Microphone stopped.");
        }
        microphoneClip = null;
    }

    float GetLoudnessFromMic()
    {
        if (microphoneClip == null) return 0f;

        int micPosition = Microphone.GetPosition(micName) - sampleWindow;
        if (micPosition < 0) return 0f;

        float[] samples = new float[sampleWindow];
        microphoneClip.GetData(samples, micPosition);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += samples[i] * samples[i];
        }
        return Mathf.Sqrt(sum / sampleWindow);
    }
}
