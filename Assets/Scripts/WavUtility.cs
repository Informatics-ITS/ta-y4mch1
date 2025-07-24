using UnityEngine;
using System.Text;
using System.IO;
using System;

/// <summary>
/// WAV utility for recording and audio playback functions in Unity.
/// Version: 1.0 alpha 1
///
/// - Use "ToAudioClip" method for loading wav file / bytes.
/// Loads .wav (PCM uncompressed) files at 8,16,24 and 32 bits and converts data to Unity's AudioClip.
///
/// - Use "FromAudioClip" method for saving wav file / bytes.
/// Converts an AudioClip's float data into wav byte array at 16 bit.
/// </summary>
/// <remarks>
/// For documentation and usage examples: https://github.com/deadlyfingers/UnityWav
/// </remarks>

public class WavUtility
{
	// This method converts byte array of WAV file into AudioClip
    public static AudioClip ToAudioClip(byte[] wavFileBytes)
    {
        // Load WAV data into an AudioClip
        // Ensure the WAV file is in a format Unity supports (e.g., PCM)
        int channels = wavFileBytes[22]; // Get the number of channels
        int sampleRate = BitConverter.ToInt32(wavFileBytes, 24); // Get the sample rate
        int bitDepth = wavFileBytes[34]; // Get the bit depth
        int sampleCount = (wavFileBytes.Length - 44) / (bitDepth / 8); // Calculate the number of samples

        float[] audioData = new float[sampleCount];

        // Convert the byte array to floating point audio data
        int byteIndex = 44;
        for (int i = 0; i < audioData.Length; i++)
        {
            audioData[i] = BitConverter.ToInt16(wavFileBytes, byteIndex) / 32768f; // Normalize to -1.0f to 1.0f range
            byteIndex += bitDepth / 8;
        }

        AudioClip audioClip = AudioClip.Create("LoadedWavClip", sampleCount, channels, sampleRate, false);
        audioClip.SetData(audioData, 0);

        return audioClip;
    }

}