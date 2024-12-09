// LeaderboardUI.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/07/2024
// Course: EECS 581
// Purpose: Updates leader board on game completion
using UnityEngine;
using UnityEngine.UI; // or using TMPro if using TextMeshPro

public class LeaderboardUI : MonoBehaviour
{
    // Assuming you have Text fields or TMP_Text fields for name and time
    public TMPro.TextMeshProUGUI Freerun_Name;
    public TMPro.TextMeshProUGUI Freerun_Time;

    public TMPro.TextMeshProUGUI ProcGen_Name;
    public TMPro.TextMeshProUGUI ProcGen_Time;

    public TMPro.TextMeshProUGUI Story_Name;
    public TMPro.TextMeshProUGUI Story_Time;

    // void Awake()
    // {
    //     DontDestroyOnLoad(this.gameObject);
    // }


    // Example method for updating the Freerun leaderboard entry
    public void UpdateFreerunLeaderboard(float bestDistance)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" };
        string randomWord = namePool[Random.Range(0, namePool.Length)];
        int randomNum = Random.Range(1, 1000);
        string username = randomWord + randomNum; // e.g., "MonkeyBlues317"

        Freerun_Name.text = username; // Or a saved player name
        Freerun_Time.text = bestDistance.ToString("F2");
    }

    
    public void UpdateProcGenLeaderboard(int totalCompletions)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" };
        string randomWord = namePool[Random.Range(0, namePool.Length)];
        int randomNum = Random.Range(1, 1000);
        string username = randomWord + randomNum; // e.g., "MonkeyBlues317"

        ProcGen_Name.text = username; // Or a saved player name
        ProcGen_Time.text = totalCompletions.ToString("F2");
    }

    
    public void UpdateStoryLeaderboard(float bestTime)
    {
        string[] namePool = { "Monkey", "Tiger", "Ninja", "Blues", "Spark", "Shadow" };
        string randomWord = namePool[Random.Range(0, namePool.Length)];
        int randomNum = Random.Range(1, 1000);
        string username = randomWord + randomNum; // e.g., "MonkeyBlues317"

        int minutes = Mathf.FloorToInt(bestTime / 60f);
        int seconds = Mathf.FloorToInt(bestTime % 60f);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        Story_Name.text = username; // Or a saved player name
        Story_Time.text = timeString;
        
    }
}
