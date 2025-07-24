using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cognitive3D;
using System.Collections.Generic;

public class AmplitudeChecker : MonoBehaviour
{
    public InputActionReference xButtonLeft;     // Tombol X (kontroler kiri) untuk kalibrasi
    public InputActionReference bButtonRight;    // Tombol B (kontroler kanan) untuk simpan threshold
    public InputActionReference aButtonRight;    // Tombol A (kontroler kanan) untuk deteksi marah

    public Image[] volumeBars;                   // Gambar bar volume
    public float micSampleRate = 44100;

    private AudioClip calibrationClip;
    private string calibrationMicDevice;
    private bool isCalibrating = false;
    private float calibrationTimer = 0f;
    private float amplitudeSum = 0f;
    private int sampleCount = 0;

    private bool hasTriggeredThisPress = false;

    private Queue<float> amplitudeWindow = new Queue<float>();
    [SerializeField] private int windowSize = 20; // Bisa diatur di Inspector
    [SerializeField] private float spikeFactor = 1.2f; // Kenaikan mendadak dianggap spike
    private float prevAvgAmp = 0f; // Untuk mendeteksi lonjakan mendadak

    void Update()
    {
        HandleCalibration();
        HandleSaveThreshold();
        HandleVolumeBar();
        HandleAngryDetection();
    }

    void HandleCalibration()
    {
        if (ThresholdManager.Instance.IsThresholdSaved) return;

        bool xPressed = xButtonLeft.action.IsPressed() || Input.GetKey(KeyCode.X);

        if (xPressed)
        {
            if (!isCalibrating)
            {
                if (Microphone.devices.Length == 0) return;

                calibrationMicDevice = Microphone.devices[0];
                calibrationClip = Microphone.Start(calibrationMicDevice, true, 5, (int)micSampleRate);

                isCalibrating = true;
                calibrationTimer = 0f;
                amplitudeSum = 0f;
                sampleCount = 0;

                Debug.Log("Kalibrasi dimulai...");
            }

            calibrationTimer += Time.deltaTime;

            int micPos = Microphone.GetPosition(calibrationMicDevice) - 128;
            if (micPos < 0 || micPos + 128 > calibrationClip.samples) return;

            float[] data = new float[128];
            calibrationClip.GetData(data, micPos);

            float sum = 0f;
            for (int i = 0; i < data.Length; i++)
                sum += Mathf.Abs(data[i]);

            float currentAmp = sum / data.Length;
            amplitudeSum += currentAmp;
            sampleCount++;

            Debug.Log($" Sampel ke-{sampleCount}, Amplitudo: {currentAmp:F4}");
        }
        else if (isCalibrating)
        {
            Microphone.End(calibrationMicDevice);
            isCalibrating = false;
            Debug.Log(" Kalibrasi dihentikan (tombol dilepas).");
        }
    }

    void HandleSaveThreshold()
    {
        if (ThresholdManager.Instance.IsThresholdSaved) return;

        bool bPressed = bButtonRight.action.WasPressedThisFrame() || Input.GetKeyDown(KeyCode.B);

        if (bPressed && sampleCount > 0)
        {
            float avg = amplitudeSum / sampleCount;
            float shoutThreshold = avg * 1.4f; // Safety factor agar bicara normal yang keras tetap aman
            ThresholdManager.Instance.SetThreshold(shoutThreshold);
            Debug.Log("Threshold disimpan: " + shoutThreshold);

            new CustomEvent("AmplitudeCalibration")
                .SetProperty("threshold", shoutThreshold)
                .Send();
        }
    }

    void HandleAngryDetection()
    {
        if (isCalibrating) return;

        bool aPressed = aButtonRight.action.IsPressed() || Input.GetKey(KeyCode.LeftArrow);

        if (!aPressed)
        {
            hasTriggeredThisPress = false;
            amplitudeWindow.Clear(); // Reset queue kalau tombol dilepas
            prevAvgAmp = 0f;
            return;
        }

        float currentAmp = VoiceRecorder.Instance.CurrentAmplitude;

        amplitudeWindow.Enqueue(currentAmp);
        if (amplitudeWindow.Count > windowSize)
            amplitudeWindow.Dequeue();

        float avgAmp = 0f;
        foreach (float amp in amplitudeWindow)
            avgAmp += amp;
        avgAmp /= amplitudeWindow.Count;

        float threshold = ThresholdManager.Instance.AmplitudeThreshold;

        float spike = (prevAvgAmp > 0f) ? avgAmp / prevAvgAmp : 1f;

        Debug.Log($" AvgAmplitude: {avgAmp:F4} | Threshold: {threshold:F4} | Spike: {spike:F2}");

        if (avgAmp > threshold && spike >= spikeFactor && !hasTriggeredThisPress)
        {
            Debug.Log("Terdeteksi marah! Skor akan dikurangi.");
            ScoreManager.Instance.InputScoreHandler("bad");
            hasTriggeredThisPress = true;

            new CustomEvent("AngryVoiceDetected")
                .SetProperty("avg_amplitude", avgAmp)
                .Send();
        }
        else if (avgAmp <= threshold)
        {
            Debug.Log(" Volume normal. Tidak marah.");
        }

        prevAvgAmp = avgAmp;
    }

    void HandleVolumeBar()
    {
        float amp = VoiceRecorder.Instance.CurrentAmplitude;
        float threshold = ThresholdManager.Instance.AmplitudeThreshold;

        float t = Mathf.Clamp01(threshold > 0f ? amp / threshold : 0f);
        int level = Mathf.Clamp(Mathf.FloorToInt(t * volumeBars.Length), 0, volumeBars.Length - 1);

        for (int i = 0; i < volumeBars.Length; i++)
        {
            volumeBars[i].enabled = i <= level;
            // Kalau mau bikin bar warna merah/hijau juga bisa di sini
        }
    }
}
