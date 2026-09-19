using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;
    private Vector2 movement;

    private int jumpForce;
    private int moveSpeed;
    private int attackForce;

    private bool canJump;
    private bool jumpPending;

    private bool canAttack;
    private bool attackPending;
    private bool isAttacking;


    private MinimalTimer isAttackingTimer;
    private MinimalTimer attackCooldownTimer;


    void Start()
    {
        jumpForce = 12;
        attackForce = 12;

        movement = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 7;
        canJump = true;
        canAttack = true;

        
    }




    /// <summary>
    /// sees if user can jump: tells the game to make the player jump
    /// </summary>
    public void jumpInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("w pressed::" + canJump);

            if (canJump)
            {
                Debug.Log("went through");
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
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            
            Debug.Log("shift pressed::" + canAttack);

            if (canAttack)
            {
                attackPending = true;
                canAttack = false;
            }

        }
    }

    void Update()
    {
        if(isAttackingTimer.IsCompleted)
        {
            isAttacking = false;
            
        }

        if(attackCooldownTimer.IsCompleted)
        {
            canAttack = true;
        }

        if(!isAttacking)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
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
            isAttackingTimer = MinimalTimer.Start(0.4f);
            attackCooldownTimer = MinimalTimer.Start(2f);

            Vector2 attackDirection = GetAttackDirection();
            rb.AddForce(attackDirection * attackForce, ForceMode2D.Impulse);

            attackPending = false;
        }

        rb.velocity = new Vector2(
            isAttacking ? rb.velocity.x : movement.x * moveSpeed,
            rb.velocity.y
        );
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("floor"))
        {
            Debug.Log("collision" + canJump);
            canJump = true;
            Debug.Log("collision" + canJump);
        }
    }

    /// <summary>
    /// move to other script?
    /// </summary>
    public readonly struct MinimalTimer
    {
        public static MinimalTimer Start(float duration) => new(duration);
        public bool IsCompleted => Time.time >= _triggerTime && _triggerTime != 0;

        private readonly float _triggerTime;
        private MinimalTimer(float duration) => _triggerTime = Time.time + duration;
    }

    //chooses (2d) vector direction based on player input
    Vector2 GetAttackDirection()
    {
        Vector2 direction = Vector2.zero;

        if (Input.GetKey(KeyCode.A))
            direction.x -= 1;

        if (Input.GetKey(KeyCode.D))
            direction.x += 1;

        if (Input.GetKey(KeyCode.S))
            direction.y -= 1;

        if (Input.GetKey(KeyCode.W))
            direction.y += 1;

        return direction.normalized;
    }

}
