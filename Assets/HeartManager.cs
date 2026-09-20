using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : MonoBehaviour
{

    public roundManager rm;

    private int n = 0;

    // Start is called before the first frame update
    void Start()
    {
        rm = GameObject.Find("gameManager").GetComponent<roundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (n == 0)
        {
            n++;

            int HR_P1 = rm.player2Score;
            int HR_P2 = rm.player1Score;

            Debug.Log("HM Player1 Score " + HR_P2);
            Debug.Log("HM Player2 Score " + HR_P1);


            switch(HR_P1)
            {
                case 1:
                    Destroy(GameObject.Find("heart4"));
                    break;

                case 2:
                    Destroy(GameObject.Find("heart4"));
                    Destroy(GameObject.Find("heart3"));
                    break;

                case 3:
                    Destroy(GameObject.Find("heart4"));
                    Destroy(GameObject.Find("heart3"));
                    Destroy(GameObject.Find("heart2"));
                    break;

                case 4:
                    Destroy(GameObject.Find("heart4"));
                    Destroy(GameObject.Find("heart3"));
                    Destroy(GameObject.Find("heart2"));
                    Destroy(GameObject.Find("heart1"));
                    break;
            }

            switch(HR_P2)
            {
                case 1:
                    Destroy(GameObject.Find("heartA1"));
                    break;

                case 2:
                    Destroy(GameObject.Find("heartA1"));
                    Destroy(GameObject.Find("heartA2"));
                    break;

                case 3:
                    Destroy(GameObject.Find("heartA1"));
                    Destroy(GameObject.Find("heartA2"));
                    Destroy(GameObject.Find("heartA3"));
                    break;

                case 4:
                    Destroy(GameObject.Find("heartA1"));
                    Destroy(GameObject.Find("heartA2"));
                    Destroy(GameObject.Find("heartA3"));
                    Destroy(GameObject.Find("heartA4"));
                    break;
            }

        }
    }
}
