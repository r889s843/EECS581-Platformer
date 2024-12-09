// LevelTimer.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/05/2024
// Course: EECS 581
// Purpose: Tracks timer for leaderboard updates

using UnityEngine;
using UnityEngine.UI; // or using TMPro if using TextMeshPro

public class LevelTimer : MonoBehaviour
{
    // TextMeshProUGUI field to display the timer
    public TMPro.TextMeshProUGUI timerText; // Assign this in the Inspector

    private float elapsedTime = 0f; // Elapsed time in seconds
    private bool timerRunning = false; // Indicates whether the timer is active

    void Start()
    {
        // Start the timer when the level begins
        timerRunning = true; // Activate the timer
    }

    void Update()
    {
        if (timerRunning) // Check if the timer is running
        {
            elapsedTime += Time.deltaTime; // Increment elapsed time by the time since last frame

            // Convert elapsedTime to minutes, seconds, and milliseconds
            int minutes = Mathf.FloorToInt(elapsedTime / 60f); // Calculate minutes
            int seconds = Mathf.FloorToInt(elapsedTime % 60f); // Calculate remaining seconds
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f); // Calculate milliseconds

            // Update UI text with minutes, seconds, and milliseconds
            timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds); // Display formatted time
        }
    }

    public void StopTimer()
    {
        timerRunning = false; // Deactivate the timer
    }

    public void ResetTimer()
    {
        elapsedTime = 0f; // Reset elapsed time to zero
        timerRunning = true; // Reactivate the timer
    }

    public float GetFinalTime()
    {
        return elapsedTime; // Return the total elapsed time
    }
}
