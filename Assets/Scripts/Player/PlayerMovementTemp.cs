using UnityEngine;
using System.Collections;

// ---------------------------------------------------
// MERGED SCRIPT EXAMPLE
// ---------------------------------------------------
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class MergedPlayerMovement : MonoBehaviour
{
    // ---------------------------------------------------
    // 1) References & Components
    // ---------------------------------------------------
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask groundLayer;
    
    // Animator & Audio
    public Animator animator;
    private AudioSource walkAudioSource;
    private AudioSource jumpAudioSource;

    // Keeping these from your original script for controlling AI vs. Player input:
    [HideInInspector] public bool agentActive = false;
    private float horizontalInput;
    private bool jumpInput;

    // ---------------------------------------------------
    // 2) “isGrounded/onWall” from First Script
    //    (Returning 0=Floor, 1=LeftWall, 2=RightWall, -1=None)
    // ---------------------------------------------------
    private int groundedState; // 0=floor,1=left wall,2=right wall,-1=none

    // We'll treat “isGrounded” as “(groundedState == 0)”.
    // We'll treat “onWall” as “(groundedState == 1 or 2)”.
    
    // The original onGround() method:
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
            Debug.Log("On Ground");
            return 0; // On floor
        }
        if (hitLeft.collider != null)
        {
            Debug.Log("On Left Wall");
            return 1; // On left wall
        }
        if (hitRight.collider != null)
        {
            Debug.Log("On Right Wall");
            return 2; // On right wall
        }

        // if (hitDown.collider != null) return 0; // on floor
        // if (hitLeft.collider != null) return 1; // on left wall
        // if (hitRight.collider != null) return 2; // on right wall
        Debug.Log("In Air");
        return -1; // not grounded
    }

    // ---------------------------------------------------
    // 3) Horizontal Movement (from Advanced Script)
    //    We’ll unify it with the “isGrounded” check above.
    // ---------------------------------------------------
    [Header("=== Horizontal Movement ===")]
    [Range(0f, 20f)] public float maxSpeed = 10f;
    [Range(0f, 100f)] public float maxAcceleration = 52f;
    [Range(0f, 100f)] public float maxDecceleration = 52f;
    [Range(0f, 100f)] public float maxTurnSpeed = 80f;
    [Range(0f, 100f)] public float maxAirAcceleration = 25f;
    [Range(0f, 100f)] public float maxAirDeceleration = 25f;
    [Range(0f, 100f)] public float maxAirTurnSpeed = 60f;
    [Tooltip("When false, the character instantly moves to max speed with no acceleration.")]
    public bool useAcceleration = true;
    [Tooltip("Friction to apply if you want to reduce effective max speed.")]
    public float friction = 0f;

    private Vector2 velocity;
    private Vector2 moveInput;
    private bool pressingHorizontal;

    // ---------------------------------------------------
    // 4) Jump & Gravity (from Advanced Script)
    //    We keep your original “jump” animations & sfx.
    //    We unify coyote time by bridging with onGround().
    // ---------------------------------------------------
    [Header("=== Jumping ===")]
    [Range(2f, 10f)] public float jumpHeight = 7.3f;
    [Range(0.2f, 1.25f)] public float timeToJumpApex = 0.5f;
    [Range(0f, 5f)] public float upwardMovementMultiplier = 1f;
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6f;
    [Range(1f, 10f)] public float jumpCutOff = 3f;
    [Tooltip("Max downward speed.")] public float speedLimit = 15f;
    [Range(0f, 0.3f)] public float coyoteTime = 0.15f;
    [Range(0f, 0.3f)] public float jumpBuffer = 0.15f;
    [Range(0, 2)] public int maxAirJumps = 1; 
    public bool variableJumpHeight = true;

    // Internal jump state
    public bool isJumping; 
    private bool pressingJump;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool canJumpAgain;
    private bool desiredJump;
    
    // Gravity
    private float defaultGravityScale = 1f;
    private float gravMultiplier = 1f;
    private float jumpSpeed;

    // ---------------------------------------------------
    // 5) Dash & Wall-Jump (from Advanced Script)
    //    We can integrate your “on wall = 1 or 2” checks.
    // ---------------------------------------------------
    [Header("=== Dash Settings ===")]
    public int dashAmount = 1;
    public float dashSpeed = 15f;
    public float dashAttackTime = 0.15f;
    public float dashEndTime = 0.1f;
    public float dashEndSpeed = 8f;
    public float dashRefillTime = 0.5f;
    public float dashSleepTime = 0.05f;
    public float dashInputBufferTime = 0.2f;

    private bool isDashing;
    private bool isDashAttacking;
    private int dashesLeft;
    private bool dashRefilling;
    private float lastPressedDashTime;

    [Header("=== Wall Jump Settings ===")]
    public Vector2 wallJumpForce = new Vector2(8f, 12f);
    public float wallJumpTime = 0.2f;

    private bool isWallJumping;
    private float wallJumpStartTime;
    private int lastWallJumpDir; // +1 or -1

    // For the advanced script’s “wall check” placeholders:
    private float lastOnWallTime;
    private float lastOnWallLeftTime;
    private float lastOnWallRightTime;
    
    // We’ll flip the sprite using the first script’s approach 
    // (checking horizontalInput > 0 or < 0).
    
    // ---------------------------------------------------
    // 6) Input Timers
    // ---------------------------------------------------
    private float lastPressedJumpTime;

    // ---------------------------------------------------
    // Initialization
    // ---------------------------------------------------
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // From first script: get audio sources
        AudioSource[] audioSources = GetComponents<AudioSource>();
        walkAudioSource = audioSources[0];
        jumpAudioSource = audioSources[1];

        defaultGravityScale = body.gravityScale;
        dashesLeft = dashAmount;

        // If there's a PlatformerAgent attached, assume AI control
        if (GetComponent<PlatformerAgent>() != null)
        {
            agentActive = true;
        }
    }

    // If AI is controlling the player, we set these externally:
    public void SetInput(float horizontal, bool jump)
    {
        horizontalInput = horizontal;
        jumpInput = jump;
    }

    // ---------------------------------------------------
    // 7) Update Loop
    // ---------------------------------------------------
    private void Update()
    {
        // (1) Get Input from either AI or Player
        if (!agentActive)
        {
            // Use advanced script’s style: raw horizontal
            horizontalInput = Input.GetAxisRaw("Horizontal");
            // But also track jump “key down”:
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpInput = true;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                jumpInput = false;
            }
            // Dashing input
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.X))
            {
                lastPressedDashTime = dashInputBufferTime;
            }
        }
        else
        {
            // AI input was set via SetInput(). Also handle dash if needed, etc.
        }

        // (2) Ground & Wall Checking: use your boxcast method
        groundedState = onGround();
        bool isOnFloor = (groundedState == 0);
        bool isOnLeftWall = (groundedState == 1);
        bool isOnRightWall = (groundedState == 2);
        bool isGrounded = isOnFloor;  // For the advanced coyote-time logic

        // (3) Movement Input in vector form for dash logic
        moveInput.x = horizontalInput;
        moveInput.y = 0; // Not really needed except for vertical inputs if you want them

        // (4) Jump Buffer & Coyote Time
        if (jumpInput)
        {
            desiredJump = true;
            pressingJump = true;
            lastPressedJumpTime = jumpBuffer;
            jumpBufferCounter = 0f;
        }
        else
        {
            // If the user physically released jump
            pressingJump = false;
        }

        // Decrement timers
        lastPressedDashTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        // If grounded, reset coyote time & air jumps
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            canJumpAgain = true;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Keep a jump buffer if jump was pressed slightly before landing
        if (desiredJump)
        {
            jumpBufferCounter += Time.deltaTime;
            if (jumpBufferCounter > jumpBuffer)
            {
                desiredJump = false;
                jumpBufferCounter = 0f;
            }
        }

        // (5) Check if we are on walls for potential wall jump
        // We'll keep a simple “on wall” time. 
        if (isOnLeftWall)  lastOnWallLeftTime = 0.2f;
        else               lastOnWallLeftTime -= Time.deltaTime;
        if (isOnRightWall) lastOnWallRightTime = 0.2f;
        else               lastOnWallRightTime -= Time.deltaTime;
        
        lastOnWallTime = Mathf.Max(lastOnWallLeftTime, lastOnWallRightTime);

        // (6) Attempt Jump
        TryPerformJump(isGrounded);

        // (7) Attempt Dash
        TryPerformDash(isGrounded);

        // (8) Flip sprite & handle walking animations from first script
        if (horizontalInput > 0.01f)
        {
            transform.localScale = Vector3.one;
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // (9) Handle “walking” sound if on the ground and moving
        if (isOnFloor && Mathf.Abs(horizontalInput) > 0.01f && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }

        // (10) Animation speed param
        animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));

        // We’ll let “isJumping” animate in Jump logic. 
        // If on floor, we can safely say not jumping
        if (isOnFloor && body.linearVelocity.y <= 0.01f)
        {
            animator.SetBool("isJumping", false);
        }
    }

    private void FixedUpdate()
    {
        // Horizontal movement
        HandleHorizontalMovement();

        // If dash-attacking, the dash coroutine sets velocity
        // so we let that override. If not dashing, we do normal movement.

        // Cap vertical speed
        if (body.linearVelocity.y < -speedLimit)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, -speedLimit);
        }

        // Apply variable gravity scale
        ApplyGravityScaling();
    }

    // ---------------------------------------------------
    // 8) Jump Logic
    // ---------------------------------------------------
    private void TryPerformJump(bool isGrounded)
    {
        if (desiredJump && lastPressedJumpTime > 0f)
        {
            if (isGrounded)       // always allow jump if physically on ground
            {
                DoJump();
            }
            else if (coyoteCounter > 0f)
            {
                DoJump();
            }
            else if (!isGrounded && canJumpAgain)
            {
                // double jump
                DoJump();
            }
            else if (lastOnWallTime > 0 && !isGrounded)
            {
                if (lastOnWallRightTime > 0) lastWallJumpDir = -1;
                else if (lastOnWallLeftTime > 0) lastWallJumpDir = 1;
                DoWallJump(lastWallJumpDir);
            }
        }

    }

    private void DoJump()
    {
        desiredJump = false;
        jumpBufferCounter = 0f;
        lastPressedJumpTime = 0f;
        coyoteCounter = 0f;

        isJumping = true;
        animator.SetBool("isJumping", true);

        // Play your jump sound
        jumpAudioSource.PlayOneShot(jumpAudioSource.clip);

        // Calculate jump velocity: v = sqrt(2*g*jumpHeight)
        float g = Physics2D.gravity.y * body.gravityScale;
        jumpSpeed = Mathf.Sqrt(2f * jumpHeight * -g);

        // “Reset” vertical velocity if needed
        float vy = body.linearVelocity.y;
        if (vy < 0) jumpSpeed -= vy;
        else if (vy > 0) jumpSpeed = Mathf.Max(jumpSpeed - vy, 0f);

        body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y + jumpSpeed);
    }

    private void DoWallJump(int dir)
    {
        desiredJump = false;
        jumpBufferCounter = 0f;
        lastPressedJumpTime = 0f;
        coyoteCounter = 0f;

        isWallJumping = true;
        isJumping = true;
        wallJumpStartTime = Time.time;

        animator.SetBool("isJumping", true);
        jumpAudioSource.PlayOneShot(jumpAudioSource.clip);

        // Force for wall jump
        Vector2 currentVel = body.linearVelocity;
        Vector2 force = wallJumpForce;
        force.x *= dir;

        // Adjust if existing velocity is in the opposite direction
        if (Mathf.Sign(currentVel.x) != Mathf.Sign(force.x))
        {
            force.x -= currentVel.x;
        }
        if (currentVel.y < 0)
        {
            force.y -= currentVel.y;
        }
        body.AddForce(force, ForceMode2D.Impulse);
    }

    // ---------------------------------------------------
    // 9) Dash Logic
    // ---------------------------------------------------
    private void TryPerformDash(bool isGrounded)
    {
        // If we’re on the ground and out of dashes, refill eventually
        if (!isDashing && dashesLeft < dashAmount && isGrounded && !dashRefilling)
        {
            StartCoroutine(RefillDash(1));
        }

        // Actually dash if conditions allow
        if (CanDash() && lastPressedDashTime > 0f)
        {
            StartCoroutine(DoTimeFreeze(dashSleepTime));

            Vector2 dashDir;
            // If we have input, use it; otherwise dash in facing direction
            if (moveInput != Vector2.zero)
                dashDir = moveInput.normalized;
            else
                dashDir = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;

            isDashing = true;
            isJumping = false;
            isWallJumping = false;

            StartCoroutine(PerformDash(dashDir));
        }
    }

    private bool CanDash()
    {
        return (dashesLeft > 0 && !isDashing);
    }

    private IEnumerator PerformDash(Vector2 dir)
    {
        lastPressedDashTime = 0f;
        dashesLeft--;
        isDashAttacking = true;

        float startTime = Time.time;
        SetGravityScale(0f);
        while (Time.time - startTime <= dashAttackTime)
        {
            body.linearVelocity = dir * dashSpeed;
            yield return null;
        }

        isDashAttacking = false;
        SetGravityScale(defaultGravityScale);

        startTime = Time.time;
        body.linearVelocity = dir * dashEndSpeed;
        while (Time.time - startTime <= dashEndTime)
        {
            yield return null;
        }
        isDashing = false;
    }

    private IEnumerator RefillDash(int amount)
    {
        dashRefilling = true;
        yield return new WaitForSeconds(dashRefillTime);
        dashRefilling = false;
        dashesLeft = Mathf.Min(dashAmount, dashesLeft + amount);
    }

    private IEnumerator DoTimeFreeze(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // ---------------------------------------------------
    // 10) Horizontal Movement (acceleration/friction from second script)
    // ---------------------------------------------------
    private void HandleHorizontalMovement()
    {
        velocity = body.linearVelocity;

        float targetSpeed = moveInput.x * Mathf.Max(maxSpeed - friction, 0f);
        pressingHorizontal = (Mathf.Abs(moveInput.x) > 0.01f);

        if (!useAcceleration)
        {
            // Instantly set speed if on ground, else accelerate in air
            if (groundedState == 0)
            {
                velocity.x = targetSpeed;
            }
            else
            {
                RunWithAcceleration(targetSpeed);
            }
        }
        else
        {
            RunWithAcceleration(targetSpeed);
        }

        // Assign new velocity
        body.linearVelocity = velocity;
    }

    private void RunWithAcceleration(float targetSpeed)
    {
        // If on the floor (0), else air
        bool isOnFloor = (groundedState == 0);

        float accel = isOnFloor ? maxAcceleration : maxAirAcceleration;
        float decel = isOnFloor ? maxDecceleration : maxAirDeceleration;
        float turn = isOnFloor ? maxTurnSpeed : maxAirTurnSpeed;

        float speedDiff = targetSpeed - velocity.x;
        float maxSpeedChange = 0f;

        if (pressingHorizontal)
        {
            // If trying to reverse direction
            if (Mathf.Sign(targetSpeed) != Mathf.Sign(velocity.x) && Mathf.Abs(velocity.x) > 0.1f)
                maxSpeedChange = turn * Time.deltaTime;
            else
                maxSpeedChange = accel * Time.deltaTime;
        }
        else
        {
            maxSpeedChange = decel * Time.deltaTime;
        }

        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, maxSpeedChange);
    }

    // ---------------------------------------------------
    // 11) Gravity Scaling (for variable jump/fall)
    // ---------------------------------------------------
    private void ApplyGravityScaling()
    {
        if (isDashAttacking) return;

        Vector2 v = body.linearVelocity;

        // Going up
        if (v.y > 0.01f && groundedState != 0)
        {
            // Pressing jump => normal upward multiplier
            if (variableJumpHeight && pressingJump && isJumping)
                gravMultiplier = upwardMovementMultiplier;
            // Released jump => cut jump short
            else if (variableJumpHeight && !pressingJump && isJumping)
                gravMultiplier = jumpCutOff;
            else
                gravMultiplier = upwardMovementMultiplier;
        }
        // Going down
        else if (v.y < -0.01f && groundedState != 0)
        {
            gravMultiplier = downwardMovementMultiplier;
        }
        else
        {
            // On ground or nearly idle vertical
            if (groundedState == 0)
            {
                isJumping = false; 
            }
            gravMultiplier = defaultGravityScale;
        }

        SetGravityScale(gravMultiplier);
    }

    private void SetGravityScale(float scale)
    {
        body.gravityScale = scale;
    }
}
