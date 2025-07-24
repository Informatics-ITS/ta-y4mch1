using UnityEngine;

public class TriggerAudioZone : MonoBehaviour
{
    public GameObject player;
    public AudioSource audioSource;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;
        if (hasPlayed) return;

        Debug.Log($"Player masuk ke {gameObject.name}, mainkan audio: {audioSource.clip.name}");

        // Stop semua audio lain
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (AudioSource a in allAudio)
        {
            if (a != audioSource && a.isPlaying)
                a.Stop();
        }

        // Mainkan audio sekali saja
        audioSource.Play();
        hasPlayed = true;
    }
}
