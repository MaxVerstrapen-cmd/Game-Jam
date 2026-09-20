using UnityEngine;
using UnityEngine.SceneManagement;

public class roundManager : MonoBehaviour
{
    public int player1Score;
    public int player2Score;

    public playerMovement player1;
    public playerMovement player2;

    public static roundManager Instance;

    public float roundEndTimer;

    public bool roundOver;

    //guards the roundOver branch below so it fires exactly once per round -
    //without it, once roundEndTimer elapses this keeps re-running every
    //frame (ShowWinScreen/LoadScene both get called repeatedly) since
    //nothing else stops roundOver from staying true
    private bool roundResolved;

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
        roundOver = false;
        roundResolved = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayers();
        roundOver = false;
        roundResolved = false;
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
        if (player1 == null || player2 == null)
        {
            return;
        }

        if (!roundOver && (player1.getHealth() <= 0 || player2.getHealth() <= 0))
        {
            //WHEN THE ROUND ENDS, SET A TIMER FOR THE ROUND TO RESET - gives
            //the finishing hit's Attack/Hurt animations (up to 0.5833333s,
            //see AttackMax.anim/AttackMax2.anim) time to finish before the
            //scene reloads or the win screen appears
            roundEndTimer = Time.time + 3f;
            player1Score += player2.getHealth() <= 0 ? 1 : 0;
            player2Score += player1.getHealth() <= 0 ? 1 : 0;

            Debug.Log("Player score: " + player1Score + " " + player2Score);

            roundOver = true;
        }

        if (roundOver && !roundResolved && roundEndTimer <= Time.time)
        {
            roundResolved = true;

            if (player1Score == 5)
            {
                ShowWinScreen("Player 1");
            }
            else if (player2Score == 5)
            {
                ShowWinScreen("Player 2");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
