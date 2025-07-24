using System.Collections.Generic;
using UnityEngine;

public class TriggerAudioManager : MonoBehaviour
{
    [Header("Assign Player GameObject")]
    public GameObject player;

    [System.Serializable]
    public class TriggerAudioPair
    {
        public Collider triggerBox;
        public AudioSource audioSource;
    }

    [Header("Assign Trigger Boxes and AudioSources")]
    public List<TriggerAudioPair> triggerAudioPairs = new List<TriggerAudioPair>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;

        for (int i = 0; i < triggerAudioPairs.Count; i++)
        {
            TriggerAudioPair pair = triggerAudioPairs[i];

            if (pair.triggerBox.bounds.Contains(player.transform.position))
            {
                PlayAudioFromIndex(i);
                break;
            }
        }
    }

    private void PlayAudioFromIndex(int index)
    {
        for (int i = 0; i < triggerAudioPairs.Count; i++)
        {
            if (i == index)
            {
                if (!triggerAudioPairs[i].audioSource.isPlaying)
                    triggerAudioPairs[i].audioSource.Play();
            }
            else
            {
                if (triggerAudioPairs[i].audioSource.isPlaying)
                    triggerAudioPairs[i].audioSource.Stop();
            }
        }
    }
}
