using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthBarUpdater : MonoBehaviour
{

    public GameObject player1, player2;

    public GameObject hp1, hp2;

    public playerMovement p1;
    public playerMovement p2;

    private int OriginalHealth1;
    private int OriginalHealth2;

    // Start is called before the first frame update
    void Start()
    {
        player1 = GameObject.Find("player1");
        player2 = GameObject.Find("player2");

        hp1 = GameObject.Find("healthbarP1");
        hp2 = GameObject.Find("healthbarP2");
        
        p1 = player1.GetComponent<playerMovement>();
        p2 = player2.GetComponent<playerMovement>();

        OriginalHealth1 = p1.getHealth();
        OriginalHealth2 = p2.getHealth();
    }

    // Update is called once per frame
    void Update()
    {

        if (p1.getHealth() != OriginalHealth1)
        {
            OriginalHealth1 = p1.getHealth();

            hp1.transform.localScale += new Vector3(-0.34f, 0, 0);

            if (OriginalHealth1 == 0)
            {
                Destroy(player1);
                Destroy(hp1);
            }
            
        }


        if (p2.getHealth() != OriginalHealth2)
        {
            OriginalHealth2 = p2.getHealth();

            hp2.transform.localScale += new Vector3(-0.34f, 0, 0);

            if (OriginalHealth2 == 0)
            {
                Destroy(player2);
                Destroy(hp2);
            }
        }

    }


}
