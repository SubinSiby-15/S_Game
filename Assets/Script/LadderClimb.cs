using UnityEngine;
using TMPro; 

public class LadderClimb : MonoBehaviour
{
    public float climbSpeed = 4f;
    public KeyCode attachKey = KeyCode.E;

    public TMP_Text ladderPromptText; 

    private Rigidbody2D rb;
    private PlayerMovement playerMove;

    private Transform currentLadder;
    private bool isOnLadder = false;
    private bool isAttached = false;
    private float defaultGravity;
    private int playerLayer;
    private int groundLayer;
    public float verticalInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMove = GetComponent<PlayerMovement>();

        playerLayer = LayerMask.NameToLayer("Player");
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void Start()
    {
        defaultGravity = rb.gravityScale;

        if (ladderPromptText != null)
        {
            ladderPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        // ------Toggle attach/detach------
        if (isOnLadder && Input.GetKeyDown(attachKey))
        {
            if (!isAttached)
                AttachToLadder();
            else
                DetachFromLadder();
        }

        if (!isOnLadder && isAttached)
        {
            DetachFromLadder();
        }

        // ------ Update UI------

        if (ladderPromptText != null)
        {
            if (isOnLadder && !isAttached)
            {
                ladderPromptText.gameObject.SetActive(true);

                ladderPromptText.text = $"Press {attachKey} to climb";
            }
            else
            {
                ladderPromptText.gameObject.SetActive(false);
            }
        }

        // ------Climbing------
        if (isAttached)
        {
            playerMove.isClimbing = true;
            rb.gravityScale = 0f;

            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);

            if (Mathf.Abs(verticalInput) < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.gravityScale = defaultGravity;
            playerMove.isClimbing = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            isOnLadder = true;
            currentLadder = collision.transform;
            Debug.Log("Player Triggered :"+ collision.name);
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

    void AttachToLadder()
    {
        isAttached = true;
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);

        if (currentLadder != null)
        {
          
            transform.position = new Vector3(
                currentLadder.position.x,   
                transform.position.y,      
                transform.position.z
            );
        }
    }

    void DetachFromLadder()
    {
        isAttached = false;
        playerMove.isClimbing = false;
        rb.gravityScale = defaultGravity;
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);
    }
}