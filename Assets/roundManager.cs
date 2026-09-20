using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class roundManager : MonoBehaviour
{
    public int player1Score;
    public int player2Score;

    public playerMovement player1;
    public playerMovement player2;

    public static roundManager Instance;

    //true from the moment a round's outcome is decided until the next round
    //actually starts (scene reload, or Play Again after a match win) - without
    //this, Update() below keeps re-detecting the same health<=0 state every
    //frame (Time.timeScale = 0 during the win screen doesn't stop Update())
    //and re-scores/re-shows the win screen indefinitely.
    private bool roundEnded;

    //the finishing hit's Attack/Hurt clips are still playing when health
    //hits 0 - Attack is the longer of the two at 0.5833333s (see
    //AttackMax.anim/AttackMax2.anim), so wait that long before reloading
    //the scene or showing the win screen, or it cuts the animation off
    private const float roundEndDelay = 0.6f;

    public void ShowWinScreen(string winnerName)
    {
        WinScreen screen = FindObjectOfType<WinScreen>();
        if (screen != null) screen.Show(winnerName);
    }

    public void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
    }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            player1Score = 0;
            player2Score = 0;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        FindPlayers();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        roundEnded = false;
        FindPlayers();
    }

    void FindPlayers()
    {
        //no-op outside mainScene (e.g. after Exit loads MainMenu) - those
        //scenes have no player1/player2 objects to find
        GameObject p1Object = GameObject.Find("player1");
        GameObject p2Object = GameObject.Find("player2");

        player1 = p1Object != null ? p1Object.GetComponent<playerMovement>() : null;
        player2 = p2Object != null ? p2Object.GetComponent<playerMovement>() : null;
    }

    void Update()
    {
        if (player1 == null || player2 == null || roundEnded)
        {
            return;
        }

        if (player1.getHealth() <= 0 || player2.getHealth() <= 0)
        {
            roundEnded = true;

            player1Score += player2.getHealth() <= 0 ? 1 : 0;
            player2Score += player1.getHealth() <= 0 ? 1 : 0;

            Debug.Log("Player score: " + player1Score + " " + player2Score);

            StartCoroutine(EndRoundAfterDelay());
        }
    }

    private IEnumerator EndRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundEndDelay);

        if (player1Score == 3)
        {
            ShowWinScreen("Player 1");
        }
        else if (player2Score == 3)
        {
            ShowWinScreen("Player 2");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}