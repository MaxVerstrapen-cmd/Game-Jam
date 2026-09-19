using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int playerNumber; //identifies which player is which

    // Start is called before the first frame update
    private Rigidbody2D rb;
    private Vector2 movement;
    Animator anim;
    public playerMovement player1;
    public playerMovement player2;

    private int jumpForce;
    private int moveSpeed;
    private int attackForce;


    private bool canJump;
    private bool jumpPending;


    private bool attackPending;
    private bool isAttacking;

    private bool parryPending;
    private bool isParrying;

    private float stunnedTimer;

    private int health;


    private float isAttackingTimer;
    private float attackCooldown;

    private float isParryingTimer;
    private float parryCooldown;

    private Vector2 attackDirection;


    public bool getIsAttacking()
    {
        return isAttacking;
    }

    public int getHealth()
    {
        return health;
    }

    public void setIstunned(float stunned)
    {
        this.stunnedTimer = stunned;
    }




    void Start()
    {
        anim = GetComponent<Animator>();
        jumpForce = 18;
        attackForce = 17;


        movement = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 9;
        canJump = true;


        health = 3;

        parryCooldown = 0;
        isParryingTimer = 0;
        stunnedTimer = 0;

        parryPending = false;
        isParrying = false;

        //player1 = GameObject.Find("player1").GetComponent<playerMovement>();
        //player2 = GameObject.Find("player2").GetComponent<playerMovement>();
    




    //Debug.Log(gameObject.name + " -> RB: " + rb.GetInstanceID());


}

    public void playerCollide(Collision2D collision)
    {
        //find the player object that the "current" player collided with
        playerMovement otherPlayer =
        collision.gameObject.GetComponent<playerMovement>();

        if (otherPlayer != null &&
           !isAttacking &&
           otherPlayer.getIsAttacking())
        {
            Debug.Log("player" + playerNumber + " Parry??? " + isParrying);

            if (isParrying)
            {
               otherPlayer.setIstunned(Time.time + 0.7f);
            }
            else
            {
                health -= 1;
               
            }
                
        }
    }




    /// <summary>
    /// sees if user can jump: tells the game to make the player jump
    /// </summary>
    public void jumpInput()
    {

        
        if (isAttacking)
            return;

        if ((Input.GetKey(KeyCode.W) && playerNumber == 1) || (Input.GetKey(KeyCode.UpArrow) && playerNumber == 2))
        {
            if (canJump)
            {
                jumpPending = true;
                canJump = false;
            }
        }
    }


    /// <summary>
    /// sees if user can attack: tells the game to make the player attack
    /// </summary>
    public void attackInput()
    {
        if ( (Input.GetKeyDown(KeyCode.LeftShift) && playerNumber == 1) || (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) && playerNumber == 2) 
        {
            if(isParrying)
            {
                return;
            }
          

            if (Time.time >= attackCooldown)
            {
                attackDirection = GetAttackDirection();
                attackPending = true;
                attackCooldown = Time.time + 1.5f;

               
            }

        }
    }

    /// <summary>
    /// sees if user can attack: tells the game to make the player attack
    /// </summary>
    public void parryInput()
    {


        if ((Input.GetKeyDown(KeyCode.Q) && playerNumber == 1) || (Input.GetKeyDown(KeyCode.Slash)) && playerNumber == 2)
        {
            if(isAttacking)
            {
                return;
            }

            if (Time.time >= parryCooldown)
            {
               
                parryPending = true;
                parryCooldown = Time.time + 2f;
                Debug.Log("Player " + playerNumber + " IS PARRYING");

            }
            

        }
    }

    /// <summary>
    /// horizontal movement for each player
    /// </summary>
    /// <returns></returns>
    public Vector2 horizontalMovement()
    {
        if(playerNumber == 1)
        {
            if(Input.GetKey(KeyCode.A))
            {
                return new Vector2(-moveSpeed, rb.velocity.y);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                return new Vector2(moveSpeed, rb.velocity.y);
            }
            else
            {
                return new Vector2(0, rb.velocity.y);
            }
        }

        else if (playerNumber == 2)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return new Vector2(-moveSpeed, rb.velocity.y);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                return new Vector2(moveSpeed, rb.velocity.y);
            }
            else
            {
                return new Vector2(0, rb.velocity.y);
            }
        }

        return new Vector2(rb.velocity.x, rb.velocity.y);
    }

    void Update()
    {

        if(Time.time >= isAttackingTimer) //when attack is over
        {
            isAttacking = false;
            
        }

        if (Time.time >= isParryingTimer) //when parry is over
        {
            if(isParrying)
            {
                Debug.Log("!!!!!Player " + playerNumber + " NOT PARRYING");
            }
            isParrying = false;
            
        }



        if (stunnedTimer < Time.time)
        {
            if (!isAttacking)
            {
                // movement.x = horizontalMovement().x; deprecated
                horizontalMovement();
                // rb.gravityScale = 3.0f;
            }
            else
            {
                // rb.gravityScale = 0.0f;
            }

            jumpInput();
            parryInput();
            attackInput();
        }

    }

    private void FixedUpdate()
    {

       

        if (jumpPending)
        {
            Debug.Log("player" + playerNumber + " has " + health + " left");
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPending = false;
        }

        if (attackPending)
        {
            if (playerNumber == 1)
            {
                anim.SetTrigger("Attack");
            }

            isAttacking = true;
            isAttackingTimer = Time.time + 0.3f;
            

            rb.AddForce(attackDirection * attackForce * (attackDirection.y == 1 ? 2 : 1), ForceMode2D.Impulse);

            attackPending = false;
        }

        if (parryPending)
        {
            isParrying = true;
            isParryingTimer = Time.time + 0.4f;



            parryPending = false;
        }

        if (stunnedTimer < Time.time) //if not stunned
        {
                rb.velocity = new Vector2(
                isAttacking ? rb.velocity.x : horizontalMovement().x,
                rb.velocity.y
                );
        }
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        //check if jump should refrwsh
        if (collision.gameObject.CompareTag("floor"))
        {
            Debug.Log("collision" + canJump);
            canJump = true;
            Debug.Log("collision" + canJump);
        }

        //checks if players are damaged
        playerCollide(collision);
    }

    
    //chooses (2d) vector direction based on player input
    Vector2 GetAttackDirection()
    {
        Vector2 direction = Vector2.zero;

        Debug.Log("P2 DASH BUTTON PRESSED");

        if (playerNumber == 1)
        {
            if (Input.GetKey(KeyCode.A))
                direction.x -= 1;

            if (Input.GetKey(KeyCode.D))
                direction.x += 1;

            if (Input.GetKey(KeyCode.S))
                direction.y -= 1;

            if (Input.GetKey(KeyCode.W))
                direction.y += 1;
        }

        if (playerNumber == 2)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                direction.x -= 1;

            if (Input.GetKey(KeyCode.RightArrow))
                direction.x += 1;

            if (Input.GetKey(KeyCode.DownArrow))
                direction.y -= 1;

            if (Input.GetKey(KeyCode.UpArrow))
                direction.y += 1;
        }

        Debug.Log(direction);

        return direction.normalized;
    }

}
