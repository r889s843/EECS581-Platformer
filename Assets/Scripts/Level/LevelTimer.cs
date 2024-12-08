using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour
{
    public TMPro.TextMeshProUGUI timerText;         // Assign this in the Inspector
    private float elapsedTime = 0f;
    private bool timerRunning = false;

    void Start()
    {
        // Start the timer when the level begins
        timerRunning = true;
    }

    void Update()
    {
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;

            // Convert elapsedTime to minutes, seconds, and milliseconds
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);

            // Update UI text with milliseconds
            timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    // Call this method when the player completes the level
    public void StopTimer()
    {
        timerRunning = false;
    }

    // Optionally, call this if you want to reset or restart the timer
    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
    }

    // If you need the final time for something else (like a leaderboard), you can add:
    public float GetFinalTime()
    {
        return elapsedTime;
    }
}
