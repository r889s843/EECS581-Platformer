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
    public string sceneName = "Test"; // Name of the current scene

    // Difficulty Tracking Variables
    [Header("Difficulty Settings")]
    public int totalCompletions = 0; // Total number of level completions
    public int completionsForGradualIncrease = 10; // Increase difficulty every 10 completions
    public int completionsForLevelUpgrade = 100; // Upgrade difficulty after 100 completions

    // Difficulty Levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Hard; // Current game difficulty

    public static int selectedDifficulty = 1; // Player's selected difficulty

    // Enemy spawn chances for each difficulty level
    [Range(0f, 1f)]
    public float spikeSpawnChance = 0f; // Chance to spawn spikes
    [Range(0f, 1f)]
    public float EnemySpawnChance = 0f; // Chance to spawn enemies on Easy difficulty

    // Number of chunks per level based on difficulty
    public int numberOfChunks = 4; // Number of level chunks to generate

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void OnEnable()
    {
        // Register the callback when a scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unregister the callback to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is called every time a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Start")
        {
            // Retrieve saved player statistics
            float bestDist = PlayerPrefs.GetFloat("BestDistance", 0f);
            float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
            int totalCompletions = PlayerPrefs.GetInt("TotalCompletions", 0);

            // Find the LeaderboardUI in the scene and update it
            LeaderboardUI leaderboardUI = FindObjectOfType<LeaderboardUI>();
            if (leaderboardUI != null)
            {
                leaderboardUI.UpdateFreerunLeaderboard(bestDist); // Update Freerun leaderboard
                leaderboardUI.UpdateProcGenLeaderboard(totalCompletions); // Update Procedural Generation leaderboard
                leaderboardUI.UpdateStoryLeaderboard(bestTime); // Update Story mode leaderboard
            }
            else
            {
                Debug.LogWarning("LeaderboardUI not found in the Main Menu scene."); // Warn if LeaderboardUI is missing
            }
        }

        // Check if the loaded scene is one where you want to generate a level
        if (scene.name == "Test") // Replace "Test" with your scene name
        {
            // Find the ProcGen instance in the scene and generate the level
            ProcGen procGen = FindAnyObjectByType<ProcGen>();
            if (procGen != null)
            {
                currentDifficulty = (Difficulty)selectedDifficulty; // Set current difficulty based on selection
                procGen.GenerateNewLevel(); // Generate a new level
            }
            else
            {
                Debug.LogError("ProcGen script not found in the scene."); // Error if ProcGen is missing
            }
            // ProcGen.Instance.OnLevelCompleted();
            // ProcGen.Instance.GenerateNewLevel();
        }
        if (scene.name == "Freerun")
        {
            // Instead of ProcGen, find the FreerunProcGen manager class
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            if (freerunManager != null)
            {
                currentDifficulty = (Difficulty)selectedDifficulty; // Set current difficulty based on selection
                freerunManager.StartFreeRunMode(); // Begin Freerun mode
            }
            else
            {
                Debug.LogError("FreerunManager not found in the scene."); // Error if FreerunProcGen is missing
            }
        }
    }

    // Public method to load a scene by name
    public void LoadScene(string sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex); // Load the specified scene
    }

    public void CheckLivesCondition(bool p2, int livesP1, int livesP2)
    {
        // If single player and Player 1 is out of lives
        if (!p2 && livesP1 <= 0)
        {
            // Get the player's final distance
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            float finalDistance = freerunManager != null ? freerunManager.playerDistance : 0f;

            // Compare with stored best distance
            float currentBest = PlayerPrefs.GetFloat("BestDistance", 0f);
            if (finalDistance > currentBest)
            {
                PlayerPrefs.SetFloat("BestDistance", finalDistance); // Update best distance
                PlayerPrefs.Save(); // Ensure data is written to disk
            }

            PlayerPrefs.SetInt("TotalCompletions", totalCompletions); // Update total completions
            PlayerPrefs.Save(); // Ensure data is written to disk

            // Load main menu or handle end-of-run flow
            LoadScene("Start"); // Transition to Main Menu
        }
        else if (p2 && livesP1 <= 0 && livesP2 > 0)
        {
            // Destroy Player1 if Player1 is out of lives in multiplayer
            GameObject player1 = GameObject.FindGameObjectWithTag("Player");
            if (player1 != null)
            {
                Destroy(player1); // Remove Player1 from the scene

                // Update camera to focus only on Player2
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    CameraController cameraController = mainCamera.GetComponent<CameraController>();
                    GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
                    if (cameraController != null && player2 != null)
                    {
                        cameraController.UpdateCameraTarget(player2.transform); // Focus camera on Player2
                    }
                }
            }
        }
        else if (p2 && livesP1 > 0 && livesP2 <= 0)
        {
            // Destroy Player2 if Player2 is out of lives in multiplayer
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
            if (player2 != null)
            {
                Destroy(player2); // Remove Player2 from the scene

                // Update camera to focus only on Player1
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    CameraController cameraController = mainCamera.GetComponent<CameraController>();
                    GameObject player1 = GameObject.FindGameObjectWithTag("Player");
                    if (cameraController != null && player1 != null)
                    {
                        cameraController.UpdateCameraTarget(player1.transform); // Focus camera on Player1
                    }
                }
            }
        }
        else if (p2 && livesP1 <= 0 && livesP2 <= 0)
        {
            // Co-op mode end when both players are out of lives
            FreerunProcGen freerunManager = FindObjectOfType<FreerunProcGen>();
            float finalDistance = freerunManager != null ? freerunManager.playerDistance : 0f;

            float currentBest = PlayerPrefs.GetFloat("BestDistance", 0f);
            if (finalDistance > currentBest)
            {
                PlayerPrefs.SetFloat("BestDistance", finalDistance); // Update best distance
                PlayerPrefs.Save(); // Ensure data is written to disk
            }

            PlayerPrefs.SetInt("TotalCompletions", totalCompletions); // Update total completions
            PlayerPrefs.Save(); // Ensure data is written to disk

            LoadScene("Start"); // Transition to Main Menu
        }
    }

    // Method to handle level completion
    public void LevelCompleted(string sceneName)
    {
        if (sceneName == "Test")
        {
            totalCompletions++; // Increment total completions

            // Gradually increase difficulty every set number of completions
            if (totalCompletions % completionsForGradualIncrease == 0)
            {
                numberOfChunks += 1; // Increment number of chunks
                switch (currentDifficulty)
                {
                    case Difficulty.Easy:
                        EnemySpawnChance = Mathf.Min(EnemySpawnChance + 0.1f, 0.6f); // Increase enemy spawn chance up to 60%
                        Debug.Log($"Increasing Difficulty: Spawn Chance: {EnemySpawnChance}, Chunk Size: {numberOfChunks}"); // Log difficulty increase
                        break;
                }
            }

            // Upgrade difficulty level after reaching the completion threshold
            if (totalCompletions >= completionsForLevelUpgrade)
            {
                if (currentDifficulty == Difficulty.Easy)
                {
                    currentDifficulty = Difficulty.Medium; // Switch to Medium difficulty
                    numberOfChunks = 4; // Set number of chunks for Medium
                    EnemySpawnChance = 0.1f; // Set base enemy spawn rate for Medium
                    spikeSpawnChance = Mathf.Max(spikeSpawnChance, 0.0f); // Optionally adjust spike spawn rate
                    Debug.Log("Switched difficulty to Medium."); // Log difficulty upgrade
                }
                else if (currentDifficulty == Difficulty.Medium)
                {
                    currentDifficulty = Difficulty.Hard; // Switch to Hard difficulty
                    numberOfChunks = 4; // Set number of chunks for Hard
                    EnemySpawnChance = 0.1f; // Set base enemy spawn rate for Hard
                    spikeSpawnChance = Mathf.Max(spikeSpawnChance, 0.0f); // Optionally adjust spike spawn rate
                    Debug.Log("Switched difficulty to Hard."); // Log difficulty upgrade
                }

                totalCompletions = 0; // Reset total completions after upgrading difficulty
                completionsForLevelUpgrade += 100; // Set next threshold for difficulty upgrade
            }

            // Notify ProcGen to generate a new level
            if (ProcGen.Instance != null)
            {
                ProcGen.Instance.GenerateNewLevel(); // Generate a new level
            }
            else
            {
                Debug.LogError("ProcGen instance not found."); // Error if ProcGen is missing
            }

            // Reload the scene to generate a new level
            LoadScene("Test"); // Reload the "Test" scene
        }
        else if (sceneName == "Level6")
        {
            LevelTimer timer = FindObjectOfType<LevelTimer>(); // Find the LevelTimer in the scene
            timer.StopTimer(); // Stop the timer
            float currentTime = timer.GetFinalTime(); // Get the final time

            // Update leaderboard if the current time is better
            float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
            bestTime = Mathf.Min(bestTime, currentTime); // Keep the lower time
            PlayerPrefs.SetFloat("BestTime", bestTime); // Save the best time
            PlayerPrefs.Save(); // Ensure data is written to disk
            LoadScene("Start"); // Transition to Main Menu
        }
        else
        {
            // Increment the level number based on the current scene name
            int lastNumber = int.Parse(sceneName[^1].ToString()); // Get the last character as number

            // Increment the number for the next level
            int nextNumber = lastNumber + 1; // Increment level number

            // Replace the last digit with the incremented number to get the next level name
            string nextLevelName = sceneName.Substring(0, sceneName.Length - 1) + nextNumber; // Form next level name
            LoadScene(nextLevelName); // Load the next level
        }
    }
}
