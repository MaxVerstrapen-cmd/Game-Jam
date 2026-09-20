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


//-----------------------------
    public GameObject player1, player2;

    public playerMovement p1;
    public playerMovement p2;

    private bool p1_2Triggered = false;
    private bool p1_1Triggered = false;
    private bool p1_0Triggered = false;

    private bool p2_2Triggered = false;
    private bool p2_1Triggered = false;
    private bool p2_0Triggered = false;
//--------------------------

    private void Awake()
    {
        mainCamera = Camera.main;

        headText.gameObject.SetActive(false);
        heelText.gameObject.SetActive(false);

        
    }

    private void Start()
    {
        player1 = GameObject.Find("player1");
        player2 = GameObject.Find("player2");

        p1 = player1.GetComponent<playerMovement>();
        p2 = player2.GetComponent<playerMovement>();
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
        int health1 = p1.getHealth();
        int health2 = p2.getHealth();

        // Player 1 health thresholds
        if (health1 <= 2 && !p1_2Triggered)
        {
            HeelSpeaks("Curse you, Head!");
            p1_2Triggered = true;
        }
        else if (health1 <= 1 && !p1_1Triggered)
        {
            HeelSpeaks("You think you've got me beat, Head? Think again!");
            p1_1Triggered = true;
        }
        else if (health1 <= 0 && !p1_0Triggered)
        {
            HeelSpeaks("Curses, Head! The princess is yours...");
            p1_0Triggered = true;
        }


        // Player 2 health thresholds
        if (health2 <= 2 && !p2_2Triggered)
        {
            HeadSpeaks("Curse you, Heel!");
            p2_2Triggered = true;
        }
        else if (health2 <= 1 && !p2_1Triggered)
        {
            HeadSpeaks("You'll have to hit harder than that, Heel!");
            p2_1Triggered = true;
        }
        else if (health2 <= 0 && !p2_0Triggered)
        {
            HeadSpeaks("No... I was supposed to get over you...");
            p2_0Triggered = true;
        }
    }
}
    // //OriginalHealth1 = p1.getHealth();
    // // Existing hit-reaction tests
    // if (p2.getHealth() != OriginalHealth2 && OriginalHealth2 == 3)
    //     OriginalHealth2 = p2.getHealth();
    //     HeadSpeaks("Curse you, Heel!");
        

    // if (p1.getHealth() != OriginalHealth1)
    //     HeelSpeaks("Curse you, Head!");
    //     OriginalHealth1 = p1.getHealth();

    // // New half-health tests
    // if (p2.getHealth() != OriginalHealth2 && OriginalHealth2 == 2)
    //     OriginalHealth2 = p2.getHealth();
    //     HeadSpeaks("You'll have to hit harder than that, Heel!");
        

    // if (p1.getHealth() != OriginalHealth1)
    //     HeelSpeaks("You think you've got me beat, Head? Think again!");
    //     OriginalHealth1 = p1.getHealth();

    // // New zero-health tests
    // if (p2.getHealth() != OriginalHealth2 && OriginalHealth2 == 1)
    //     OriginalHealth2 = p2.getHealth();
    //     HeadSpeaks("No... I was supposed to get over you...");
        

    // if (p1.getHealth() != OriginalHealth1)
    //     HeelSpeaks("Curses, Head! The princess is yours...");
    //     OriginalHealth1 = p1.getHealth();

