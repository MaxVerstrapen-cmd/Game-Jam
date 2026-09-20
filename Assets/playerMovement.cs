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

    //VFX frame sequences - shared between both players, so these are set
    //identically on both Player.prefab and Player2.prefab
    [SerializeField] private Sprite[] parrySprites; //played on this player when they parry
    [SerializeField] private Sprite[] stunSprites; //played above this player when their attack gets parried

    private EffectFlipbook parryEffect;
    private EffectFlipbook stunEffect;

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

    //every player this dash has already damaged. one dash overlaps the other
    //player's body collider AND their hitbox child, and a parried dash also
    //reports through OnCollisionEnter2D, so without this a single dash would
    //land two or three times.
    private readonly HashSet<playerMovement> hitThisDash =
        new HashSet<playerMovement>();

    //mirrors the current Physics2D.IgnoreCollision state for this pair, so we
    //only poke the physics engine when it actually changes. re-applying it
    //every step resets the pair's contact state and re-fires collision events.
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

        //Parry plays at the base of the sword, sized to the 0.4s parry
        //window (isParryingTimer) below. Stun plays as a small halo just
        //above the head, sized to the 0.7s stun window (setIstunned) in
        //Hitbox(). Both offsets/scales are given in world units (tuned
        //against the knight, whose scene instance is scaled 3x) - see
        //CreateEffectChild for why that matters.
        parryEffect = CreateEffectChild("ParryEffect", new Vector3(0.9f, 1.65f, 0), 3f, parrySprites, 6f / 0.4f);
        stunEffect = CreateEffectChild("StunEffect", new Vector3(0, 1.75f, 0), 1.05f, stunSprites, 6f / 0.7f);

        //Debug.Log(gameObject.name + " -> RB: " + rb.GetInstanceID());
    }

    //worldPositionOffset/worldScale are in world units, not this player's
    //local space - the knight and samurai are scaled differently in the
    //scene (3x vs 5x respectively), and SetParent(transform, false) keeps
    //local position/scale relative to the parent, so the same local values
    //would render at different real sizes/offsets depending which player
    //they're parented to. Dividing by this player's own current scale
    //cancels that out so the effect looks the same on both. (The x offset's
    //sign still flips correctly with the character's own left/right facing,
    //since PlayerAnimation.cs flips by negating this same transform's
    //localScale.x, and that sign survives the division below.)
    private EffectFlipbook CreateEffectChild(string name, Vector3 worldPositionOffset, float worldScale, Sprite[] frames, float frameRate)
    {
        float parentScale = transform.localScale.x != 0 ? transform.localScale.x : 1f;

        GameObject effectObject = new GameObject(name);
        effectObject.transform.SetParent(transform, false);
        effectObject.transform.localPosition = worldPositionOffset / parentScale;
        effectObject.transform.localScale = Vector3.one * (worldScale / Mathf.Abs(parentScale));

        EffectFlipbook flipbook = effectObject.AddComponent<EffectFlipbook>();
        flipbook.Initialize(frames, frameRate);

        return flipbook;
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
            stunEffect.Play();
            return;
        }

        if (otherPlayer.health <= 0)
        {
            return;
        }

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
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpPending = false;
        }

        if (attackPending)
        {
            anim.SetTrigger("Attack");

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
            isParryingTimer = Time.time + 0.4f;
            parryEffect.Play();

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
