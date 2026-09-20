using System.Collections;
using TMPro;
using UnityEngine;

public class CombatDialogue : MonoBehaviour
{
    [Header("Fighters")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform heel;

    [Header("Dialogue Text")]
    [SerializeField] private TextMeshProUGUI headText;
    [SerializeField] private TextMeshProUGUI heelText;

    [Header("Display Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float displayDuration = 1.5f;

    private Camera mainCamera;
    private Coroutine headRoutine;
    private Coroutine heelRoutine;

    private void Awake()
    {
        mainCamera = Camera.main;

        headText.gameObject.SetActive(false);
        heelText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        PositionText(head, headText);
        PositionText(heel, heelText);
    }

    private void PositionText(Transform fighter, TextMeshProUGUI dialogueText)
    {
        if (fighter == null || dialogueText == null || mainCamera == null)
            return;

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(fighter.position + worldOffset);

        dialogueText.rectTransform.position = screenPosition;
    }

    public void HeadSpeaks(string line)
    {
        if (headRoutine != null)
            StopCoroutine(headRoutine);

        headRoutine = StartCoroutine(ShowLine(headText, line));
    }

    public void HeelSpeaks(string line)
    {
        if (heelRoutine != null)
            StopCoroutine(heelRoutine);

        heelRoutine = StartCoroutine(ShowLine(heelText, line));
    }

    private IEnumerator ShowLine(TextMeshProUGUI dialogueText, string line)
    {
        dialogueText.text = line;
        dialogueText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        dialogueText.gameObject.SetActive(false);
    }

    private void Update()
{
    // Temporary testing controls — remove these once combat hits trigger dialogue.
    if (Input.GetKeyDown(KeyCode.Alpha1))
    {
        HeadSpeaks("Curse you, Heel!");
    }

    if (Input.GetKeyDown(KeyCode.Alpha2))
    {
        HeelSpeaks("Curse you, Head!");
    }
}
}
