using UnityEngine;
using UnityEngine.SceneManagement;

public class roundManager : MonoBehaviour
{
    public int player1Score;
    public int player2Score;

    public playerMovement player1;
    public playerMovement player2;

    public static roundManager Instance;

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
        FindPlayers();
    }

    void FindPlayers()
    {
        player1 = GameObject.Find("player1").GetComponent<playerMovement>();
        player2 = GameObject.Find("player2").GetComponent<playerMovement>();
    }

    void Update()
    {
        if (player1 == null || player2 == null)
        {
            return;
        }

        if (player1.getHealth() <= 0 || player2.getHealth() <= 0)
        {
            player1Score += player2.getHealth() <= 0 ? 1 : 0;
            player2Score += player1.getHealth() <= 0 ? 1 : 0;

            Debug.Log("Player score: " + player1Score + " " + player2Score);

            if (player1Score == 5)
            {
                Debug.Log("player1 wins");
            }
            else if (player2Score == 5)
            {
                Debug.Log("player2 wins");
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}