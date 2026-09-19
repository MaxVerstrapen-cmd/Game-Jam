using UnityEngine;
public class PlayerAnimation : MonoBehaviour
{

    Animator anim;
    public int playerNumber;

    void Start()
    {
        anim = GetComponent<Animator>();
        //grab player number from playermovement
        playerNumber = GetComponent<playerMovement>().playerNumber;
    }

    void Update()
    {
        float x = GetHorizontalInput();
        anim.SetBool("IsRunning", x != 0);

        if (x != 0)
        {
            float size = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(x < 0 ? -size : size, transform.localScale.y, transform.localScale.z);
        }
    }


    float GetHorizontalInput()
    {
        if (playerNumber == 1)
        {
            if (Input.GetKey(KeyCode.A))
            {
                return -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                return 1;
            }
        }
        else if (playerNumber == 2)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return -1;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                return 1;
            }
        }

        return 0;
    }
}
