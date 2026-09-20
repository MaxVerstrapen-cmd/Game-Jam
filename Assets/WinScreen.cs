using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinScreen : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void Show(string winnerName)
    {
        winnerText.text = winnerName + " Wins!";
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        roundManager.Instance.ResetScores();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        roundManager.Instance.ResetScores();
        SceneManager.LoadScene(mainMenuScene);
    }
}
