using UnityEngine;
using UnityEngine.UI;

public class ProcGenUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI completionsText; // Assign in Inspector

    void Start()
    {
        // Load totalCompletions from LevelManager
        UpdateCompletionsText(LevelManager.Instance.totalCompletions);
    }

    public void UpdateCompletionsText(int completions)
    {
        completionsText.text = $"Completions: {completions}";
    }
}
