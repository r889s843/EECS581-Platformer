// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/01/2024
// Course: EECS 581
// Purpose: Level Manager

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public string nextLevelName; // Set the name of the next level in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Assuming the player has a tag "Player"
        if (other.CompareTag("Player"))
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        // Load the next level by name
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("Next level name is not set!");
        }
    }
}
