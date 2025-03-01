// Portal.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/28/2025
// Course: EECS 582
// Purpose: Adds portals to the game for the story mode

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Scene to Load")]
    [SerializeField] private string sceneName = "Level1";  // Name of the scene to load

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Key the player presses to trigger portal

    private bool isPlayerInRange = false; // Whether the player is inside the portal's trigger

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure to set your player's Tag to "Player" in the Inspector
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        // Check if the player is in range and presses E
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // Call your LevelManager's LoadScene
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("LevelManager. Instance not found. Make sure LevelManager is in the scene.");
            }
        }
    }
}
