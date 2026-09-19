using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        // Fall back to the Button on this same object if nothing was dragged in.
        if (startButton == null)
        {
            startButton = GetComponent<Button>();
        }

        if (startButton == null)
        {
            Debug.LogError("StartGame: no Button assigned and none on " + name, this);
            return;
        }

        startButton.onClick.AddListener(StartTheGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTheGame()
    {

            SceneManager.LoadScene("mainScene");
    }
}
