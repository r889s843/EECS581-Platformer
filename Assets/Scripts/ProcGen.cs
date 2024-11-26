// ProcGen.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/01/2024
// Course: EECS 581
// Purpose: Level Generator with Dynamic Difficulty Adjustment

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class ProcGen : MonoBehaviour
{
    // Singleton instance to ensure only one ProcGen exists
    public static ProcGen Instance { get; private set; }

    // Prefabs for various game elements
    public GameObject wallPrefab;        // Prefab for walls
    public GameObject flagPrefab;        // Prefab for the goal flag
    public GameObject deathZonePrefab;   // Prefab for the death zone area

    // Tilemap references
    public Tilemap groundTilemap;        // Assign Ground Tilemap in Inspector
    public Tilemap hazardTilemap;        // Assign Hazard Tilemap in Inspector

    // Tile references for ground
    public TileBase leftTile;            // Tile for the left end of a platform
    public TileBase centerTile;          // Tile for the center of a platform
    public TileBase rightTile;           // Tile for the right end of a platform

    // Tile reference for hazards
    public TileBase spikeTile;           // Spike/hazard tile

    // Level Generation Settings
    public int numberOfChunks = 4;       // Starting Number of chunks
    public float startX = 0f;            // Starting X position
    public float startY = 0f;            // Starting Y position
    public float minY = -10f;            // Minimum Y position
    public float maxY = 10f;             // Maximum Y position

    public int totalCompletions = 0;
    public int completionsForGradualIncrease = 5; // Increase difficulty every 5 completions
    public int completionsForLevelUpgrade = 100;  // Switch from Easy to Medium after 100 completions


    // Difficulty Levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Medium;  // Current game difficulty

    // Enemy prefabs categorized by difficulty
    public List<GameObject> easyEnemyPrefabs;    // Enemies for Easy difficulty
    public List<GameObject> mediumEnemyPrefabs;  // Enemies for Medium difficulty
    public List<GameObject> hardEnemyPrefabs;    // Enemies for Hard difficulty

    [Header("Spike Settings")]
    [Range(0f, 1f)]
    public float spikeSpawnChance = 0f;          // 0% chance to spawn spikes initially

    // Enemy spawn chances for each difficulty level
    [Range(0f, 1f)]
    public float EnemySpawnChance = 0f;      // 0% chance for Easy enemies initially
    // [Range(0f, 1f)]
    // public float mediumEnemySpawnChance = 0f;    // 0% chance for Medium enemies initially
    // [Range(0f, 1f)]
    // public float hardEnemySpawnChance = 0f;      // 0% chance for Hard enemies initially

    // Run Tracking
    [Header("Run Tracking")]
    public int runsPerDifficultyIncrease = 100;  // Number of runs before increasing difficulty

    private List<GameObject> generatedObjects = new List<GameObject>(); // List to track instantiated GameObjects

    private void Awake()
    {
        // Implementing Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }


    private void Start()
    {
        GenerateNewLevel();     // Generate the first level
    }

    // Public method to start generating a new level
    public void GenerateNewLevel()
    {
        ClearExistingLevel(); // Clear any previously generated level

        Vector2 initialPlatformEnd = CreateInitialPlatform();
        float x = initialPlatformEnd.x;      // Initialize current X position after initial platform
        float y = initialPlatformEnd.y;      // Initialize current Y position after initial platform
        float minYReached = y;               // Track the lowest Y position reached

        // Retrieve current difficulty settings from LevelManager
        Difficulty currentDifficulty = (Difficulty)LevelManager.Instance.currentDifficulty;
        int numberOfChunks = LevelManager.Instance.numberOfChunks;
        float spikeSpawnChance = LevelManager.Instance.spikeSpawnChance;
        float EnemySpawnChance = LevelManager.Instance.EnemySpawnChance;

        // Set ProcGen's difficulty settings based on LevelManager
        this.currentDifficulty = currentDifficulty;
        this.numberOfChunks = numberOfChunks;
        this.spikeSpawnChance = spikeSpawnChance;
        this.EnemySpawnChance = EnemySpawnChance;

        // Generate the specified number of chunks
        for (int i = 0; i < numberOfChunks; i++)
        {
            Vector2 newCoords = CreateSafeChunk(x, y);    // Create a safe chunk
            x = newCoords.x;                              // Update X position
            y = newCoords.y;                              // Update Y position

            newCoords = CreateDangerChunk(x, y);         // Create a danger chunk
            x = newCoords.x;                              // Update X position
            y = newCoords.y;                              // Update Y position

            minYReached = Mathf.Min(minYReached, y);     // Update the minimum Y reached
        }

        CreateEndChunk(x, y);             // Create the end chunk with the flag
        CreateDeathZone(x, minYReached);  // Create the death zone area
    }
// THIS HAS BEEN REFACTORED BUT STILL GOOD TO HAVE HERE SINCE I MAY NEED TO USE THIS FOR THE INFINITE RUN MODE
    // // Method to be called by the agent upon successful level completion
    //  public void OnLevelCompleted()
    // {
    //     totalCompletions++;
    //     Debug.Log($"Level Completed! Total Completions: {totalCompletions}, Current Difficulty: {currentDifficulty}, Chunks: {numberOfChunks}, Enemy Spawn Chance: {easyEnemySpawnChance}, Spike Spawn Chance: {spikeSpawnChance}");
    //     // Gradually increase difficulty every 5 completions
    //     if (totalCompletions % completionsForGradualIncrease == 0)
    //     {
    //         numberOfChunks += 1;          // Incrementally increase chunks
    //         switch (currentDifficulty)
    //         {
    //             case Difficulty.Easy:
    //                 easyEnemySpawnChance = Mathf.Min(easyEnemySpawnChance + 0.05f, 0.6f); // Cap at 60%
    //                 Debug.Log($"Increasing Difficulty: Spawn Chance: {easyEnemySpawnChance}, Chunk Size: {numberOfChunks}");
    //                 break;
    //             case Difficulty.Medium:
    //                 mediumEnemySpawnChance = Mathf.Min(mediumEnemySpawnChance + 0.05f, 0.6f); // Cap at 60%
    //                 Debug.Log($"Increasing Difficulty: Spawn Chance: {mediumEnemySpawnChance}, Chunk Size: {numberOfChunks}");
    //                 break;
    //             case Difficulty.Hard:
    //                 hardEnemySpawnChance = Mathf.Min(hardEnemySpawnChance + 0.05f, 0.6f); // Cap at 60%
    //                 Debug.Log($"Increasing Difficulty: Spawn Chance: {hardEnemySpawnChance}, Chunk Size: {numberOfChunks}");
    //                 break;
    //         }
    //     }
    //     // Upgrade difficulty level after 100 completions
    //     if (totalCompletions >= completionsForLevelUpgrade && currentDifficulty == Difficulty.Easy)
    //     {
    //         currentDifficulty = Difficulty.Medium;
    //         numberOfChunks = 4;          // Reset chunks
    //         totalCompletions = 0;
    //         Debug.Log("Switched difficulty to Medium.");
    //     }
    //     if (totalCompletions >= completionsForLevelUpgrade && currentDifficulty == Difficulty.Medium)
    //     {
    //         currentDifficulty = Difficulty.Hard;
    //         numberOfChunks = 4;          // Reset chunks
    //         totalCompletions = 0;
    //         Debug.Log("Switched difficulty to Hard.");
    //     }
    //     GenerateNewLevel(); // Generate the next level with updated parameters
    // }
// THIS HAS BEEN REFACTORED BUT STILL GOOD TO HAVE HERE SINCE I MAY NEED TO USE THIS FOR THE INFINITE RUN MODE

    // Clears all previously generated level elements
    private void ClearExistingLevel()
    {
        // Clear all tiles from the ground and hazard tilemaps
        groundTilemap.ClearAllTiles();
        hazardTilemap.ClearAllTiles();

        // Destroy all instantiated GameObjects (walls, flags, death zones, enemies)
        foreach (GameObject obj in generatedObjects)
        {
            Destroy(obj);
        }
        generatedObjects.Clear(); // Clear the tracking list
    }

    // Creates the initial platform at x=0, y=0 with 4 tiles: left, center, center, right
    private Vector2 CreateInitialPlatform()
    {
        float currentX = 0f;
        float currentY = 0f;

        // Place left tile at x=0
        PaintGroundTile(currentX, currentY, leftTile);
        currentX += 1f;

        // Place first center tile at x=1
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;

        // Place second center tile at x=2
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;

        // Place right tile at x=3
        PaintGroundTile(currentX, currentY, rightTile);
        currentX += 1f;

        return new Vector2(currentX, currentY); // Returns (4f, 0f)
    }

    // Creates a safe chunk consisting of ground/platform tiles
    private Vector2 CreateSafeChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int minPlatformLength = GetMinimumPlatformLength();                          // Get minimum platform length based on difficulty
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3);   // Randomize platform length

        // Instantiate ground/platform tiles using left, center, and right tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                // For platforms 2 tiles wide (Hard difficulty)
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Place center tile
            }
            currentX += 1f; // Move to the next tile position
        }

        return new Vector2(currentX, currentY); // Return the new position after the platform
    }

    // Creates a danger chunk which can include gaps, jumps, or wall jumps based on difficulty
    private Vector2 CreateDangerChunk(float x, float y)
    {
        if (currentDifficulty == Difficulty.Easy)
        {
            int randomValue = Random.Range(0, 3); // Random value between 0 and 2
            switch (randomValue)
            {
                case 0:
                    return CreateGap(x, y);        // Create a gap
                case 1:
                    return CreateJump(x, y);       // Create a jump
                case 2:
                    return CreateShortJump(x, y);  // Create a short jump
                default:
                    return new Vector2(x, y);       // Default position
            }
        }
        else if (currentDifficulty == Difficulty.Medium)
        {
            int randomValue = Random.Range(0, 4); // Random value between 0 and 3
            switch (randomValue)
            {
                case 0:
                    return CreateGap(x, y);            // Create a gap
                case 1:
                    return CreateJump(x, y);           // Create a jump
                case 2:
                    return CreateShortJump(x, y);      // Create a short jump
                case 3:
                    return CreateWallJumpSection(x, y); // Create a wall jump section
                default:
                    return new Vector2(x, y);           // Default position
            }
        }
        else // Difficulty.Hard
        {
            int randomValue = Random.Range(0, 3); // Random value between 0 and 2
            switch (randomValue)
            {
                case 0:
                    return CreateJump(x, y);           // Create a jump
                case 1:
                    return CreateShortJump(x, y);      // Create a short jump
                case 2:
                    return CreateWallJumpSection(x, y); // Create a wall jump section
                default:
                    return new Vector2(x, y);           // Default position
            }
        }
    }

    // Creates a gap between platforms
    private Vector2 CreateGap(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // Determine gap size randomly between 5 and 8 units
        float gapSize = Random.Range(5f, 8f);

        // Adjust gap size to ensure it's passable without momentum
        while (!NoMomentumJumpTest(gapSize, 0f))
        {
            gapSize -= 0.5f;
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move X position forward by gap size

        float platformStartX = currentX; // Starting X for the next platform

        int minPlatformLength = GetMinimumPlatformLength();                        // Get minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2); // Randomize platform length

        // Instantiate ground/platform tiles using left, center, and right tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                // For platforms 2 tiles wide (Hard difficulty)
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Place center tile
            }
            currentX += 1f; // Move to the next tile position
        }

        float platformEndX = currentX; // Ending X position of the platform

        SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform

        if (currentDifficulty != Difficulty.Easy)
        {
            // Spawn spikes on the platform for Medium and Hard difficulties
            SpawnSpikes(platformStartX, platformEndX, currentY);
        }

        return new Vector2(currentX, currentY); // Return the new position after the platform
    }

    // Creates a jump between platforms
    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x;
        float deltaY = Random.Range(0, 2) == 0 ? 2f : -2f; // Randomly decide to jump up or down
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY); // Clamp Y position within limits

        // Determine gap size randomly between 4 and 6 units
        float gapSize = Random.Range(4f, 6f);

        // Adjust gap size to ensure the jump is possible with momentum
        while (!MomentumJumpTest(gapSize, currentY - y))
        {
            gapSize -= 0.5f;
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move X position forward by gap size

        int minPlatformLength = GetMinimumPlatformLength();                        // Get minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2); // Randomize platform length

        float platformStartX = currentX; // Starting X for the next platform

        // Instantiate ground/platform tiles using left, center, and right tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                // For platforms 2 tiles wide (Hard difficulty)
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Place center tile
            }
            currentX += 1f; // Move to the next tile position
        }

        float platformEndX = currentX; // Ending X position of the platform

        SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform

        return new Vector2(currentX, currentY); // Return the new position after the platform
    }

    // Creates a short jump consisting of multiple platforms
    private Vector2 CreateShortJump(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int numPlatforms = Random.Range(2, 4); // Number of platforms in the short jump

        for (int i = 0; i < numPlatforms; i++)
        {
            float gapSize = 2f; // Small gap size between platforms

            // Random elevation change based on difficulty
            float deltaY = Random.Range(-1, 2) * GetMinimumVerticalSpacing();
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y position within limits

            // Ensure sufficient vertical spacing
            if (Mathf.Abs(nextY - currentY) < GetMinimumVerticalSpacing())
            {
                deltaY = GetMinimumVerticalSpacing() * Mathf.Sign(deltaY);
                nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);
            }

            // Adjust gap size to ensure jump is possible without momentum
            while (!NoMomentumJumpTest(gapSize, nextY - currentY))
            {
                gapSize -= 0.5f;
                if (gapSize <= 0f)
                {
                    gapSize = 1f; // Minimum gap size
                    break;
                }
            }

            currentX += gapSize; // Move X position forward by gap size
            currentY = nextY;     // Update Y position

            int minPlatformLength = GetMinimumPlatformLength();                        // Get minimum platform length
            int platformLength = Random.Range(minPlatformLength, minPlatformLength + 1); // Randomize platform length

            float platformStartX = currentX; // Starting X for the next platform

            // Instantiate ground/platform tiles using left, center, and right tiles
            for (int j = 0; j < platformLength; j++)
            {
                if (platformLength == 2)
                {
                    // For platforms 2 tiles wide (Hard difficulty)
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                    else
                        PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                }
                else
                {
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                    else if (j == platformLength - 1)
                        PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                    else
                        PaintGroundTile(currentX, currentY, centerTile); // Place center tile
                }
                currentX += 1f; // Move to the next tile position
            }

            float platformEndX = currentX; // Ending X position of the platform

            SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform

            if (currentDifficulty != Difficulty.Easy)
            {
                // Spawn spikes on the platform for Medium and Hard difficulties
                SpawnSpikes(platformStartX, platformEndX, currentY);
            }
        }

        return new Vector2(currentX, currentY); // Return the new position after the platforms
    }

    // Creates a wall jump section with walls and platforms
    private Vector2 CreateWallJumpSection(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // **1. Entry Platform**
        PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
        currentX += 1f; // Move to the next tile position
        PaintGroundTile(currentX, currentY, centerTile); // Place center tile
        currentX += 1f; // Move to the next tile position
        PaintGroundTile(currentX, currentY, centerTile); // Place center tile
        currentX += 1f; // Move to the next tile position
        PaintGroundTile(currentX, currentY, centerTile); // Place center tile
        currentX += 1f; // Move to the next tile position
        PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
        currentX += 1f; // Move to the next tile position

        // **2. Place the First Wall**
        CreateWall(currentX - 3f, currentY + 7f); // Instantiate the first wall at an elevated position
        // Note: The Y offset (+7) should match your wall prefab's size and desired placement
        currentX += 1f; // Move to the next tile position

        // **3. Place the Second Wall**
        float wallGap = 5f; // Gap between the two walls for the player to perform wall jumps
        CreateWall(currentX - 4f + wallGap, currentY + 5f); // Instantiate the second wall
        // Note: Adjust the Y offset (+5) based on your wall prefab's size

        // **4. Platform at the Top Right**
        float exitPlatformX = currentX - 4f + wallGap + 1f;    // X position for the exit platform
        float wallHeight = wallPrefab.GetComponent<Renderer>().bounds.size.y; // Get wall height from prefab
        float exitPlatformY = currentY + wallHeight;          // Y position for the exit platform
        int exitPlatformLength = 2;                           // Number of tiles in the exit platform

        // Instantiate exit ground/platform tiles using left, center, and right tiles
        for (int i = 0; i < exitPlatformLength; i++)
        {
            if (exitPlatformLength == 2)
            {
                // For platforms 2 tiles wide (Hard difficulty)
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile);   // Place left tile
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile);  // Place right tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile);   // Place left tile
                else if (i == exitPlatformLength - 1)
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile);  // Place right tile
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, centerTile); // Place center tile
            }
            exitPlatformX += 1f; // Move to the next tile position
        }

        // **5. Update currentX and currentY to the end of the exit platform**
        currentX = exitPlatformX;
        currentY = exitPlatformY;

        SpawnEnemies(currentX - (exitPlatformLength * 1f), currentX, currentY); // Adjusted multiplier to 1f
        SpawnSpikes(currentX - (exitPlatformLength * 1f), currentX, currentY); // Adjusted multiplier to 1f

        return new Vector2(currentX, currentY); // Return the new position after the exit platform
    }

    // Creates a wall by instantiating the wall prefab
    private void CreateWall(float x, float startY)
    {
        if (wallPrefab != null)
        {
            GameObject wall = Instantiate(wallPrefab, new Vector3(x, startY, 0f), Quaternion.identity, transform); // Instantiate wall
            generatedObjects.Add(wall); // Track the instantiated wall for cleanup
        }
    }

    // Creates the end chunk with the flag
    private Vector2 CreateEndChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int platformLength = 5; // Number of tiles in the end platform

        // Instantiate ground/platform tiles using left, center, and right tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                // For platforms 2 tiles wide (Hard difficulty)
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);   // Place left tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);  // Place right tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Place center tile
            }
            currentX += 1f; // Move to the next tile position
        }

        // Place the flag at the end of the platform
        if (flagPrefab != null)
        {
            GameObject flag = Instantiate(
                flagPrefab,
                new Vector3(currentX - 6f, currentY + 5f, 0f), // Position the flag slightly above ground
                flagPrefab.transform.rotation,
                transform
            );
            flag.name = "Flag"; // Name the flag object for easy identification
            generatedObjects.Add(flag); // Track the instantiated flag for cleanup

            // Update the agent's goalTransform to the flag's position
            PlatformerAgent[] agents = FindObjectsOfType<PlatformerAgent>();
            if (agents.Length > 0)
            {
                foreach (PlatformerAgent agent in agents){
                    agent.goalTransform = flag.transform; // Set the goal for the agent
                } 
            }
            else
            {
                Debug.LogWarning("PlatformerAgent not found in the scene.");
            }
        }

        return new Vector2(currentX, currentY); // Return the new position after the flag
    }

    // Creates the death zone area to handle player falling off the level
    private void CreateDeathZone(float x, float minY)
    {
        if (deathZonePrefab != null)
        {
            // Calculate the new X position by shifting 10 units to the left
            // This adjustment ensures that the death zone extends 20 units more to the left
            float deathZoneX = ((x + 10f) / 2f) - 10f; // Shifted 10 units left

            // Instantiate the death zone prefab at the adjusted position
            GameObject deathZone = Instantiate(
                deathZonePrefab,
                new Vector3(deathZoneX, minY - 4f, 0f), // Adjusted X position
                Quaternion.identity,
                transform
            );

            // Increase the scale.x by 40f to extend 20 units more to the left
            deathZone.transform.localScale = new Vector3(x + 70f, 2f, 1f); // x + 30f + 40f = x + 70f

            deathZone.name = "DeathZone"; // Name the death zone for easy identification
            generatedObjects.Add(deathZone); // Track the instantiated death zone for cleanup
        }
    }


    // Paints a ground tile on the Ground Tilemap at the specified position
    private void PaintGroundTile(float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        groundTilemap.SetTile(tilePos, tile);          // Set the specified tile at the calculated position
    }

    // Paints a spike tile on the Hazard Tilemap at the specified position
    private void PaintSpikeTile(float x, float y)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        hazardTilemap.SetTile(tilePos, spikeTile);      // Set the spike tile at the calculated position
    }

    // Converts world coordinates to Tilemap cell coordinates
    private Vector3Int WorldToTilePosition(float x, float y)
    {
        return groundTilemap.WorldToCell(new Vector3(x, y, 0f)); // Convert to cell position based on ground tilemap
    }

    // Spawns spikes in triplets on the platform based on spikeSpawnChance
    private void SpawnSpikes(float startX, float endX, float y)
    {
        float xPos = startX;
        while (xPos < endX)
        {
            // Decide whether to spawn a triplet at this position
            if (Random.value < spikeSpawnChance)
            {
                // Place three spikes in a row
                for (int i = 0; i < 3; i++)
                {
                    PaintSpikeTile(xPos, y + 1f); // Position spikes slightly above the ground
                    xPos += 1f;                     // Move to the next tile position (1f spacing)

                    // Ensure spikes do not exceed the platform's end
                    if (xPos >= endX)
                        break;
                }
            }
            else
            {
                xPos += 1f; // Move to the next tile position without spawning spikes (1f spacing)
            }
        }
    }

    // Spawns enemies on the platform based on difficulty and spawn chance
    private void SpawnEnemies(float startX, float endX, float y)
    {
        float platformLength = (endX - startX); // Calculate platform length in tiles

        // Only spawn enemies on platforms wider than 2 tiles
        if (platformLength <= 2)
            return;

        // Determine the center X position of the platform
        float centerX = startX + (endX - startX) / 2f;

        // Define a range around the center to spawn enemies (e.g., center ±1 tile)
        float spawnStartX = centerX - 1f; // Adjust the range as needed
        float spawnEndX = centerX + 1f;

        // Clamp the spawn range within the platform boundaries
        spawnStartX = Mathf.Max(spawnStartX, startX + 1f);
        spawnEndX = Mathf.Min(spawnEndX, endX - 1f);

        // Enemy spawn chance and prefab selection based on difficulty
        // float spawnChance = 0f;
        List<GameObject> enemyPrefabs = null;

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                // spawnChance = easyEnemySpawnChance;
                enemyPrefabs = easyEnemyPrefabs;
                break;
            case Difficulty.Medium:
                // spawnChance = mediumEnemySpawnChance;
                enemyPrefabs = mediumEnemyPrefabs;
                break;
            case Difficulty.Hard:
                // spawnChance = hardEnemySpawnChance;
                enemyPrefabs = hardEnemyPrefabs;
                break;
        }

        // Ensure there are enemy prefabs available
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            // Iterate through the spawn range with a step of 1f (tile spacing)
            for (float x = spawnStartX; x < spawnEndX; x += 1f)
            {
                if (Random.value < EnemySpawnChance) // Check if an enemy should spawn at this position
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; // Select a random enemy prefab
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(x, y + 1f, 0f), Quaternion.identity, transform); // Instantiate the enemy
                    generatedObjects.Add(enemy); // Track the instantiated enemy for cleanup
                }
            }
        }
    }

    // Determines the minimum platform length based on difficulty
    private int GetMinimumPlatformLength()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Medium:
                return 3; // Minimum 3 tiles for Easy and Medium
            case Difficulty.Hard:
                return 2; // Minimum 2 tiles for Hard
            default:
                return 2;
        }
    }

    // Determines the minimum vertical spacing between platforms based on difficulty
    private float GetMinimumVerticalSpacing()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Medium:
                return 2f; // More vertical spacing for Easy and Medium
            case Difficulty.Hard:
                return 1f; // Less vertical spacing for Hard
            default:
                return 2f;
        }
    }

    // Determines whether a jump with momentum is possible
    private bool MomentumJumpTest(float ground_x, float ground_y)
    {
        // Constants for the momentum jump curve
        float a = -0.07367866f;
        float b = 1.05197485f;

        // Calculate predicted y based on ground_x
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y; // Return true if the ground_y is below the curve
    }

    // Determines whether a jump without momentum is possible
    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        // Constants for the no momentum jump curve
        float a = -0.92504437f;
        float b = 4.32346315f;

        // Calculate predicted y based on ground_x
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y; // Return true if the ground_y is below the curve
    }

    // Determines whether a wall jump is possible
    private bool WallJumpTest(float player_x, float ground_x, float ground_y)
    {
        // Adjust ground_x if player is behind the platform
        if (player_x < ground_x)
        {
            ground_x = ground_x + (player_x - ground_x) * 2f;
        }

        // Constants for the wall jump curve
        float a = -0.19835401f;
        float b = 1.45395189f;

        // Calculate predicted y based on ground_x
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y; // Return true if the ground_y is below the curve
    }
}
