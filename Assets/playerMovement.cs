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
    private bool canJump;
    private bool jumpPending;

    void Start()
    {
        jumpForce = 15;
        movement = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = 10;
        canJump = true;



    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");

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

    private void FixedUpdate()
    {

        movement.x = Input.GetAxisRaw("Horizontal");

        
        if(jumpPending)
          {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPending = false;
        }


        rb.velocity = new Vector2(
            movement.x * moveSpeed,
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


}
