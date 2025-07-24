using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class AudioTrigger : MonoBehaviour
{
	public GameObject objectA;
	public GameObject objectB;

	private AudioSource audioA;
	private AudioSource audioB;

	private bool isProcessing = false;
	private bool hasPlayed = false;

	public InputActionReference trigButton;
	public InputActionReference yButton;

	private bool trigPressed = false;
	private bool yPressed = false;


	void Start()
	{
		if (objectA != null)
			audioA = objectA.GetComponent<AudioSource>();

		if (objectB != null)
			audioB = objectB.GetComponent<AudioSource>();
	}

	void Update()
	{
		if (hasPlayed) return;

		bool yStat = yButton.action.ReadValue<float>() > 0.5f;
		bool trigStat = trigButton.action.ReadValue<float>() > 0.5f;

		// Trigger kiri ditekan
		if (trigStat && !trigPressed)
		{
			trigPressed = true;
			if (audioA != null && !audioA.isPlaying)
				audioA.Play();
		}

		if (!trigStat && trigPressed) {
			trigPressed = false;
		}

		// Tombol Y ditekan
		if (yStat && !yPressed && !isProcessing)
		{
			yPressed = true;
			isProcessing = true;
			StartCoroutine(PlayBAfterDelay(5f));
		}

		if (!yStat && yPressed) {
			yPressed= false;
		}
	}

	IEnumerator PlayBAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		if (audioB != null)
			audioB.Play();

		hasPlayed = true;
	}
}
