// PlayerMovement.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Physics engine. Decides how the player interacts with movement and collidable objects.

using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;//this player's rigidbody component
    private BoxCollider2D boxCollider; //this player's box collider
    [SerializeField] private LayerMask groundLayer; //ground layer mask for raycast collision detection
    public Animator animator;
    private AudioSource walkAudioSource;
    private AudioSource jumpAudioSource;

    // Movement and physics parameters
    [Header("Movement Parameters")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float airDeceleration = 10f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpHeight = 20f;
    [SerializeField] private float jumpHeightBonus = 0f; // Additional jump height based on speed
    [SerializeField] private float wallJumpX = 10f;
    [SerializeField] private float wallJumpY = 20f;
    [SerializeField] private float coyoteTime; //how much time after the player leaves the ground they can jump
    private float coyoteCounter; //how much time has passed since the player has left the ground
    private bool coyoteSpent = false; // tracks if player has used coyote time jump to prevent extra jumping    

    private float horizontalInput;//keyboard input for moving horizontally
    private bool jumpInput;//keyboard input for jumping
    [SerializeField] private int grounded; //holds returned value of onGround() -> 0 for floor, 1 for left wall, 2 for right wall, -1 for none

    // Wall jump variables
    private bool wallJumping = false;
    private float wallJumpTime = 0.2f;
    private float wallJumpCounter;

    [HideInInspector] public bool agentActive = false; // Indicates if the agent is controlling the player

    private LivesUI livesUI;

    // public override void Initialize()
    // {
    //     body = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    //     startPosition = transform.position; // Store the starting position
    //     previousPosition = startPosition; // Initialize previous position
    //     playerMovement = GetComponent<PlayerMovement>(); // Get the PlayerMovement component

    //     livesUI = FindObjectOfType<LivesUI>();
    //     GameObject flag = GameObject.FindGameObjectWithTag("Flag");
    //     if (flag != null) {
    //         goalTransform = flag.transform;
    //     }
    // }

    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Flag"))
    //     {
    //         SetReward(5.0f);  // Reward for reaching the goal
    //         // Debug.Log($"Agent Won. Final reward: {GetCumulativeReward()}");

    //         currentLevelCompletions++;  // Increment completion count
    //         if (currentLevelCompletions % 10 == 0) {
    //             Debug.Log($"Current Count: {currentLevelCompletions}");
    //         }

    //         if (currentLevelCompletions >= levelCompletionThreshold)
    //         {
    //             levelCompleted = true;  // Mark level as completed
    //             EndEpisode();  // End the episode
    //         }
    //         else
    //         {
    //             // Reset agent's position without ending the episode
    //             transform.position = startPosition;
    //             body.linearVelocity = Vector2.zero;
    //         }
    //     }
    //     else if (collision.CompareTag("DeathZone") || collision.CompareTag("Enemy") || collision.CompareTag("Hazard") || collision.CompareTag("Projectile"))
    //     {
    //         SetReward(-1.0f);  // Negative reward for dying or hitting a hazard
    //         // Debug.Log($"Agent Died. Final reward: {GetCumulativeReward()}");
    //         if (CompareTag("Player")){
    //             livesUI.LoseLifeP1();
    //         } else if (CompareTag("Player2")){
    //             livesUI.LoseLifeP2();
    //         }
    //         EndEpisode(); // End the episode
    //     }
    // }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        walkAudioSource = audioSources[0]; // Assign the first AudioSource component to walkAudioSource
        jumpAudioSource = audioSources[1]; // Assign the second AudioSource component to jumpAudioSource
        coyoteCounter = coyoteTime; //initalize timer

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
        grounded = onGround();

        // Reset gravity and movement if on floor
        if (grounded == 0)
        {
            body.gravityScale = 5;
            wallJumping = false;
            animator.SetBool("isJumping", false);
            coyoteCounter = coyoteTime; //reset coyote counter when on ground  
            coyoteSpent = false; //resets coyote jump tracker
        }
        else
        {
            coyoteCounter -= Time.deltaTime; //decrease coyote counter
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
        if (grounded == 0) //if on floor
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;

        // Apply acceleration to reach target speed
        if (!wallJumping || grounded == 0)
        {
            float movement = accelRate * Time.deltaTime;
            float newVelocityX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, movement);
            body.linearVelocity = new Vector2(newVelocityX, body.linearVelocity.y);
        }

        // Wall sliding logic
        if ( (grounded == 1 || grounded == 2) && !wallJumping) //if on a wall AND not on floor AND not wall jumping
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
            if (grounded == 0) //if on floor
            {
                // Jump height increases with movement speed
                float speedFactor = Mathf.Abs(body.linearVelocity.x) / maxSpeed;
                float adjustedJumpHeight = jumpHeight + speedFactor * jumpHeightBonus;
                Jump(new Vector2(body.linearVelocity.x, adjustedJumpHeight));
                coyoteSpent = true; //prevents a double jump when coyote time activates very close to ground after normal jump
            }
            else if (grounded == 1) //wall on left
            {
                // Wall jump to the right
                Vector2 wallJumpDirection = new Vector2(wallJumpX, wallJumpY);  
                Jump(wallJumpDirection, isWallJump: true);
            }
            else if (grounded == 2) //wall on right
            {
                // Wall jump to the left
                Vector2 wallJumpDirection = new Vector2(-wallJumpX, wallJumpY);  
                Jump(wallJumpDirection, isWallJump: true);
            }
            else if (grounded == -1 && coyoteCounter > 0 && !coyoteSpent) //coyote time
            {
                // Jump height increases with movement speed
                float speedFactor = Mathf.Abs(body.linearVelocity.x) / maxSpeed;
                float adjustedJumpHeight = jumpHeight + speedFactor * jumpHeightBonus;
                Jump(new Vector2(body.linearVelocity.x, adjustedJumpHeight));
                coyoteSpent = true;
            }
        }

        //smaller jump when space bar released
        if(Input.GetKeyUp(KeyCode.Space) && body.linearVelocity.y > 0) //if space is released and player is already jumping
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y / 2); //cuts vertical velocity in half
        }

        // Flip player direction based on movement
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // Update animations
        animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));

        // Play walk sound if moving on the ground
        if (grounded == 0 && Mathf.Abs(horizontalInput) > 0.01f && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();        
        }
    }

    //makes the player jump
    private void Jump(Vector2 jumpForce, bool isWallJump = false)
    {
        body.linearVelocity = jumpForce;
        animator.SetBool("isJumping", true);

        jumpAudioSource.PlayOneShot(jumpAudioSource.clip);

        if (isWallJump)
        {
            wallJumping = true;
            wallJumpCounter = wallJumpTime;
        }
    }

    //checks for contact with floor or walls
    //returns 0 for floor, 1 for left wall, 2 for right wall, and -1 if in the air
    private int onGround()
    {
        float rayDistance = 0.02f;

        RaycastHit2D hitDown = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.down, rayDistance, groundLayer
        );

        RaycastHit2D hitLeft = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.left, rayDistance, groundLayer
        );

        RaycastHit2D hitRight = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size, 0,
            Vector2.right, rayDistance, groundLayer
        );

        if (hitDown.collider != null)
        {
            return 0; // On floor
        }
        if (hitLeft.collider != null)
        {
            return 1; // On left wall
        }
        if (hitRight.collider != null)
        {
            return 2; // On right wall
        }

        return -1; // not grounded
    }

    //public getter function for if the player is grounded or not
    public bool getGrounded()
    {
        if(onGround() == 0){
            return true;
        }

        return false;
    }
}

