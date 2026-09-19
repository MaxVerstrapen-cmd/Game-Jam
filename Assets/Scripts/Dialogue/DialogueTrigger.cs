using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogue;
    public DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager.StartDialogue(dialogue);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueManager.NextLine();
        }
    }
}
