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

    private int jumpForce;
    private int moveSpeed;
    private int attackForce;

    private int maxSpeed;

    private bool canJump;
    private bool jumpPending;


    private bool attackPending;
    private bool isAttacking;

    private int health = 3;


    private float isAttackingTimer;
    private float attackCooldown;

    private Vector2 attackDirection;


    public bool getIsAttacking()
    {
        return isAttacking;
    }

    public int getHealth()
    {
        return health;
    }




    void Start()
    {
        jumpForce = 18;
        attackForce = 17;
        maxSpeed = 20;

        movement = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 9;
        canJump = true;



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
            health -= 1;
            Debug.Log("player" + playerNumber + " has " + health + " left");
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
           
         

            if (Time.time >= attackCooldown)
            {
                attackDirection = GetAttackDirection();
                attackPending = true;
                attackCooldown = Time.time + 1.5f;

               
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

       
       

        if(!isAttacking)
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
        attackInput();

    }

    private void FixedUpdate()
    {

       

        if (jumpPending)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPending = false;
        }

        if (attackPending)
        {
            isAttacking = true;
            isAttackingTimer = Time.time + 0.2f;
           

            rb.AddForce(attackDirection * attackForce * (attackDirection.y == 1 ? 2 : 1), ForceMode2D.Impulse);

            attackPending = false;
        }

        rb.velocity = new Vector2(
            isAttacking ? rb.velocity.x : horizontalMovement().x,
            rb.velocity.y
        );
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
