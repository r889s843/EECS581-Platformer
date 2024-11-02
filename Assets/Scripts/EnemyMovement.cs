// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 1/01/2024
// Course: EECS 581
// Purpose: Enemy Movements


using UnityEngine;
using System.Collections;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementMode
    {
        None,
        JumpInPlace,
        WalkLeftRight
    }

    public MovementMode movementMode = MovementMode.None;
    public LayerMask platformLayer; // The ground the AI can walk on.
    public float jumpHeight = 10f; // How high the AI can jump.
    public float walkSpeed = 5f; // Speed for walking left and right.
    public float actionInterval = 1.5f; // Time in seconds between actions (jumping or direction change for walking).

    private Rigidbody2D body; // Reference to the enemy's Rigidbody2D component.
    private CircleCollider2D circleCollider; // Reference to the enemy's main CircleCollider2D component.
    private BoxCollider2D groundCollider; // Additional BoxCollider2D for ground detection.
    private float nextActionTime; // Time at which the next action can occur.
    private bool movingRight = true; // Determines the current walking direction for left-right movement.

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        groundCollider = GetComponent<BoxCollider2D>();

        // Ensure gravity is enabled so the enemy can fall to the ground
        body.gravityScale = 1f;
    }

    private void FixedUpdate()
    {
        switch (movementMode)
        {
            case MovementMode.None:
                HandleNone();
                break;

            case MovementMode.JumpInPlace:
                HandleJumpingInPlace();
                break;

            case MovementMode.WalkLeftRight:
                HandleWalkingLeftRight();
                break;
        }
    }

    private bool IsGrounded()
    {
        // Use BoxCast for ground detection similar to the player's script
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            groundCollider.bounds.center, 
            groundCollider.bounds.size, 
            0, 
            Vector2.down, 
            0.1f, 
            platformLayer
        );

        return raycastHit.collider != null;
    }

    private void HandleNone()
    {
        if (IsGrounded())
        {
            // Stop horizontal movement and keep the vertical velocity unchanged to ensure the enemy stays on the ground
            body.linearVelocity = new Vector2(0, body.linearVelocity.y);
        }
        else
        {
            // Allow gravity to pull the enemy down if not grounded
            body.gravityScale = 1f;
        }
    }

    private void HandleJumpingInPlace()
    {
        if (Time.time >= nextActionTime && IsGrounded())
        {
            Jump();
            nextActionTime = Time.time + actionInterval;
        }
    }

    private void HandleWalkingLeftRight()
    {
        if (Time.time >= nextActionTime)
        {
            movingRight = !movingRight;
            nextActionTime = Time.time + actionInterval;
        }

        float horizontalVelocity = movingRight ? walkSpeed : -walkSpeed;
        body.linearVelocity = new Vector2(horizontalVelocity, body.linearVelocity.y);
    }

    private void Jump()
    {
        body.linearVelocity = new Vector2(body.linearVelocity.x, jumpHeight); // Apply upward velocity to jump
    }
}
