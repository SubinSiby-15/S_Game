using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float jumpForce = 5f;

    [Header("Ladder")]
    public float climbSpeed = 4f;
    public KeyCode attachKey = KeyCode.E;
    public TMP_Text ladderPromptText;

    private Rigidbody2D rb;
    private SpriteRenderer spr;
    private Animator anim;

    private float horizontalInput;
    private float verticalInput;

    private bool isGrounded;
    private bool isJumping;
    private bool isClimbing;
    private bool isOnLadder;
    private bool isAttached;
    private bool attachedFromTop; // ← NEW: tracks if player attached while standing on ground

    private Transform currentLadder;
    private float defaultGravity;

    private int playerLayer;
    private int groundLayer;
    private int playerStatus = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        defaultGravity = rb.gravityScale;

        playerLayer = LayerMask.NameToLayer("Player");
        groundLayer = LayerMask.NameToLayer("Ground");

        if (ladderPromptText != null)
            ladderPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        HandleLadder();
        HandleMovement();
        HandleAnimation();
    }

    void HandleMovement()
    {
        if (isClimbing) return;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0) spr.flipX = false;
        else if (horizontalInput < 0) spr.flipX = true;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
        }
    }

    void HandleLadder()
    {
        // Attach / Detach toggle
        if (isOnLadder && Input.GetKeyDown(attachKey))
        {
            if (!isAttached) AttachToLadder();
            else DetachFromLadder();
        }

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
            // ── Moving UP ──────────────────────────────────────────────
            // Ignore ground so player can pass through top surface
            Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);
            attachedFromTop = false; // No longer relevant once moving up
        }
        else if (verticalInput < -0.1f)
        {
            // ── Moving DOWN ────────────────────────────────────────────
            if (attachedFromTop)
            {
                // Player attached while standing on ground.
                // Keep ignoring until they've dropped below the surface.
                if (currentLadder != null)
                {
                    Collider2D ladderCol = currentLadder.GetComponent<Collider2D>();
                    float ladderTop = ladderCol != null
                        ? ladderCol.bounds.max.y
                        : currentLadder.position.y;

                    // Once player centre is clearly below the top surface, restore collision
                    if (transform.position.y < ladderTop - 0.6f)
                    {
                        attachedFromTop = false;
                        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
                    }
                    // else: keep ignoring so they can sink through the surface
                }
            }
            else
            {
                // Normal downward climb — restore collision so player can't go underground
                Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
            }
        }
        // verticalInput == 0 → keep whatever state is currently active
    }

    void HandleAnimation()
    {
        if (isClimbing)
            playerStatus = 5;
        else if (isGrounded)
        {
            isJumping = false;
            playerStatus = horizontalInput != 0
                ? (Input.GetKey(KeyCode.LeftShift) ? 2 : 1)
                : 0;
        }
        else
        {
            playerStatus = rb.linearVelocity.y > 0 ? 3 : 4;
        }

        anim.SetInteger("STATUS", playerStatus);
        anim.SetBool("IS_GROUNDED", isGrounded);
        anim.SetBool("JUMPING", isJumping);
        anim.SetBool("CLIMBING", isClimbing);
    }

    void AttachToLadder()
    {
        isAttached = true;

        // If grounded when attaching → player is on top, wants to go down
        // Flag this so HandleClimbingCollision can manage the entry phase
        attachedFromTop = isGrounded;

        if (currentLadder != null)
        {
            transform.position = new Vector3(
                currentLadder.position.x,
                transform.position.y,
                transform.position.z
            );
        }

        // If attaching from top, immediately ignore collision
        // so the player isn't blocked by the ground surface when descending
        if (attachedFromTop)
            Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);
    }

    void DetachFromLadder()
    {
        isAttached = false;
        isClimbing = false;
        attachedFromTop = false;

        rb.gravityScale = defaultGravity;

        // Always restore ground collision on detach
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
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