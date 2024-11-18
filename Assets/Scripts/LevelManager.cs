// LevelManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/01/2024
// Course: EECS 581
// Purpose: Level Manager

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Singleton instance
    public static LevelManager Instance { get; private set; }

    [Header("Scene Settings")]
    public string sceneName = "Test"; // Replace "Test" with your actual scene name

    // Difficulty Tracking Variables
    [Header("Difficulty Settings")]
    public int totalCompletions = 0;
    public int completionsForGradualIncrease = 10; // Increase difficulty every 5 completions
    public int completionsForLevelUpgrade = 100;  // Switch from Easy to Medium after 100 completions

    // Difficulty Levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Easy;  // Current game difficulty

    // Enemy spawn chances for each difficulty level
    [Range(0f, 1f)]
    public float spikeSpawnChance = 0f;          // Spike spawn chance
    [Range(0f, 1f)]
    public float EnemySpawnChance = 0f;      // Easy enemy spawn chance

    // Number of chunks per level based on difficulty
    public int numberOfChunks = 4;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Register the callback when a scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unregister the callback
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is called every time a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is one where you want to generate a level
        if (scene.name == "Test") // Replace "Test" with your scene name
        {
            // Find the ProcGen instance in the scene and generate the level
            ProcGen procGen = FindAnyObjectByType<ProcGen>();
            if (procGen != null)
            {
                procGen.GenerateNewLevel();
            }
            else
            {
                Debug.LogError("ProcGen script not found in the scene.");
            }
            // ProcGen.Instance.OnLevelCompleted();
            // ProcGen.Instance.GenerateNewLevel();
        }
    }

    // Public method to load a scene
    public void LoadScene(string sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Method to handle level completion
    public void LevelCompleted()
    {
        totalCompletions++;

        // Gradually increase difficulty every 5 completions
        if (totalCompletions % completionsForGradualIncrease == 0)
        {
            numberOfChunks += 1; // Incrementally increase chunks
            switch (currentDifficulty)
            {
                case Difficulty.Easy:
                    EnemySpawnChance = Mathf.Min(EnemySpawnChance + 0.1f, 0.6f); // Cap at 60%
                    Debug.Log($"Increasing Difficulty: Spawn Chance: {EnemySpawnChance}, Chunk Size: {numberOfChunks}");
                    break;
            }
        }

        // Upgrade difficulty level after 100 completions
        if (totalCompletions >= completionsForLevelUpgrade)
        {
            if (currentDifficulty == Difficulty.Easy)
            {
                currentDifficulty = Difficulty.Medium;
                numberOfChunks = 4; // Significant increase in chunks
                EnemySpawnChance = 0.1f; // Set a base spawn rate
                spikeSpawnChance = Mathf.Max(spikeSpawnChance, 0.0f); // Optionally increase spike spawn rate
                Debug.Log("Switched difficulty to Medium.");
            }
            else if (currentDifficulty == Difficulty.Medium)
            {
                currentDifficulty = Difficulty.Hard;
                numberOfChunks = 4; // Significant increase in chunks
                EnemySpawnChance = 0.1f; // Set a base spawn rate
                spikeSpawnChance = Mathf.Max(spikeSpawnChance, 0.0f); // Optionally increase spike spawn rate
                Debug.Log("Switched difficulty to Hard.");
            }

            totalCompletions = 0; // Reset total completions after upgrading difficulty
            completionsForLevelUpgrade += 100; // Set next threshold for difficulty upgrade
        }

        // Notify ProcGen to generate a new level
        if (ProcGen.Instance != null)
        {
            ProcGen.Instance.GenerateNewLevel();
        }
        else
        {
            Debug.LogError("ProcGen instance not found.");
        }

        // Reload the scene to generate a new level
        LoadScene("Test");
    }

}
