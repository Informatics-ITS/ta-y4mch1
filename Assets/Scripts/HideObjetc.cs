using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HideObject : MonoBehaviour
{
    [System.Serializable]
    public class HideTarget
    {
        public GameObject target;
        public bool isAnimated;
        public bool useTriggerInsteadOfTime;
        public GameObject triggerObject;

        public bool isCalibrationStep;
        public bool isSaveThresholdStep;
        public bool isSpawn;

        public bool isMove;
        public bool isTurn;

        public GameObject nextObject;

        public bool activateConversationAfterHidden;
        public GameObject assistantObject;
        public GameObject npcWithConversation;
    }

    [System.Serializable]
    public class HideSequence
    {
        public List<HideTarget> targets = new List<HideTarget>();
    }

    [Header("Input (assign dari InputActionReference)")]
    public InputActionReference rightTriggerButton;
    public InputActionReference leftXButton;
    public InputActionReference rightBButton;

    public InputActionReference moveButton;
    public InputActionReference turnButton;

    [Header("Sequence Steps")]
    public List<HideSequence> sequences = new List<HideSequence>();

    [Header("Audio (opsional)")]
    public AudioSource audioSource;
    [Tooltip("Isi sesuai urutan sequence. Boleh kosong/null jika tidak perlu.")]
    public List<AudioClip> sequenceAudios;

    [Header("Animation Settings")]
    public float animationDuration = 2f;
    public float fallSpeed = 2f;

    private int currentStep = 0;
    private bool isRunning = false;

    private List<bool> hasPlayedAudio;

    private Coroutine moveCoroutine;
    private Coroutine turnCoroutine;

    private void Start()
    {
        hasPlayedAudio = new List<bool>();
        for (int i = 0; i < sequences.Count; i++)
            hasPlayedAudio.Add(false);
    }

    private void OnEnable()
    {
        if (rightTriggerButton != null)
        {
            rightTriggerButton.action.Enable();
            rightTriggerButton.action.performed += OnTriggerPressed;
        }

        if (leftXButton != null)
        {
            leftXButton.action.Enable();
            leftXButton.action.performed += OnXPressed;
        }

        if (rightBButton != null)
        {
            rightBButton.action.Enable();
            rightBButton.action.performed += OnBPressed;
        }

        if (moveButton != null)
        {
            moveButton.action.Enable();
            moveButton.action.performed += OnMovePressed;
        }

        if (turnButton != null)
        {
            turnButton.action.Enable();
            turnButton.action.performed += OnTurnPressed;
        }
    }

    private void OnDisable()
    {
        if (rightTriggerButton != null)
            rightTriggerButton.action.performed -= OnTriggerPressed;

        if (leftXButton != null)
            leftXButton.action.performed -= OnXPressed;

        if (rightBButton != null)
            rightBButton.action.performed -= OnBPressed;

        if (moveButton != null)
            moveButton.action.performed -= OnMovePressed;

        if (turnButton != null)
            turnButton.action.performed -= OnTurnPressed;
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame)
                TryRunCurrentSequence();

            if (Keyboard.current.xKey.wasPressedThisFrame)
                RunCalibrationSteps();

            if (Keyboard.current.bKey.wasPressedThisFrame)
                RunSaveThresholdSteps();

            if (Keyboard.current.wKey.wasPressedThisFrame)
                RunMoveSteps();

            if (Keyboard.current.sKey.wasPressedThisFrame)
                RunTurnSteps();
        }
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        TryRunCurrentSequence();
    }

    private void OnXPressed(InputAction.CallbackContext context)
    {
        RunCalibrationSteps();
    }

    private void OnBPressed(InputAction.CallbackContext context)
    {
        RunSaveThresholdSteps();
    }

    private void OnMovePressed(InputAction.CallbackContext context)
    {
        if (moveCoroutine == null)
            moveCoroutine = StartCoroutine(HandleHoldMove());
    }

    private void OnTurnPressed(InputAction.CallbackContext context)
    {
        if (turnCoroutine == null)
            turnCoroutine = StartCoroutine(HandleHoldTurn());
    }

    private void TryRunCurrentSequence()
    {
        if (!isRunning && currentStep < sequences.Count)
        {
            StartCoroutine(RunSequence(sequences[currentStep]));

            if (audioSource != null && !audioSource.isPlaying && !hasPlayedAudio[currentStep])
            {
                if (sequenceAudios != null && currentStep < sequenceAudios.Count && sequenceAudios[currentStep] != null)
                {
                    audioSource.clip = sequenceAudios[currentStep];
                    audioSource.Play();
                }

                hasPlayedAudio[currentStep] = true;
            }

            currentStep++;
        }
    }

    private void RunCalibrationSteps()
    {
        foreach (var sequence in sequences)
        {
            foreach (var item in sequence.targets)
            {
                if (item.isCalibrationStep && item.target != null && item.target.activeSelf)
                    StartCoroutine(WaitAndSwap(item.target, item.nextObject, 5f));
            }
        }
    }

    private void RunSaveThresholdSteps()
    {
        foreach (var sequence in sequences)
        {
            foreach (var item in sequence.targets)
            {
                if (item.isSaveThresholdStep && item.target != null && item.target.activeSelf)
                {
                    Debug.Log($"[SaveThreshold] Hiding {item.target.name} → Showing {item.nextObject?.name}");
                    item.target.SetActive(false);
                    if (item.nextObject != null)
                        item.nextObject.SetActive(true);

                    if (item.activateConversationAfterHidden)
                        ActivateAfterHidden(item);
                }
            }
        }
    }

    private void RunMoveSteps()
    {
        foreach (var sequence in sequences)
        {
            foreach (var item in sequence.targets)
            {
                if (item.isMove && item.target != null && item.target.activeSelf)
                {
                    Debug.Log($"[Move] Hiding {item.target.name} → Showing {item.nextObject?.name}");
                    item.target.SetActive(false);
                    if (item.nextObject != null)
                        item.nextObject.SetActive(true);

                    if (item.activateConversationAfterHidden)
                        ActivateAfterHidden(item);
                }
            }
        }
    }

    private void RunTurnSteps()
    {
        foreach (var sequence in sequences)
        {
            foreach (var item in sequence.targets)
            {
                if (item.isTurn && item.target != null && item.target.activeSelf)
                {
                    Debug.Log($"[Turn] Hiding {item.target.name} → Showing {item.nextObject?.name}");
                    item.target.SetActive(false);
                    if (item.nextObject != null)
                        item.nextObject.SetActive(true);

                    if (item.activateConversationAfterHidden)
                        ActivateAfterHidden(item);
                }
            }
        }
    }

    private IEnumerator HandleHoldMove()
    {
        float holdDuration = 0f;
        while (moveButton.action.IsPressed())
        {
            holdDuration += Time.deltaTime;
            if (holdDuration >= 3f)
            {
                RunMoveSteps();
                break;
            }
            yield return null;
        }
        moveCoroutine = null;
    }

    private IEnumerator HandleHoldTurn()
    {
        float holdDuration = 0f;
        while (turnButton.action.IsPressed())
        {
            holdDuration += Time.deltaTime;
            if (holdDuration >= 3f)
            {
                RunTurnSteps();
                break;
            }
            yield return null;
        }
        turnCoroutine = null;
    }

    private IEnumerator RunSequence(HideSequence sequence)
    {
        isRunning = true;

        foreach (var item in sequence.targets)
        {
            if (item.target == null) continue;

            if (!item.isAnimated)
            {
                if (item.nextObject != null)
                    item.nextObject.SetActive(true);

                item.target.SetActive(false);

                if (item.activateConversationAfterHidden)
                    ActivateAfterHidden(item);
            }
        }

        foreach (var item in sequence.targets)
        {
            if (item.target == null || !item.isAnimated) continue;

            if (item.useTriggerInsteadOfTime && item.triggerObject != null)
            {
                yield return StartCoroutine(AnimateUntilTouch(item));
            }
            else
            {
                yield return StartCoroutine(AnimateAndHideAfterTime(item));
            }

            if (item.nextObject != null)
                item.nextObject.SetActive(true);

            if (item.activateConversationAfterHidden)
                ActivateAfterHidden(item);
        }

        isRunning = false;
    }

    private IEnumerator AnimateAndHideAfterTime(HideTarget item)
    {
        GameObject obj = item.target;
        Vector3 startPos = obj.transform.position;
        Vector3 endPos = startPos - new Vector3(0, fallSpeed * animationDuration, 0);
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            obj.transform.position = Vector3.MoveTowards(
                obj.transform.position,
                endPos,
                fallSpeed * Time.deltaTime
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        obj.SetActive(false);
    }

    private IEnumerator AnimateUntilTouch(HideTarget item)
    {
        GameObject obj = item.target;
        GameObject trigger = item.triggerObject;
        Collider triggerCol = trigger.GetComponent<Collider>();
        if (triggerCol == null)
        {
            Debug.LogWarning("Trigger object must have a Collider!");
            yield break;
        }

        float safeOffset = 0.05f;

        while (true)
        {
            obj.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            Bounds objBounds = obj.GetComponent<Collider>()?.bounds ?? new Bounds(obj.transform.position, Vector3.one);
            Bounds targetBounds = triggerCol.bounds;

            if (objBounds.min.y <= targetBounds.max.y + safeOffset)
                break;

            yield return null;
        }

        obj.SetActive(false);
    }

    private IEnumerator WaitAndSwap(GameObject current, GameObject next, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (current != null) current.SetActive(false);
        if (next != null) next.SetActive(true);
    }

    private void ActivateAfterHidden(HideTarget item)
    {
        if (item.assistantObject != null)
            item.assistantObject.SetActive(true);

        if (item.npcWithConversation != null)
        {
            var convo = item.npcWithConversation.GetComponent<ConversationManager>();
            if (convo != null)
                convo.enabled = true;
        }
    }
}
