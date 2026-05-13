using System.Diagnostics;
using UnityEngine;

public class PLAYER_MOVEMENT : MonoBehaviour
{
    public float Player_Speed = 2;
    public float jumpForce = 5f;
    Rigidbody2D RGBD;
    SpriteRenderer SPRT;
    Animator ANIM;
    float horizontal_input;
    bool is_Grounded;
    bool is_Jumping;
    int player_status = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RGBD = GetComponent<Rigidbody2D>();
        SPRT = GetComponent<SpriteRenderer>();
        ANIM = GetComponent<Animator>();

    }


    void Update()
    {
        horizontal_input = Input.GetAxis("Horizontal");
        RGBD.linearVelocity = new Vector2(horizontal_input * Player_Speed, RGBD.linearVelocity.y);
        if (horizontal_input > 0)
        {
            SPRT.flipX = false;
        }
        else if (horizontal_input < 0)
        {
            SPRT.flipX = true;
        }

        if (is_Grounded)
        {
            is_Jumping = false;
            if (horizontal_input != 0)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Player_Speed = 4;  // A faster speed for running
                    player_status = 2; // A new status for your running animation
                }
                else
                {
                    Player_Speed = 2;  // Normal walking speed
                    player_status = 1; // Walking status
                }
            }
            else
            {
                player_status = 0; // Walking Idle Status
            }
        }
        else
        {
            if (RGBD.linearVelocity.y > 0)
            {
                player_status = 3; // Jumping Up Status
            }
            else
            {
                player_status = 4; // Falling Down Status
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {

            RGBD.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            is_Jumping = true;
        }
       
        
        ANIM.SetInteger("STATUS", player_status);
        ANIM.SetBool("IS_GROUNDED", is_Grounded);
        ANIM.SetBool("JUMPING", is_Jumping);
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            is_Grounded = true;
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            is_Grounded = false;
        }
    }
}
