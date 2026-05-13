using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{

  
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 7f;

    [Header("Ladder")]
    public float climbSpeed = 4f;
    public KeyCode attachKey = KeyCode.E;
    public TMP_Text ladderPromptText;

    // Public so other scripts can read it if needed
    [HideInInspector] public bool isClimbing = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float moveInput;
    private float verticalInput;

    private bool isGrounded;

    // Ladder state
    private bool isOnLadder;
    private bool isAttached;
    private bool attachedFromTop;
    private Transform currentLadder;
    private float defaultGravity;

    private int playerLayer;
    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        defaultGravity = rb.gravityScale;

        playerLayer = LayerMask.NameToLayer("Player");
        groundLayer = LayerMask.NameToLayer("Ground");

        if (ladderPromptText != null)
            ladderPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        HandleLadder();
        HandleMovement();
        HandleAnimations();
    }

    // ─────────────────────────────────────────
    //  MOVEMENT
    // ─────────────────────────────────────────
    void HandleMovement()
    {
        // Block normal movement while climbing
        if (isClimbing) return;

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("IsJumping");
        }

        // Flip sprite
        if (moveInput > 0) sr.flipX = false;
        else if (moveInput < 0) sr.flipX = true;
    }

    // ─────────────────────────────────────────
    //  LADDER
    // ─────────────────────────────────────────
    void HandleLadder()
    {
        // Attach / Detach toggle
        if (isOnLadder && Input.GetKeyDown(attachKey))
        {
            if (!isAttached) AttachToLadder();
            else DetachFromLadder();
        }

        // Auto detach if walked off ladder
        if (!isOnLadder && isAttached)
            DetachFromLadder();

        // Prompt UI
        if (ladderPromptText != null)
        {
            ladderPromptText.gameObject.SetActive(isOnLadder && !isAttached);
            if (isOnLadder && !isAttached)
                ladderPromptText.text = $"Press {attachKey} to climb";
        }

        if (isAttached)
        {
            isClimbing = true;
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);

            if (Mathf.Abs(verticalInput) < 0.1f)
                rb.linearVelocity = Vector2.zero;

            HandleClimbingCollision();
        }
        else
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
        }
    }

    void HandleClimbingCollision()
    {
        if (verticalInput > 0.1f)
        {
            // ── Moving UP ──────────────────────────────────
            // Ignore ground so player passes through top surface
            Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);
            attachedFromTop = false;
        }
        else if (verticalInput < -0.1f)
        {
            // ── Moving DOWN ────────────────────────────────
            if (attachedFromTop)
            {
                // Attached from top — keep ignoring until player
                // has fully sunk below the surface
                if (currentLadder != null)
                {
                    Collider2D ladderCol = currentLadder.GetComponent<Collider2D>();
                    float ladderTop = ladderCol != null
                        ? ladderCol.bounds.max.y
                        : currentLadder.position.y;

                    // Once player is clearly below the surface, restore collision
                    if (transform.position.y < ladderTop - 0.6f)
                    {
                        attachedFromTop = false;
                        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
                    }
                    // else: keep ignoring so they sink through the floor
                }
            }
            else
            {
                // Normal downward climb — ground blocks underground entry
                Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
            }
        }
        // verticalInput == 0 → hold position, keep current collision state
    }

    void AttachToLadder()
    {
        isAttached = true;

        // If grounded when pressing E, player is on top and wants to go down
        attachedFromTop = isGrounded;

        // Snap to ladder X axis
        if (currentLadder != null)
        {
            transform.position = new Vector3(
                currentLadder.position.x,
                transform.position.y,
                transform.position.z
            );
        }

        // Pre-ignore ground so player isn't immediately blocked when descending
        if (attachedFromTop)
            Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);
    }

    void DetachFromLadder()
    {
        isAttached = false;
        isClimbing = false;
        attachedFromTop = false;

        rb.gravityScale = defaultGravity;
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
    }

    // ─────────────────────────────────────────
    //  ANIMATIONS
    // ─────────────────────────────────────────
    void HandleAnimations()
    {
        bool isWalking = moveInput != 0 && !isClimbing;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);

        anim.SetBool("IsWalking", isWalking);
        anim.SetBool("IsRunning", isRunning);
        anim.SetBool("IsGrounded", isGrounded && !isClimbing);
        anim.SetBool("IsClimbing", isClimbing);
        anim.SetFloat("YVelocity", rb.linearVelocity.y);
    }

    // ─────────────────────────────────────────
    //  COLLISION DETECTION
    // ─────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if(collision.gameObject.name=="MovingPlatform")
        {
            transform.SetParent(collision.transform);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = true;
            currentLadder = collision.transform;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = false;
            currentLadder = null;
            DetachFromLadder();
        }
    }
}