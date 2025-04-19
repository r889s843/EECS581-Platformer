// LevelCompletions.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/08/2024
// Course: EECS 581
// Purpose: Tracks level completions for leader board updates
using UnityEngine;
using UnityEngine.UI;

public class ProcGenUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI completionsText; // Assign in Inspector

    void Start()
    {
        // Load totalCompletions from LevelManager
        UpdateCompletionsText(PlayerManager.Instance.playerData.procGenCompletionCount);
    }

    public void UpdateCompletionsText(int completions)
    {
        completionsText.text = $"Completions: {completions}";
    }
}
