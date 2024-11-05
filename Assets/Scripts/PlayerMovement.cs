// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Physics engine. Decides how the player interacts with movement and collidable objects.

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // Movement and physics parameters
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float airDeceleration = 10f;
    [SerializeField] private float jumpHeight = 20f;
    [SerializeField] private float jumpHeightBonus = 0f; // Additional jump height based on speed
    [SerializeField] private float wallJumpX = 10f;
    [SerializeField] private float wallJumpY = 20f;

    private float horizontalInput;
    private bool jumpInput;
    private int wallSide; // -1 for left wall, 1 for right wall, 0 if not on a wall
    public Animator animator;

    // Wall jump variables
    private bool wallJumping = false;
    private float wallJumpTime = 0.2f;
    private float wallJumpCounter;

    [HideInInspector] public bool agentActive = false; // Indicates if the agent is controlling the player

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        // Check if the PlatformerAgent component is attached
        if (GetComponent<PlatformerAgent>() != null)
        {
            agentActive = true;
        }
    }

    // Method to set inputs from the AI agent
    public void SetInput(float horizontal, bool jump)
    {
        horizontalInput = horizontal;
        jumpInput = jump;
    }

    private void Update()
    {
        // Get input from player controls if not controlled by the agent
        if (!agentActive)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            jumpInput = Input.GetKeyDown(KeyCode.Space);
        }

        // Check if player is grounded and/or on a wall
        bool grounded = isGrounded();
        wallSide = onWall();

        // Reset gravity and movement if grounded
        if (grounded)
        {
            body.gravityScale = 5;
            wallSide = 0;
            wallJumping = false;
            animator.SetBool("isJumping", false);
        }

        // Handle wall jump timer
        if (wallJumping)
        {
            wallJumpCounter -= Time.deltaTime;
            if (wallJumpCounter <= 0)
            {
                wallJumping = false;
            }
        }

        // Calculate target speed based on input and max speed
        float targetSpeed = horizontalInput * maxSpeed;
        float speedDifference = targetSpeed - body.linearVelocity.x;

        // Determine acceleration rate
        float accelRate;
        if (grounded)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;

        // Apply acceleration to reach target speed
        if (!wallJumping || grounded)
        {
            float movement = accelRate * Time.deltaTime;
            float newVelocityX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, movement);
            body.linearVelocity = new Vector2(newVelocityX, body.linearVelocity.y);
        }

        // Wall sliding logic
        if (wallSide != 0 && !grounded && !wallJumping)
        {
            body.gravityScale = 2; // Reduce gravity to create sliding effect
            body.linearVelocity = new Vector2(0, Mathf.Clamp(body.linearVelocity.y, -5, float.MaxValue));
        }
        else
        {
            body.gravityScale = 5; // Reset gravity to normal
        }

        // Handle jumping
        if (jumpInput)
        {
            if (grounded)
            {
                // Jump height increases with movement speed
                float speedFactor = Mathf.Abs(body.linearVelocity.x) / maxSpeed;
                float adjustedJumpHeight = jumpHeight + speedFactor * jumpHeightBonus;
                Jump(new Vector2(body.linearVelocity.x, adjustedJumpHeight));
            }
            else if (wallSide != 0)
            {
                // Wall jump in the opposite direction
                Vector2 wallJumpDirection = new Vector2(-wallSide * wallJumpX, wallJumpY);
                Jump(wallJumpDirection, isWallJump: true);
            }
            jumpInput = false; // Reset jump input
        }

        // Flip player direction based on movement
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // Update animations
        animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));
    }

    private void Jump(Vector2 jumpForce, bool isWallJump = false)
    {
        body.linearVelocity = jumpForce;
        animator.SetBool("isJumping", true);

        if (isWallJump)
        {
            wallJumping = true;
            wallJumpCounter = wallJumpTime;
        }
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.down, 0.1f, groundLayer
        );
        return raycastHit.collider != null;
    }

    private int onWall()
    {
        RaycastHit2D hitLeft = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.left, 0.2f, wallLayer
        );

        RaycastHit2D hitRight = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.right, 0.2f, wallLayer
        );

        if (hitLeft.collider != null)
        {
            return -1; // On left wall
        }
        if (hitRight.collider != null)
        {
            return 1; // On right wall
        }

        return 0; // Not on a wall
    }
}
