// LevelManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/01/2024
// Course: EECS 581
// Purpose: Level Manager

using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Singleton instance
    public static LevelManager Instance { get; private set; }

    [Header("Scene Settings")]
    public string sceneName = "Test";

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

    public Difficulty currentDifficulty = Difficulty.Hard;  // Current game difficulty

    public static int selectedDifficulty = 1;   // Player's selected difficulty

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

        if (scene.name == "Main Menu"){
            float bestDist = PlayerPrefs.GetFloat("BestDistance", 0f);
            float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
            int totalCompletions = PlayerPrefs.GetInt("TotalCompletions", 0);
            LeaderboardUI leaderboardUI = FindObjectOfType<LeaderboardUI>();
            if (leaderboardUI != null)
            {
                leaderboardUI.UpdateFreerunLeaderboard(bestDist);
                leaderboardUI.UpdateProcGenLeaderboard(totalCompletions);
                leaderboardUI.UpdateStoryLeaderboard(bestTime);
            }
            else
            {
                Debug.LogWarning("LeaderboardUI not found in the Main Menu scene.");
            }
        }

        // Check if the loaded scene is one where you want to generate a level
        if (scene.name == "Test") // Replace "Test" with your scene name
        {
            // Find the ProcGen instance in the scene and generate the level
            ProcGen procGen = FindAnyObjectByType<ProcGen>();
            if (procGen != null)
            {
                currentDifficulty = (Difficulty)selectedDifficulty; 
                procGen.GenerateNewLevel();
            }
            else
            {
                Debug.LogError("ProcGen script not found in the scene.");
            }
            // ProcGen.Instance.OnLevelCompleted();
            // ProcGen.Instance.GenerateNewLevel();
        }
        if (scene.name == "Freerun")
        {
            // Instead of ProcGen, find freerun manager class
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            if (freerunManager != null)
            {
                currentDifficulty = (Difficulty)selectedDifficulty; 
                freerunManager.StartFreeRunMode(); // Call a method to begin freerun mode
            }
            else
            {
                Debug.LogError("FreerunManager not found in the scene.");
            }
        }
    }

    // Public method to load a scene
    public void LoadScene(string sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // private void UpdateLeaderboardUI(float finalDistance)
    // {
    //     float bestDist = PlayerPrefs.GetFloat("BestDistance", 0f);

    //     LeaderboardUI leaderboardUI = FindObjectOfType<LeaderboardUI>();
    //     if (leaderboardUI != null)
    //     {
    //         // Update the leaderboard with the bestDist
    //         leaderboardUI.UpdateFreerunLeaderboard(bestDist);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("LeaderboardUI not found in the scene.");
    //     }
    // }


    public void CheckLivesCondition(bool p2, int livesP1, int livesP2)
    {
        // If single player and P1 is out of lives
        if (!p2 && livesP1 <= 0)
        {
            // Get the player's final distance
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            float finalDistance = freerunManager != null ? freerunManager.playerDistance : 0f;

            // Compare with stored best distance
            float currentBest = PlayerPrefs.GetFloat("BestDistance", 0f);
            if (finalDistance > currentBest)
            {
                PlayerPrefs.SetFloat("BestDistance", finalDistance);
                PlayerPrefs.Save(); // Ensure data is written to disk
            }

            // Now you can trigger the Leaderboard UI update if needed
            // UpdateLeaderboardUI(finalDistance);

            PlayerPrefs.SetInt("TotalCompletions", totalCompletions);
            PlayerPrefs.Save();

            LevelTimer timer = FindObjectOfType<LevelTimer>();
            timer.StopTimer();
            float bestTime = timer.GetFinalTime();

            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();


            // Load main menu or handle end-of-run flow
            LoadScene("Main Menu");
        }
        else if (p2 && livesP1 <= 0 && livesP2 <= 0)
        {
            // Co-op mode end
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            float finalDistance = freerunManager != null ? freerunManager.playerDistance : 0f;

            float currentBest = PlayerPrefs.GetFloat("BestDistance", 0f);
            if (finalDistance > currentBest)
            {
                PlayerPrefs.SetFloat("BestDistance", finalDistance);
                PlayerPrefs.Save();
            }

            PlayerPrefs.SetInt("TotalCompletions", totalCompletions);
            PlayerPrefs.Save();

            LevelTimer timer = FindObjectOfType<LevelTimer>();
            timer.StopTimer();
            float bestTime = timer.GetFinalTime();

            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();

            // UpdateLeaderboardUI(finalDistance);
            LoadScene("Main Menu");
        }
    }



    // Method to handle level completion
    public void LevelCompleted(string sceneName)
    {
    if(sceneName == "Test")
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
    
    else if (sceneName == "Level6")
        {
            LoadScene("Main Menu");
        }
    else {
         int lastNumber = int.Parse(sceneName[^1].ToString());

        // Increment the number
        int nextNumber = lastNumber + 1;

        // Replace the last digit with the incremented number
        string nextLevelName = sceneName.Substring(0, sceneName.Length - 1) + nextNumber;
        LoadScene(nextLevelName);
    }
    }
}
