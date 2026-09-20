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
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayers();
        roundOver = false;
    }

    void FindPlayers()
    {
        player1 = GameObject.Find("player1").GetComponent<playerMovement>();
        player2 = GameObject.Find("player2").GetComponent<playerMovement>();
    }

    void Update()
    {
       

        if ((player1 != null && player2 != null) & (player1.getHealth() <= 0 || player2.getHealth() <= 0))
        {
            if(!roundOver) //WHEN THE ROUND ENDS, SET A TIMER FOR THE ROUND TO RESET
            {
                roundEndTimer = Time.time + 3f;
                player1Score += player2.getHealth() <= 0 ? 1 : 0;
                player2Score += player1.getHealth() <= 0 ? 1 : 0;

                Debug.Log("Player score: " + player1Score + " " + player2Score);

                roundOver = true;
                Debug.Log("round is over? " + roundOver);
            }
        }


        if (roundOver && roundEndTimer <= Time.time)
        {
            Debug.Log("round ending imminently?");
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