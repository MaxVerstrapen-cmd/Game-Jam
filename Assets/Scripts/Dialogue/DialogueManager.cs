using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public Image portraitImage;

    public event Action OnDialogueFinished;

    private DialogueData currentDialogue;
    private int currentLineIndex;

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentLineIndex = 0;

        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (currentDialogue == null)
            return;

        currentLineIndex++;

        if (currentLineIndex < currentDialogue.lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = currentDialogue.lines[currentLineIndex];

        speakerNameText.text = string.IsNullOrWhiteSpace(line.speakerName)
                ? currentDialogue.speakerName
                : line.speakerName;
        dialogueText.text = line.text;

        if (line.portrait != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (currentDialogue.speakerPortrait != null)
        {
            portraitImage.sprite = currentDialogue.speakerPortrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        currentDialogue = null;
        currentLineIndex = 0;

        OnDialogueFinished?.Invoke();
    }
}