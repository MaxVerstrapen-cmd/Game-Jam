using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenHowToPlay : MonoBehaviour
{
    public void OpenScene()
    {
        SceneManager.LoadScene("HowToPlayScene");
    }
}
