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

        if (PlayerManager.Instance != null && PlayerManager.Instance.playerData != null)
        {
            UpdateFreerunLeaderboard(PlayerManager.Instance.playerData.bestFreerunDistance);
            UpdateProcGenLeaderboard(PlayerManager.Instance.playerData.procGenCompletionCount);
            UpdateStoryLeaderboard(PlayerManager.Instance.playerData.bestStoryTime);
        }

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
        string username = "Unknown";
        if (PlayerManager.Instance != null && PlayerManager.Instance.playerData != null && !string.IsNullOrEmpty(PlayerManager.Instance.playerData.username))
        {
            username = PlayerManager.Instance.playerData.username;
        }

        Freerun_Name.text = username;
        Freerun_Time.text = bestDistance.ToString("F2");
    }

    public void UpdateProcGenLeaderboard(int totalCompletions)
    {
        string username = "Unknown";
        if (PlayerManager.Instance != null && PlayerManager.Instance.playerData != null && !string.IsNullOrEmpty(PlayerManager.Instance.playerData.username))
        {
            username = PlayerManager.Instance.playerData.username;
        }

        ProcGen_Name.text = username;
        ProcGen_Time.text = totalCompletions.ToString("F2");
    }

    public void UpdateStoryLeaderboard(float bestTime)
    {
        string username = "Unknown";
        if (PlayerManager.Instance != null && PlayerManager.Instance.playerData != null && !string.IsNullOrEmpty(PlayerManager.Instance.playerData.username))
        {
            username = PlayerManager.Instance.playerData.username;
        }

        int minutes = Mathf.FloorToInt(bestTime / 60f);
        int seconds = Mathf.FloorToInt(bestTime % 60f);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        Story_Name.text = username;
        Story_Time.text = timeString;
    }
}
