// PlayerDeath.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Manage Player death in human-only mode.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeath : MonoBehaviour
{
    [Header("References")]
    private Material originalMaterial;
    public Dissolve dissolveEffect;
    public LivesUI livesUI;
    public PlayerMovement playerMovement;  // Reference to our PlayerMovement script
    public Transform respawnPoint;         // Assign a spawn location in the Inspector (optional)

    [Header("Dissolve Settings")]
    public float dissolveSpeed = 2f;

    private Vector2 startPosition;
    private bool isDead = false;

    private void Start()
    {
        // If you prefer the original position as the respawn, store it
        startPosition = transform.position;

        originalMaterial = GetComponent<Renderer>().material;

        // If we didn’t assign them in the Inspector:
        if (dissolveEffect == null)
            dissolveEffect = GetComponent<Dissolve>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        
        if (livesUI == null)
            livesUI = FindObjectOfType<LivesUI>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // For any lethal collision tags:
        if (collision.CompareTag("DeathZone") || collision.CompareTag("Enemy") ||
            collision.CompareTag("Hazard")   || collision.CompareTag("Projectile"))
        {
            // Decrement the appropriate player's life
            if (CompareTag("Player") && !isDead)
                livesUI.LoseLifeP1();
            else if (CompareTag("Player2") && !isDead)
                livesUI.LoseLifeP2();

            isDead = true;

            // Disable movement so player can’t move while dissolving
            if (playerMovement != null)
                playerMovement.enabled = false;

            // Freeze the player in place by making Rigidbody2D kinematic and zeroing velocity
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Stop all movement
                rb.angularVelocity = 0f;         // Stop rotation
                rb.isKinematic = true;           // Ignore physics forces
            }

            // Reset dissolveAmount in a fresh material instance
            Renderer renderer = GetComponent<Renderer>();
            Material newMat = new Material(dissolveEffect.material);
            newMat.SetFloat("_DissolveAmount", 0f);
            renderer.material = newMat;
            dissolveEffect.material = newMat;

            // Start the dissolve effect
            dissolveEffect.StartDissolve(dissolveSpeed);

            // Respawn after the dissolve completes
            StartCoroutine(HandleDeathAndRespawn());
        }
    }

    private IEnumerator HandleDeathAndRespawn()
    {
        // Wait for dissolve to complete
        // (1.5f / dissolveSpeed) is what you’re using now, adjust as you wish
        yield return new WaitForSeconds(1.6f / dissolveSpeed);

        // Actually respawn: Move to spawn point (or back to startPosition)
        if (respawnPoint != null)
            transform.position = respawnPoint.position;
        else
            transform.position = startPosition;

        // Re-enable physics and reset velocity
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = false; // Restore physics
        }

        // Now that we’re “alive” again, re-enable movement
        if (playerMovement != null)
            playerMovement.enabled = true;
            isDead = false;

        dissolveEffect.StopDissolve(dissolveSpeed);

        yield return new WaitForSeconds(1.5f / dissolveSpeed);

        GetComponent<Renderer>().material = originalMaterial; // Restore original material
    }
}
