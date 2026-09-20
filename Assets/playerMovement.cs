using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int playerNumber; //identifies which player is which

    // Start is called before the first frame update
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Vector2 movement;
    Animator anim;
    public playerMovement player1;
    public playerMovement player2;

    [SerializeField] private Collider2D attackHitbox; //for player hitbox

    private int jumpForce;
    private int moveSpeed;
    private int attackForce;
    private int maxSpeed;

    private bool canJump; 
    private bool jumpPending;


    private bool attackPending;
    private bool isAttacking;

    private int health = 3;
    private bool parryPending;
    private bool isParrying;

    private float stunnedTimer;


    private float isAttackingTimer;
    private float attackCooldown;

    private float jumpCooldown;

    private float isParryingTimer;
    private float parryCooldown;

    private Vector2 attackDirection;

    public AudioSource dashSource;
    public AudioClip dashSound;

    public AudioSource jumpSource;
    public AudioClip jumpSound;

    public AudioSource hitSource;
    public AudioClip hitSound;

    public AudioSource parrySource;
    public AudioClip parrySound;


    private readonly HashSet<playerMovement> hitThisDash =
        new HashSet<playerMovement>();


    private bool passingThrough;


    public bool getIsAttacking()
    {
        return isAttacking;
    }

    public int getHealth()
    {
        return health;
    }

    public void setHealth(int nHealth)
    {
        this.health = nHealth;
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
        maxSpeed = 25;


        movement = Vector2.zero;
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        moveSpeed = 9;
        canJump = true;



        parryCooldown = 0;
        isParryingTimer = 0;
        stunnedTimer = 0;
        jumpCooldown = 0;

        parryPending = false;
        isParrying = false;

        player1 = GameObject.Find("player1").GetComponent<playerMovement>();
        player2 = GameObject.Find("player2").GetComponent<playerMovement>();

        //the hitbox children are added per-scene, so the script can easily be
        //missing off them - without it nothing ever calls Hitbox()
        if (attackHitbox.GetComponent<HitboxScript>() == null)
        {
            Debug.LogWarning(
                "Player " + playerNumber + ": " + attackHitbox.name +
                " had no HitboxScript, adding one at runtime"
            );

            attackHitbox.gameObject.AddComponent<HitboxScript>();
        }

        attackHitbox.enabled = false;




        //Debug.Log(gameObject.name + " -> RB: " + rb.GetInstanceID());
    }




    public void Hitbox(playerMovement otherPlayer)
    {
        //only a live dash does damage, the hitbox collider is disabled
        //outside of one, but the parried-dash collision path is not
        if (!isAttacking || otherPlayer == null || otherPlayer == this)
        {
            return;
        }

        //Add returns false if this dash already connected with them
        if (!hitThisDash.Add(otherPlayer))
        {
            return;
        }

        Debug.Log("PLAYER " + playerNumber +
                  " HIT PLAYER " + otherPlayer.playerNumber);

        if (otherPlayer.isParrying)
        {
            Debug.Log("PLAYER " + otherPlayer.playerNumber + " PARRIED!");

            // The attacker gets stunned
            setIstunned(Time.time + 0.7f);
            return;
        }

        if (otherPlayer.health <= 0)
        {
            return;
        }

        hitSource.PlayOneShot(hitSound);
        otherPlayer.health -= 1;

        if (otherPlayer.playerNumber == 2)
        {
            otherPlayer.anim.SetTrigger("Hurt");
        }
        else if (otherPlayer.playerNumber == 1)
        {
            otherPlayer.anim.SetTrigger("Hurt");
        }

        Debug.Log(
            "PLAYER " + otherPlayer.playerNumber +
            " HEALTH: " + otherPlayer.health
        );
    }


    /// <summary>
    /// decides, for this frame, whether the two players are allowed to move
    /// through each other. a dash passes through the other player, unless that
    /// player is parrying it - then they stay solid and the dash is blocked.
    /// both players run this and reach the same answer, so neither one can
    /// switch the other's pass-through off while their dash is still going.
    /// </summary>
    private void updatePassThrough()
    {
        playerMovement otherPlayer =
            playerNumber == 1 ? player2 : player1;

        if (otherPlayer == null || otherPlayer == this)
        {
            return;
        }

        bool passThrough =
            (isAttacking && !otherPlayer.isParrying) ||
            (otherPlayer.isAttacking && !isParrying);

        if (passThrough == passingThrough)
        {
            return;
        }

        passingThrough = passThrough;

        Physics2D.IgnoreCollision(
            bodyCollider,
            otherPlayer.bodyCollider,
            passThrough
        );
    }




    /// <summary>
    /// sees if user can jump: tells the game to make the player jump
    /// </summary>
    public void jumpInput()
    {

        if (isAttacking)
        {
            return;
        }

        if ((Input.GetKey(KeyCode.W) && playerNumber == 1) || (Input.GetKey(KeyCode.UpArrow) && playerNumber == 2))
        {

            if (canJump )
            {
                if(playerNumber == 1)
                {
                    anim.SetBool("IsJumping", true);
                }
                jumpPending = true;
                canJump = false;
                jumpCooldown = Time.time + 0.2f;
            }
        }
    }


    /// <summary>
    /// sees if user can attack: tells the game to make the player attack
    /// </summary>
    public void attackInput()
    {
        if ((Input.GetKeyDown(KeyCode.LeftShift) && playerNumber == 1) || (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Comma)) && playerNumber == 2)
        {
            if (isParrying)
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


        if ((Input.GetKeyDown(KeyCode.Space) && playerNumber == 1) || ( (Input.GetKeyDown(KeyCode.Keypad1)  ) || (Input.GetKeyDown(KeyCode.Period) )) && playerNumber == 2)
        {
            if (isAttacking)
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
        if (playerNumber == 1)
        {
            if (Input.GetKey(KeyCode.A))
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

        if (Time.time >= isAttackingTimer)
        {
            if (isAttacking)
            {
                isAttacking = false;
                attackHitbox.enabled = false;
                hitThisDash.Clear();

                Debug.Log("PLAYER " + playerNumber + " DASH ENDED");
            }
        }




        if (Time.time >= isParryingTimer) //when parry is over
        {
            if (isParrying)
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
            rb.velocity = new Vector2(rb.velocity.x, 0);

            jumpSource.PlayOneShot(jumpSound);

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPending = false;
        }

        if (attackPending)
        {
            anim.SetTrigger("Attack");
            dashSource.PlayOneShot(dashSound);


            Debug.Log("PLAYER " + playerNumber + " STARTING DASH");

            hitThisDash.Clear();
            attackHitbox.enabled = true;

            isAttacking = true;
            isAttackingTimer = Time.time + 0.3f;

            rb.AddForce(
                attackDirection * attackForce *
                (attackDirection.y > 0 ? 2 : 1),
                ForceMode2D.Impulse
            );

            attackPending = false;
        }

        if (parryPending)
        {
            isParrying = true;
            isParryingTimer = Time.time + 0.3f;

            parrySource.PlayOneShot(parrySound);


            parryPending = false;
        }


        if (stunnedTimer < Time.time) //if not stunned
        {
            rb.velocity = new Vector2(
            Mathf.Clamp( (isAttacking ? rb.velocity.x : horizontalMovement().x), (-1 * maxSpeed) , maxSpeed),
            Mathf.Clamp( rb.velocity.y, (-1 * maxSpeed), maxSpeed ) );
        }

        updatePassThrough();
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("floor") && jumpCooldown <= Time.time)
        {
            anim.SetBool("IsJumping", false);
            canJump = true;
            return;
        }

       
        if (!isAttacking)
        {
            return;
        }

        Hitbox(collision.collider.GetComponentInParent<playerMovement>());
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
