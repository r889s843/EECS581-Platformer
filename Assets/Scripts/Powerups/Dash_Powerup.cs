// Dash_Powerup.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/23/2025
// Course: EECS 582
// Purpose: Defines the dash ability

using UnityEngine;
using System.Collections;

public class Dash_Powerup : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 30f;     // Horizontal speed during dash
    public float dashDuration = 0.2f; // How long the dash lasts
    
    private Rigidbody2D body;
    private PlayerMovement playerMovement;
    private bool isDashing = false;

    private void Start()
    {
        // Assume this script is on the same GameObject as PlayerMovement
        playerMovement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
    }

    public void ActivatePowerup()
    {
        if (!isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        // Temporarily disable normal movement (so playerMovement doesn't fight the dash velocity)
        // Or at least store old values so you can restore them after dash
        bool oldAgentActive = playerMovement.agentActive;  // if AI is controlling, you may want to freeze it
        playerMovement.enabled = false;

        // Determine dash direction based on facing
        float dashDirection = (transform.localScale.x >= 0) ? 1f : -1f;

        // Set velocity to dash
        body.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        // Restore normal movement
        playerMovement.enabled = true;
        // If you had an AI agent controlling movement, you can re-enable it if needed
        playerMovement.agentActive = oldAgentActive;

        // Optionally reset velocity to zero after dash
        body.linearVelocity = new Vector2(0f, body.linearVelocity.y);

        isDashing = false;
    }
}