// LeaderboardUI.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/07/2024
// Course: EECS 581
// Purpose: Updates leaderboard on game completion

using TMPro;
using UnityEngine;
using UnityEngine.UI; // or using TMPro if using TextMeshPro

public class LeaderboardUI : MonoBehaviour
{
    // TextMeshProUGUI fields for Freerun leaderboard
    public TMPro.TextMeshProUGUI Freerun_Name; // Displays the player's name for Freerun mode
    public TMPro.TextMeshProUGUI Freerun_Time; // Displays the player's best distance for Freerun mode

    // TextMeshProUGUI fields for Procedural Generation leaderboard
    public TMPro.TextMeshProUGUI ProcGen_Name; // Displays the player's name for Procedural Generation mode
    public TMPro.TextMeshProUGUI ProcGen_Time; // Displays the player's total completions for Procedural Generation mode

    // TextMeshProUGUI fields for Story mode leaderboard
    public TMPro.TextMeshProUGUI Story_Name; // Displays the player's name for Story mode
    public TMPro.TextMeshProUGUI Story_Time; // Displays the player's best time for Story mode
    public TMP_Dropdown modeDropdown;
    public GameObject[] leaderboardTabs;

    void Start()
    {
        modeDropdown.onValueChanged.AddListener(OnModeChanged);

    }
    void OnModeChanged(int index)
    {
        foreach (GameObject leaderboard in leaderboardTabs)
        {
            leaderboard.SetActive(false);
        }
        leaderboardTabs[index].SetActive(true);
    }
    public void UpdateFreerunLeaderboard(float bestDistance)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" }; // Pool of random names
        string randomWord = namePool[Random.Range(0, namePool.Length)]; // Select a random name
        int randomNum = Random.Range(1, 1000); // Generate a random number
        string username = randomWord + randomNum; // Combine to create a unique username, e.g., "Monkey317"

        Freerun_Name.text = username; // Set the username in the Freerun leaderboard
        Freerun_Time.text = bestDistance.ToString("F2"); // Set the best distance with two decimal places
    }


    public void UpdateProcGenLeaderboard(int totalCompletions)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" }; // Pool of random names
        string randomWord = namePool[Random.Range(0, namePool.Length)]; // Select a random name
        int randomNum = Random.Range(1, 1000); // Generate a random number
        string username = randomWord + randomNum; // Combine to create a unique username, e.g., "Tiger845"

        ProcGen_Name.text = username; // Set the username in the Procedural Generation leaderboard
        ProcGen_Time.text = totalCompletions.ToString("F2"); // Set the total completions with two decimal places
    }


    public void UpdateStoryLeaderboard(float bestTime)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" }; // Pool of random names
        string randomWord = namePool[Random.Range(0, namePool.Length)]; // Select a random name
        int randomNum = Random.Range(1, 1000); // Generate a random number
        string username = randomWord + randomNum; // Combine to create a unique username, e.g., "Ninja512"

        int minutes = Mathf.FloorToInt(bestTime / 60f); // Calculate minutes from best time
        int seconds = Mathf.FloorToInt(bestTime % 60f); // Calculate remaining seconds from best time
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds); // Format time as MM:SS

        Story_Name.text = username; // Set the username in the Story mode leaderboard
        Story_Time.text = timeString; // Set the formatted best time
    }
}
