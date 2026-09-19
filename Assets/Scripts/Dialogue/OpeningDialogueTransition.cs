using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningDialogueTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "mainScene";

    private bool isTransitioning;

    private void OnEnable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueFinished += LoadGameplay;
        }
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueFinished -= LoadGameplay;
        }
    }

    private void LoadGameplay()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        SceneManager.LoadScene(gameplaySceneName);
    }
}
