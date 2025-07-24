using UnityEngine;

public class DelayedAudioStart : MonoBehaviour
{
    public AudioSource audioSource; 

    void Start()
    {
        // Panggil fungsi PlayAudio setelah 15 detik
        Invoke("PlayAudio", 20f);
    }

    void PlayAudio()
    {
        audioSource.Play();
    }
}
