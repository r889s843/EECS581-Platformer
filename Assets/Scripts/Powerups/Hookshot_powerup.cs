// Hookshot_Powerup.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/23/2025
// Course: EECS 582
// Purpose: Defines the teleport/hookshot ability

using UnityEngine;
using System.Collections;

public class Teleport_Powerup : MonoBehaviour
{
    [Header("Teleport Settings")]
    public float teleportRange = 10f;   // How far the player can teleport
    public LayerMask obstacleLayers;   // Layers to test for collision (walls, ground, etc.)

    private PlayerMovement playerMovement;
    private Rigidbody2D body;
    public SpriteRenderer spriteRenderer; // To hide the player during teleport

    // Assign these in the Inspector
    public ParticleSystem teleportOutFX;
    public ParticleSystem teleportInFX;

    // Adjust this delay to match your outward effect duration
    public float teleportDelay = 0.2f;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
        // spriteRenderer = GetComponent<SpriteRenderer>(); // Get the renderer to hide/show player
    }

    public void ActivatePowerup()
    {
        // Determine teleport target
        float direction = (transform.parent.localScale.x >= 0) ? 1f : -1f;
        Vector2 origin = transform.parent.position;
        Vector2 rayDir = new Vector2(direction, 0f);

        RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, teleportRange, obstacleLayers);
        Vector2 targetPosition;
        if (hit.collider != null)
        {
            targetPosition = hit.point - (rayDir * 0.5f);
        }
        else
        {
            targetPosition = origin + rayDir * teleportRange;
        }

        // Start the teleport sequence
        StartCoroutine(TeleportSequence(targetPosition));
    }

    private IEnumerator TeleportSequence(Vector2 targetPosition)
    {
        // Play the outward particle effect at the player's current position
        teleportOutFX.transform.position = transform.parent.position;
        teleportOutFX.Play();

        // Disable player visibility and optionally movement/physics
        spriteRenderer.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (body != null) body.simulated = false; // Prevents physics interference

        // Wait for half the total time (0.15s for first effect and disappearance)
        yield return new WaitForSeconds(0.15f);

        // Teleport the player instantly
        transform.parent.position = targetPosition;

        // Play the inward particle effect at the new position
        teleportInFX.transform.position = targetPosition;
        teleportInFX.Play();

        // Re-enable player visibility and movement/physics
        spriteRenderer.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (body != null) body.simulated = true;

        // Wait for the remaining half (0.15s for second effect)
        yield return new WaitForSeconds(0.15f);
    }
}