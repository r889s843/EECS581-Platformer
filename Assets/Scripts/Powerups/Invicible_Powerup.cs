// Invicible_Powerup.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Defines the behavior and collision of the invicible power up 

using System.Collections;
using UnityEngine;

public class Invincible_Powerup : MonoBehaviour
{
    private BoxCollider2D powerupCollider; // This object's box collider
    private Renderer powerupRenderer; // This object's renderer

    private GameObject playerObject; // Reference to the player object
    private SpriteRenderer playerSpriteRenderer; // The player's sprite renderer
    private int playerLayer; // Layer for the player

    private bool isInvincible = false; // Track if the player is invincible
    private float invincibilityDuration = 5f; // Duration of the invincibility in seconds

    private void Start()
    {
        powerupCollider = GetComponent<BoxCollider2D>();
        powerupRenderer = GetComponent<Renderer>();

        playerObject = GameObject.Find("Player"); // Find the player object
        playerSpriteRenderer = playerObject.GetComponent<SpriteRenderer>(); // Get the player's sprite renderer

        // Get the layers for enemies and the player
        playerLayer = LayerMask.NameToLayer("Player"); // Get the player's layer
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Player") // If the player collides with the powerup
        {
            ActivatePowerup(); // activate it
        }
    }

    private void ActivatePowerup()
    {
        // Make the powerup invisible and inactive
        powerupCollider.enabled = false;
        powerupRenderer.enabled = false;

        // Start the invincibility process
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Temporarily change the player's layer to ignore collisions with enemies
        playerObject.layer = LayerMask.NameToLayer("Invincible");

        // Start color cycling effect
        float timer = 0f;
        while (timer < invincibilityDuration)
        {
            playerSpriteRenderer.color = Random.ColorHSV(); // Randomize the player's color
            timer += 0.1f; // Adjust for the speed of the color cycle
            yield return new WaitForSeconds(0.1f); // Wait for a short duration before changing the color again
        }

        // Reset everything after the duration
        isInvincible = false;
        playerObject.layer = playerLayer; // Restore the player's original layer
        playerSpriteRenderer.color = Color.white; // Reset the player's color
    }
}