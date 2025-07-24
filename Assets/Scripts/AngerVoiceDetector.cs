using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AngerVoiceDetector : MonoBehaviour
{
    public bool IsAngry { get; private set; }

    private AudioClip micClip;
    private const int sampleRate = 44100;
    private float[] samples = new float[1024];
    private float lastPitch = 0f;
    private float smoothedPitch = 0f;
    private float displayedLevel = 0f;

    [SerializeField] private List<Image> pitchBars; // UI bar

    // === Input System Reference ===
    public InputActionReference aButtonAction; // Gantikan OVRInput.Button.One

    void Start()
    {
        micClip = Microphone.Start(null, true, 1, sampleRate);
    }

    void Update()
    {
        bool isAButtonHeld = aButtonAction != null && aButtonAction.action.ReadValue<float>() > 0.5f;

        if (!isAButtonHeld)
        {
            IsAngry = false;
            UpdatePitchBarUI(0f); // Reset bar saat tidak menekan tombol
            return;
        }

        if (!Microphone.IsRecording(null)) return;

        int micPos = Microphone.GetPosition(null) - samples.Length;
        if (micPos < 0) return;

        micClip.GetData(samples, micPos);
        float rms = GetRMS(samples);
        float rawPitch = DetectPitch(samples, sampleRate);

        // Haluskan pitch untuk UI
        smoothedPitch = Mathf.Lerp(smoothedPitch, rawPitch, 0.2f);

        Debug.Log("Pitch (Hz): " + rawPitch.ToString("F2"));

        // Deteksi pitch naik tajam pakai raw
        float pitchRise = rawPitch - lastPitch;
        lastPitch = rawPitch;

        // Deteksi marah
        IsAngry = (rms > 0.3f && rawPitch > 350f && pitchRise > 30f);

        // Update UI Bar pakai pitch yang dihaluskan
        UpdatePitchBarUI(smoothedPitch);
    }

    float GetRMS(float[] buffer)
    {
        float sum = 0f;
        foreach (float s in buffer) sum += s * s;
        return Mathf.Sqrt(sum / buffer.Length);
    }

    float DetectPitch(float[] buffer, int sampleRate)
    {
        int minLag = sampleRate / 500;
        int maxLag = sampleRate / 50;
        float maxCorr = 0f;
        int bestLag = 0;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float corr = 0f;
            for (int i = 0; i < buffer.Length - lag; i++)
                corr += buffer[i] * buffer[i + lag];

            if (corr > maxCorr)
            {
                maxCorr = corr;
                bestLag = lag;
            }
        }

        return bestLag == 0 ? 0f : sampleRate / (float)bestLag;
    }

    private void UpdatePitchBarUI(float pitch)
    {
        int targetLevel = 0;

        if (pitch > 100f) targetLevel = 1;
        if (pitch > 150f) targetLevel = 2;
        if (pitch > 200f) targetLevel = 3;
        if (pitch > 250f) targetLevel = 4;
        if (pitch > 350f) targetLevel = 5;

        // Lerp untuk haluskan level transisi bar
        displayedLevel = Mathf.Lerp(displayedLevel, targetLevel, 0.5f);

        for (int i = 0; i < pitchBars.Count; i++)
        {
            var img = pitchBars[i];
            Color color = img.color;

            float targetAlpha = i <= displayedLevel ? 1f : 0f;

            // Lerp alpha agar lebih smooth
            float newAlpha = Mathf.Lerp(color.a, targetAlpha, 0.3f);

            img.color = new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}
