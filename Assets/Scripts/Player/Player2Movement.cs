// Player2Movement.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/6/2024
// Course: EECS 581
// Purpose: Controls how player 2 interacts with movement and collidable objects.

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

// ABANDON ALL HOPE YE WHO ENTER HERE 2

// Ensures Rigidbody2D and BoxCollider2D are attached to the GameObject
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Player2Movement : MonoBehaviour
{
    // ---------------------------------------------------
    // 1) References & Components
    // ---------------------------------------------------
    private Rigidbody2D body;           // Reference to the player's physics body for movement
    private BoxCollider2D boxCollider;  // Collider used for ground and wall detection
    [SerializeField] private LayerMask groundLayer; // Layer mask to define what counts as ground/walls
    
    // Animator & Audio
    public Animator animator;           // Controls player animations (e.g., jumping, walking)
    public ParticleSystem smokeFX;
    private AudioSource walkAudioSource;// Plays walking sound when moving on ground
    private float walkSoundCooldown = 0f;
    public float walkSoundDelay = 0.25f; // control frequency of walk noise loop
    private AudioSource jumpAudioSource;// Plays jump sound when jumping
    public SpriteRenderer spriteRenderer;

    // Variables for AI vs. player control
    [HideInInspector] public bool agentActive = false; // If true, AI controls the player instead of input
    private float horizontalInput;      // Horizontal movement input (-1 to 1)
    private bool jumpInput;             // True when jump key is pressed
    
    // Struct to hold collision states
    public struct GroundState
    {
        public bool onFloor;    // True if touching the ground
        public bool onLeftWall; // True if touching a wall on the left
        public bool onRightWall;// True if touching a wall on the right
    }

    // Checks if the player is touching the ground or walls using boxcasts
    public GroundState CheckGround()
    {
        float rayDistance = 0.05f; // Distance to check for collisions
        GroundState state = new GroundState();

        // Cast rays downward, left, and right to detect ground/walls
        RaycastHit2D hitDown = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, rayDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.left, rayDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.right, rayDistance, groundLayer);

        state.onFloor = hitDown.collider != null;
        state.onLeftWall = hitLeft.collider != null;
        state.onRightWall = hitRight.collider != null;

        return state;
    }

    public bool isOnFloor; // Public flag to track if player is grounded, updated in Update

    // ---------------------------------------------------
    // 3) Horizontal Movement
    // ---------------------------------------------------
    [Header("=== Horizontal Movement ===")]
    [Range(0f, 30f)] public float maxSpeed = 12f;           // Max horizontal speed
    [Range(0f, 100f)] public float maxAcceleration = 80f;   // Acceleration rate on ground
    [Range(0f, 100f)] public float maxDecceleration = 80f;  // Deceleration rate on ground
    [Range(0f, 100f)] public float maxTurnSpeed = 95f;      // Speed to change direction on ground
    [Range(0f, 100f)] public float maxAirAcceleration = 40f;// Acceleration rate in air
    [Range(0f, 100f)] public float maxAirDeceleration = 40f;// Deceleration rate in air
    [Range(0f, 100f)] public float maxAirTurnSpeed = 80f;   // Speed to change direction in air
    [Tooltip("When false, the character instantly moves to max speed with no acceleration.")]
    public bool useAcceleration = true;                     // Toggle for smooth vs instant movement
    [Tooltip("Friction to apply if you want to reduce effective max speed.")]
    public float friction = 20f;                             // Slows player when not pressing input

    private Vector2 velocity;         // Current velocity of the player
    private Vector2 moveInput;        // Input direction for movement
    private bool pressingHorizontal;  // True if horizontal input is active

    // ---------------------------------------------------
    // 4) Jump & Gravity
    // ---------------------------------------------------
    [Header("=== Jumping ===")]
    [Range(0f, 10f)] public float jumpHeight = 1.5f;          // Height of a standard jump
    [Range(0.2f, 1.25f)] public float timeToJumpApex = 0.5f;// Time to reach jump peak (unused in current physics)
    [Range(0f, 10f)] public float upwardMovementMultiplier = 1.5f; // Gravity multiplier when rising
    [Range(1f, 10f)] public float downwardMovementMultiplier = 6f; // Gravity multiplier when falling
    [Range(1f, 10f)] public float jumpCutOff = 2f;          // Gravity multiplier when jump is released early
    [Tooltip("Max downward speed.")] public float speedLimit = 20f; // Caps falling speed
    public float coyoteTime = 0.15f;                        // Grace period to jump after leaving ground
    public float jumpBuffer = 0.25f;                        // Time window to buffer a jump input
    public bool canDoubleJump = true;                       // Enables double jumping
    public bool variableJumpHeight = true;                  // Allows variable jump height based on button hold
    [Range(1f, 3f)] public float apexControlMultiplier = 2f; // Boosts air control near jump apex
    [Range(0.5f, 2f)] public float doubleJumpMultiplier = 1f;  // Adjusts double jump strength

    // Internal jump state
    public bool isJumping;          // True when the player is in a jump state
    private bool pressingJump;      // True while jump key is held
    private float coyoteCounter;    // Timer for coyote time (ground only)
    private float jumpBufferCounter;// Timer for jump input buffering
    private bool canJumpAgain;      // True if double jump is available
    private bool desiredJump;       // True if jump is requested
    
    // Gravity
    private float defaultGravityScale = 1f; // Default gravity scale from Rigidbody2D
    private float gravMultiplier = 1f;      // Current gravity multiplier
    private float jumpSpeed;                // Calculated jump velocity

    // ---------------------------------------------------
    // 5) Dash & Wall-Jump
    // ---------------------------------------------------
    [Header("=== Dash Settings ===")]
    public bool canDash = true;         // Enables dashing
    public int dashAmount = 1;         // Max number of dashes
    public float dashSpeed = 30;        // Speed during dash
    public float dashAttackTime = 0.1f;// Duration of dash movement
    public float dashEndTime = 0.05f;    // Duration of dash slowdown
    public float dashEndSpeed = 10f;     // Speed at dash end
    public float dashRefillTime = 0.2f; // Time to refill a dash
    public float dashSleepTime = 0.02f; // Brief time freeze at dash start
    public float dashInputBufferTime = 0.2f; // Buffer time for dash input

    private bool isDashing;         // True during dash
    private bool isDashAttacking;   // True during dash's active phase
    private int dashesLeft;         // Remaining dashes
    private bool dashRefilling;     // True while dash is refilling
    private float lastPressedDashTime; // Timer for dash input buffer
    private bool isWallDashing;     // True if dashing off a wall
    private bool hasDashed;         // True if dash has been used since last reset

    [Header("=== Wall Jump Settings ===")]
    public Vector2 wallJumpForce = new Vector2(12f, 8f); // Force applied for wall jump (x, y)
    private bool isWallJumping;     // True during wall jump
    public float wallJumpControlDelay = 0.15f; // Delay before regaining air control
    private float wallJumpTime;     // Timer for wall jump control delay
    public float wallCoyoteTime = 0.15f; // Grace period to wall jump after leaving wall
    private float wallCoyoteCounter;// Timer for wall coyote time
    
    // ---------------------------------------------------
    // 6) Input Timers
    // ---------------------------------------------------
    private float lastPressedJumpTime; // Timer for jump input buffer

    // New Squash/Stretch and Tilt Settings
    [Header("=== Squash & Stretch Settings ===")]
    public float jumpStretchFactor = 1.2f;    // How tall the sprite gets during jump ascent
    public float landSquashFactor = 0.8f;     // How squashed the sprite gets on landing
    public float squashSpeed = 10f;           // Speed of squash/stretch transitions
    public float landSquashDuration = 0.1f;   // How long the landing squash lasts

    [Header("=== Tilt Settings ===")]
    public float maxTiltAngle = 15f;          // Max rotation angle at max speed
    public float tiltSpeed = 5f;              // Speed of tilt transition

    private Vector3 baseScale;             // Normal scale of the sprite
    private bool wasOnFloorLastFrame;         // Track previous ground state for landing detection
    private float landSquashTimer;            // Timer for landing squash duration

    // ---------------------------------------------------
    // Initialization
    // ---------------------------------------------------
    private void Awake()
    {
        // Get required components
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Assign audio sources (assumes two are attached in order: walk, jump)
        AudioSource[] audioSources = GetComponents<AudioSource>();
        walkAudioSource = audioSources[0];
        jumpAudioSource = audioSources[1];
        spriteRenderer = GetComponentInParent<SpriteRenderer>(); // Fetch from parent (player)

        // Initialize gravity and physics
        defaultGravityScale = body.gravityScale;
        UpdateJumpPhysics();
        dashesLeft = dashAmount;
        hasDashed = false;
        canJumpAgain = true;

        // Check for AI control
        if (GetComponent<PlatformerAgent>() != null)
            agentActive = true;

        // Store default scale (assuming initial scale is 1,1,1 or -1,1,1 for flip)
        baseScale = new Vector3(1f, 1f, 1f);
    }

    // Calculates initial jump speed based on jump height and gravity
    private void UpdateJumpPhysics()
    {
        float g = Physics2D.gravity.y * defaultGravityScale;
        jumpSpeed = Mathf.Sqrt(2f * jumpHeight * -g); // v = sqrt(2 * g * h)
    }

    // Allows external AI to set input
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
        // Handle player input if not AI-controlled
        if (!agentActive)
        {
            horizontalInput = Input.GetAxisRaw("HorizontalArrows");
            if (Input.GetKeyDown(KeyCode.UpArrow)) 
            {
                jumpInput = true;
            }
            pressingJump = Input.GetKey(KeyCode.UpArrow);
            // if (Input.GetKeyUp(KeyCode.Space)) 
            //     Debug.Log("Jump released, pressingJump = false");
            if (Input.GetKeyDown(KeyCode.RightControl)) lastPressedDashTime = dashInputBufferTime;
        }

        // Check collision states
        GroundState groundState = CheckGround();
        bool isOnFloor = groundState.onFloor;
        bool isOnLeftWall = groundState.onLeftWall;
        bool isOnRightWall = groundState.onRightWall;
        bool isGrounded = isOnFloor;

        this.isOnFloor = isOnFloor;

        // Set movement input
        moveInput.x = horizontalInput;
        moveInput.y = 0;

        // Process jump input
        if (jumpInput)
        {
            desiredJump = true;
            lastPressedJumpTime = jumpBuffer;
            jumpBufferCounter = 0f;
            if (isGrounded || coyoteCounter > 0f)
            {
                isJumping = true;
            }
            jumpInput = false;
        }

        // Decrease input buffer timers
        lastPressedDashTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        // Reset abilities based on ground/wall contact
        if (isGrounded)
        {
            coyoteCounter = coyoteTime; // Allow jump shortly after leaving ground
            canJumpAgain = true;
            hasDashed = false;
            if (!isJumping) animator.SetBool("isJumping", false); // Reset jump animation on landing
        }
        if (isOnLeftWall || isOnRightWall)
        {
            wallCoyoteCounter = wallCoyoteTime; // Allow wall jump shortly after leaving wall
            canJumpAgain = true;
            hasDashed = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;    // Decrease ground coyote timer
            wallCoyoteCounter -= Time.deltaTime;// Decrease wall coyote timer
        }

        // Handle jump buffer
        if (desiredJump)
        {
            jumpBufferCounter += Time.deltaTime;
            if (jumpBufferCounter > jumpBuffer)
            {
                desiredJump = false;
                jumpBufferCounter = 0f;
            }
        }

        // Attempt jump or dash based on state
        TryPerformJump(isGrounded, isOnLeftWall, isOnRightWall);
        TryPerformDash(isGrounded, isOnLeftWall, isOnRightWall);

        // Flip sprite based on movement direction, unless dashing or wall jumping
        if (!isWallDashing && (!isWallJumping || wallJumpTime > wallJumpControlDelay))
        {
            if (horizontalInput > 0.01f)
                baseScale.x = 1f; // Face right
            else if (horizontalInput < -0.01f)
                baseScale.x = -1f; // Face left
        }


        // Play walking sound when moving on ground
        if(isOnFloor && Mathf.Abs(horizontalInput) > 0.01f)
        {
            if (!walkAudioSource.isPlaying && walkSoundCooldown <= 0f)
            {
                walkAudioSource.Play();
                walkSoundCooldown = walkSoundDelay; // Reset cooldown
            }
        }

        // Reduce cooldown over time
        if (walkSoundCooldown > 0f)
        {
            walkSoundCooldown -= Time.deltaTime;
        }

        // Update animation speed parameter
        animator.SetFloat("Speed", Mathf.Abs(body.linearVelocity.x));

        // Squash, Stretch, and Tilt Logic
        UpdateSquashStretchAndTilt(isOnFloor);
        wasOnFloorLastFrame = isOnFloor; // Track for landing detection

        // jumpInput = false; // Reset jump input after processing
    }

    // Physics updates (runs at fixed time steps)
    private void FixedUpdate()
    {
        if (isDashAttacking) return; // Skip physics during dash active phase

        HandleHorizontalMovement(); // Update horizontal velocity

        // Limit falling speed
        if (body.linearVelocity.y < -speedLimit)
            body.linearVelocity = new Vector2(body.linearVelocity.x, -speedLimit);

        ApplyGravityScaling(); // Adjust gravity based on jump state

        // Manage wall jump control delay
        if (isWallJumping)
        {
            wallJumpTime += Time.fixedDeltaTime;
            if (wallJumpTime >= wallJumpControlDelay)
                isWallJumping = false; // Restore full air control
        }
    }

    // New Method for Squash, Stretch, and Tilt
    private void UpdateSquashStretchAndTilt(bool isOnFloor)
    {
        Vector3 targetScale = baseScale; // Start with base scale (includes flip)
        float targetTilt = 0f;

        // Jump Squeeze
        if (isJumping && body.linearVelocity.y > 0 && !isDashing)
        {
            targetScale.y = baseScale.y * jumpStretchFactor;
            targetScale.x = baseScale.x / jumpStretchFactor;
        }

        // Landing Plop
        if (isOnFloor && !wasOnFloorLastFrame)
        {
            landSquashTimer = landSquashDuration;
        }
        if (landSquashTimer > 0)
        {
            targetScale.y = baseScale.y * landSquashFactor;
            targetScale.x = baseScale.x / landSquashFactor;
            landSquashTimer -= Time.deltaTime;
        }

        // Tilt at Max Speed (disabled during dash or wall jump)
        if (!isDashing && (!isWallJumping || wallJumpTime > wallJumpControlDelay))
        {
            float speedRatio = Mathf.Abs(body.linearVelocity.x) / maxSpeed;
            targetTilt = Mathf.Lerp(0f, maxTiltAngle, speedRatio) * -Mathf.Sign(body.linearVelocity.x);
        }

        // Apply scale instantly for flipping, smoothly for squash/stretch
        Vector3 currentScale = transform.localScale;
        currentScale.x = baseScale.x; // Instant flip
        currentScale.y = Mathf.Lerp(currentScale.y, targetScale.y, Time.deltaTime * squashSpeed);
        transform.localScale = currentScale;

        // Smooth tilt
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    // ---------------------------------------------------
    // 8) Jump Logic
    // ---------------------------------------------------
    // Attempts to perform a jump based on current state
    private void TryPerformJump(bool isGrounded, bool isOnLeftWall, bool isOnRightWall)
    {
        if (desiredJump && lastPressedJumpTime > 0f)
        {
            isJumping = true;
            if (isGrounded){ // Ground jump
                isJumping = true;
                DoJump(false);
            }
            else if (coyoteCounter > 0f){ // Coyote time jump
                isJumping = true;
                DoJump(false);
            }
            else if (!isGrounded && (isOnLeftWall || isOnRightWall || wallCoyoteCounter > 0f)) // Wall jump
            {
                int dir = 0;
                if (isOnRightWall) dir = -1; // Push left
                else if (isOnLeftWall) dir = 1; // Push right
                else if (wallCoyoteCounter > 0f) // Wall coyote time
                    dir = (horizontalInput < -0.01f) ? 1 : (horizontalInput > 0.01f) ? -1 : (transform.localScale.x > 0 ? -1 : 1);
                if (dir != 0)
                    DoWallJump(dir);
            }
            else if (!isGrounded && canJumpAgain && canDoubleJump) // Double jump
            {
                isJumping = true;
                canJumpAgain = false;
                DoJump(true);
            }
        }
    }

    // Executes a jump (ground or double)
    private void DoJump(bool isDoubleJump)
    {
        // Reset jump-related flags and timers
        isJumping = true;
        desiredJump = false;
        jumpBufferCounter = 0f;
        lastPressedJumpTime = 0f;
        coyoteCounter = 0f;

        animator.SetBool("isJumping", true);
        jumpAudioSource.pitch = Random.Range(0.75f, 1.1f); // Randomize pitch slightly
        jumpAudioSource.PlayOneShot(jumpAudioSource.clip);
        smokeFX.Play();

        gravMultiplier = upwardMovementMultiplier; // Initialize gravity at jump start

        // Calculate jump speed
        float g = Physics2D.gravity.y * body.gravityScale;
        jumpSpeed = Mathf.Sqrt(2f * jumpHeight * -g);
        float effectiveJumpSpeed = isDoubleJump ? jumpSpeed * doubleJumpMultiplier : jumpSpeed;

        // Apply jump velocity (double jump adds to positive velocity only)
        if (isDoubleJump)
            body.linearVelocity = new Vector2(body.linearVelocity.x, Mathf.Max(body.linearVelocity.y, 0) + effectiveJumpSpeed);
        else
            body.linearVelocity = new Vector2(body.linearVelocity.x, effectiveJumpSpeed);
    }

    // Executes a wall jump
    private void DoWallJump(int dir)
    {
        desiredJump = false;
        jumpBufferCounter = 0f;
        lastPressedJumpTime = 0f;
        coyoteCounter = 0f;

        isWallJumping = true;
        isJumping = true;
        wallJumpTime = 0f;

        animator.SetBool("isJumping", true);
        jumpAudioSource.pitch = Random.Range(0.75f, 1.1f); // Randomize pitch slightly
        jumpAudioSource.PlayOneShot(jumpAudioSource.clip);
        smokeFX.Play();

        // Apply wall jump force in specified direction
        Vector2 jumpVelocity = new Vector2(wallJumpForce.x * dir, wallJumpForce.y);
        body.linearVelocity = jumpVelocity;

        // Flip sprite by setting baseScale.x (1 for right, -1 for left)
        baseScale.x = dir; // dir is 1 (right) or -1 (left)
    }

    // ---------------------------------------------------
    // 9) Dash Logic
    // ---------------------------------------------------
    // Attempts to perform a dash
    private void TryPerformDash(bool isGrounded, bool isOnLeftWall, bool isOnRightWall)
    {
        if (!isDashing && dashesLeft < dashAmount && isGrounded && !dashRefilling)
            StartCoroutine(RefillDash(1)); // Refill dash on ground

        if (CanDash() && lastPressedDashTime > 0f && !hasDashed)
        {
            StartCoroutine(DoTimeFreeze(dashSleepTime)); // Brief time freeze effect

            Vector2 dashDir;
            if (isOnLeftWall)
            {
                dashDir = Vector2.right;
                baseScale.x = 1f; // Face right (away from left wall)
                isWallDashing = true;
            }
            else if (isOnRightWall)
            {
                dashDir = Vector2.left;
                baseScale.x = -1f; // Face left (away from right wall)
                isWallDashing = true;
            }
            else if (moveInput != Vector2.zero)
                dashDir = moveInput.normalized;
            else
                dashDir = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;

            isDashing = true;
            isJumping = false;
            isWallJumping = false;

            StartCoroutine(PerformDash(dashDir));
        }
    }

    // Checks if dashing is allowed
    private bool CanDash()
    {
        return (dashesLeft > 0 && !isDashing && canDash);
    }

    // Coroutine to handle dash mechanics
    private IEnumerator PerformDash(Vector2 dir)
    {
        lastPressedDashTime = 0f;
        dashesLeft--;
        isDashAttacking = true;
        hasDashed = true;

        float startTime = Time.time;
        SetGravityScale(0f); // Disable gravity during dash
        float initialXVelocity = body.linearVelocity.x;

        // Timer for spawning after-images (adjust delay as needed)
        float afterImageSpawnDelay = 0.05f;
        float nextAfterImageTime = Time.time;
        
        while (Time.time - startTime <= dashAttackTime)
        {
            body.linearVelocity = dir * dashSpeed;

             // Spawn after-image if the delay has passed
            if (Time.time >= nextAfterImageTime)
            {
                PlayerAfterPool.Instance.GetFromPool();
                nextAfterImageTime = Time.time + afterImageSpawnDelay;
            }

            yield return null;
        }

        isDashAttacking = false;
        SetGravityScale(defaultGravityScale); // Restore gravity

        startTime = Time.time;
        Vector2 endVelocity = dir * dashEndSpeed;
        endVelocity.x = Mathf.Lerp(initialXVelocity, endVelocity.x, 0.5f);
        body.linearVelocity = endVelocity;
        while (Time.time - startTime <= dashEndTime)
        {
            body.linearVelocity = Vector2.Lerp(body.linearVelocity, Vector2.zero, Time.deltaTime / dashEndTime);
            yield return null;
        }
        isDashing = false;
        isWallDashing = false;
    }

    // Refills dashes after a delay
    private IEnumerator RefillDash(int amount)
    {
        dashRefilling = true;
        yield return new WaitForSeconds(dashRefillTime);
        dashRefilling = false;
        dashesLeft = Mathf.Min(dashAmount, dashesLeft + amount);
    }

    // Brief time freeze effect for dash
    private IEnumerator DoTimeFreeze(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // ---------------------------------------------------
    // 10) Horizontal Movement
    // ---------------------------------------------------
    // Updates horizontal velocity
    private void HandleHorizontalMovement()
    {
        velocity = body.linearVelocity;
        float targetSpeed = moveInput.x * maxSpeed;
        pressingHorizontal = (Mathf.Abs(moveInput.x) > 0.01f);

        if (!useAcceleration)
        {
            if (isOnFloor)
                velocity.x = targetSpeed;
            else
                RunWithAcceleration(targetSpeed);
        }
        else
        {
            if (!isWallJumping || wallJumpTime > wallJumpControlDelay)
                RunWithAcceleration(targetSpeed); // Apply acceleration unless in wall jump delay
        }

        if (!pressingHorizontal && isOnFloor)
            velocity.x = Mathf.MoveTowards(velocity.x, 0, friction * Time.fixedDeltaTime); // Apply friction

        body.linearVelocity = velocity;
    }

    // Applies smooth acceleration to horizontal movement
    private void RunWithAcceleration(float targetSpeed)
    {
        float accel = isOnFloor ? maxAcceleration : maxAirAcceleration * 1.2f;
        float decel = isOnFloor ? maxDecceleration : maxAirDeceleration;
        float turn = isOnFloor ? maxTurnSpeed * 1.5f : maxAirTurnSpeed * 1.5f;

        if (isWallJumping)
        {
            accel *= 0.5f; // Reduced control during wall jump
            turn *= 0.5f;
        }

        float vY = body.linearVelocity.y;
        bool atApex = Mathf.Abs(vY) < 1f && !isOnFloor && isJumping;
        if (atApex && !isWallJumping)
        {
            accel *= apexControlMultiplier; // Enhanced control at jump apex
            turn *= apexControlMultiplier;
        }

        float speedDiff = targetSpeed - velocity.x;
        float maxSpeedChange = 0f;

        if (pressingHorizontal)
        {
            if (speedDiff > 0 && velocity.x < 0 || speedDiff < 0 && velocity.x > 0)
                maxSpeedChange = turn * Time.fixedDeltaTime; // Turning speed
            else
                maxSpeedChange = accel * Time.fixedDeltaTime; // Acceleration
        }
        else
            maxSpeedChange = decel * Time.fixedDeltaTime; // Deceleration

        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, maxSpeedChange);
    }

    // ---------------------------------------------------
    // 11) Gravity Scaling
    // ---------------------------------------------------
    // Adjusts gravity based on jump state
    private void ApplyGravityScaling()
    {
        if (isDashAttacking) return;

        Vector2 v = body.linearVelocity;

        if (v.y > 0) // Only apply jump gravity logic while in air
        {
            if (variableJumpHeight && pressingJump) // Check if the player is holding space and variable height
                gravMultiplier = upwardMovementMultiplier;
            else if (variableJumpHeight && !pressingJump)
                gravMultiplier = Mathf.Lerp(gravMultiplier, jumpCutOff, Time.fixedDeltaTime * 20f); // Shorten jump when player lets go of space
            else if (isJumping)
                gravMultiplier = upwardMovementMultiplier;
            else
                gravMultiplier = downwardMovementMultiplier;
        }
        else
        {
            isJumping = false; // Reset on ground, regardless of velocity
            gravMultiplier = defaultGravityScale;
        }

        SetGravityScale(gravMultiplier);
    }

    // Sets the gravity scale on the Rigidbody2D
    private void SetGravityScale(float scale)
    {
        body.gravityScale = scale;
    }
}