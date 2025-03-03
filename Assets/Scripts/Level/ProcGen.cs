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
    // Singleton instance
    public static ProcGen Instance { get; private set; }

    // Prefabs for various game objects
    public GameObject flagPrefab;        // Prefab for the flag at the end of the level
    public GameObject deathZonePrefab;   // Prefab for the death zone area

    // Tilemap references for ground and hazards
    public Tilemap groundTilemap;        // Tilemap for ground tiles
    public Tilemap  wallTilemap;        // Tilemap for walls
    public Tilemap hazardTilemap;        // Tilemap for hazard tiles

    // Tile references for different parts of platforms and hazards
    public TileBase leftTile;            // Tile for the left end of a platform
    public TileBase centerTile;          // Tile for the center of a platform
    public TileBase rightTile;           // Tile for the right end of a platform
    public TileBase leftBottomWallTile;           // Tile for the left end of the wall tile
    public TileBase rightBottomWallTile;           // Tile for the right end of the wall tile
    public TileBase leftTopWallTile;           // Tile for the left end of the wall tile
    public TileBase rightTopWallTile;           // Tile for the right end of the wall tile
    public TileBase spikeTile;           // Spike/hazard tile

    // Platform generation settings
    public int numberOfChunks = 4;       // Number of chunks to generate per level
    public float startX = 0f;            // Starting X position for level generation
    public float startY = 0f;            // Starting Y position for level generation
    public float minY = -10f;            // Minimum Y position for platforms
    public float maxY = 20f;             // Maximum Y position for platforms

    // Difficulty levels enumeration
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Hard; // Current game difficulty

    // Enemy prefabs categorized by difficulty
    public List<GameObject> easyEnemyPrefabs;    // Enemy prefabs for Easy difficulty
    public List<GameObject> mediumEnemyPrefabs;  // Enemy prefabs for Medium difficulty
    public List<GameObject> hardEnemyPrefabs;    // Enemy prefabs for Hard difficulty

    // Spawn chance settings
    [Range(0f, 1f)]
    public float spikeSpawnChance = 0f;          // Chance to spawn spikes
    [Range(0f, 1f)]
    public float EnemySpawnChance = 0f;          // Chance to spawn enemies

    // List to keep track of all generated objects in the level
    private List<GameObject> generatedObjects = new List<GameObject>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment to persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        // GenerateNewLevel(); // Begin level generation when the game starts
    }

    public void GenerateNewLevel()
    {
        ClearExistingLevel(); // Clear any previously generated level

        Vector2 initialPlatformEnd = CreateInitialPlatform(); // Create the initial platform
        float x = initialPlatformEnd.x;      // Current X position for generation
        float y = initialPlatformEnd.y;      // Current Y position for generation
        float minYReached = y;               // Track the minimum Y reached

        // Retrieve current difficulty settings from LevelManager
        Difficulty currentDifficulty = (Difficulty)LevelManager.Instance.currentDifficulty;
        int numberOfChunks = LevelManager.Instance.numberOfChunks;
        float spikeSpawnChance = LevelManager.Instance.spikeSpawnChance;
        float EnemySpawnChance = LevelManager.Instance.EnemySpawnChance;

        // Update ProcGen's settings based on LevelManager
        this.currentDifficulty = currentDifficulty;
        this.numberOfChunks = numberOfChunks;
        this.spikeSpawnChance = spikeSpawnChance;
        this.EnemySpawnChance = EnemySpawnChance;

        // Generate the specified number of chunks
        for (int i = 0; i < numberOfChunks; i++)
        {
            Vector2 newCoords = CreateSafeChunk(x, y); // Create a safe (non-dangerous) chunk
            x = newCoords.x; // Update X position
            y = newCoords.y; // Update Y position
            Debug.Log($"SafeChunk ended at: x = {x}, y = {y}");

            newCoords = CreateDangerChunk(x, y); // Create a dangerous chunk
            x = newCoords.x; // Update X position
            y = newCoords.y; // Update Y position
            Debug.Log($"DangerChunk ended at: x = {x}, y = {y}");

            minYReached = Mathf.Min(minYReached, y); // Update the minimum Y reached
        }
        Debug.Log($"Gen ended at: x = {x}, y = {y}");

        CreateEndChunk(x, y); // Create the ending chunk with the flag
        CreateDeathZone(x, minYReached); // Create the death zone based on the level's end
    }

    private void ClearExistingLevel()
    {
        groundTilemap.ClearAllTiles(); // Clear all ground tiles
        hazardTilemap.ClearAllTiles(); // Clear all hazard tiles

        // Destroy all generated objects (enemies, walls, etc.)
        foreach (GameObject obj in generatedObjects)
        {
            Destroy(obj);
        }
        generatedObjects.Clear(); // Clear the list of generated objects
    }

    private Vector2 CreateInitialPlatform()
    {
        float currentX = 0f; // Starting X position
        float currentY = 0f; // Starting Y position

        // Paint the initial platform tiles
        PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint another center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile

        float finalX = currentX; // Final X position of the initial platform
        float finalY = currentY; // Final Y position of the initial platform
        currentX += 1f; // Move to next position

        return new Vector2(finalX, finalY); // Return the end position of the initial platform
    }

    private Vector2 CreateSafeChunk(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        int minPlatformLength = GetMinimumPlatformLength(); // Determine minimum platform length based on difficulty
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3); // Randomize platform length

        // Paint the platform tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position of the platform
        float finalY = currentY; // Final Y position of the platform
        return new Vector2(finalX, finalY); // Return the end position of the platform
    }

    private Vector2 CreateDangerChunk(float x, float y)
    {
        // Determine if the current Y position is near the maximum allowed height
        bool nearMaxHeight = y >= maxY - 1f;

        if (currentDifficulty == Difficulty.Easy)
        {
            if (nearMaxHeight)
            {
                // Force a downward jump if near maximum height
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y);
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 3); // Random value to decide chunk type
            switch (randomValue)
            {
                case 0: return CreateGap(x, y); // Create a gap
                case 1: return CreateJump(x, y); // Create a jump
                case 2: return CreateShortJump(x, y); // Create a short jump
                default: return new Vector2(x, y); // Default case
            }
        }
        else if (currentDifficulty == Difficulty.Medium)
        {
            if (nearMaxHeight)
            {
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y); // Create a downward jump
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 4); // Random value to decide chunk type
            switch (randomValue)
            {
                case 0: return CreateGap(x, y); // Create a gap
                case 1: return CreateJump(x, y); // Create a jump
                case 2: return CreateShortJump(x, y); // Create a short jump
                case 3: return CreateWallJumpSection(x, y); // Create a wall jump section
                default: return new Vector2(x, y); // Default case
            }
        }
        else // Difficulty.Hard
        {
            if (nearMaxHeight)
            {
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y); // Create a downward jump
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 3); // Random value to decide chunk type
            switch (randomValue)
            {
                case 0: return CreateJump(x, y); // Create a jump
                case 1: return CreateShortJump(x, y); // Create a short jump
                case 2: return CreateWallJumpSection(x, y); // Create a wall jump section
                default: return new Vector2(x, y); // Default case
            }
        }
    }

    private Vector2 CreateGap(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        float gapSize = Random.Range(5f, 8f); // Determine the size of the gap
        while (!NoMomentumJumpTest(gapSize, 0f)) // Ensure the gap is jumpable
        {
            gapSize -= 0.5f; // Decrease gap size if not jumpable
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move past the gap
        float platformStartX = currentX; // Starting X position for the next platform

        int minPlatformLength = GetMinimumPlatformLength(); // Determine minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2); // Randomize platform length

        // Paint the platform tiles after the gap
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position of the platform
        float finalY = currentY; // Final Y position of the platform

        SpawnEnemies(platformStartX, currentX, currentY); // Spawn enemies on the platform

        if (currentDifficulty != Difficulty.Easy)
        {
            SpawnSpikes(platformStartX, currentX, currentY); // Spawn spikes if difficulty is not Easy
        }

        return new Vector2(finalX, finalY); // Return the end position of the platform
    }

    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x; // Current X position
        float deltaY = Random.Range(0, 2) == 0 ? 2f : -2f; // Randomly decide to jump up or down
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY); // Clamp Y within bounds

        float gapSize = Random.Range(4f, 6f); // Determine the size of the gap
        while (!MomentumJumpTest(gapSize, currentY - y)) // Ensure the jump is feasible
        {
            gapSize -= 0.5f; // Decrease gap size if jump is not feasible
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move past the gap
        int minPlatformLength = GetMinimumPlatformLength(); // Determine minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2); // Randomize platform length

        float platformStartX = currentX; // Starting X position for the next platform

        // Paint the elevated platform tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position of the platform
        float finalY = currentY; // Final Y position of the platform

        SpawnEnemies(platformStartX, currentX, currentY); // Spawn enemies on the platform
        return new Vector2(finalX, finalY); // Return the end position of the platform
    }

    private Vector2 CreateShortJump(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        int numPlatforms = Random.Range(2, 4); // Number of small platforms to create

        for (int i = 0; i < numPlatforms; i++) // Create multiple small platforms
        {
            float gapSize = 2f; // Fixed small gap size
            float deltaY = Random.Range(-1, 2) * GetMinimumVerticalSpacing(); // Random Y delta for jump
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds

            if (Mathf.Abs(nextY - currentY) < GetMinimumVerticalSpacing()) // Ensure sufficient vertical change
            {
                deltaY = GetMinimumVerticalSpacing() * Mathf.Sign(deltaY); // Adjust Y delta
                nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds
            }

            while (!NoMomentumJumpTest(gapSize, nextY - currentY)) // Ensure the jump is feasible
            {
                gapSize -= 0.5f; // Decrease gap size if jump is not feasible
                if (gapSize <= 0f)
                {
                    gapSize = 1f; // Minimum gap size
                    break;
                }
            }

            currentX += gapSize; // Move past the gap
            currentY = nextY; // Update Y position

            int minPlatformLength = GetMinimumPlatformLength(); // Determine minimum platform length
            int platformLength = Random.Range(minPlatformLength, minPlatformLength + 1); // Randomize platform length

            float platformStartX = currentX; // Starting X position for the next platform

            // Paint the small platform tiles
            for (int j = 0; j < platformLength; j++)
            {
                if (platformLength == 2)
                {
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                    else
                        PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                }
                else
                {
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                    else if (j == platformLength - 1)
                        PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                    else
                        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
                }
                currentX += 1f; // Move to next position
            }

            float finalX = currentX - 1f; // Final X position of the platform
            float finalY = currentY; // Final Y position of the platform
            SpawnEnemies(platformStartX, currentX, currentY); // Spawn enemies on the platform

            if (currentDifficulty != Difficulty.Easy)
            {
                SpawnSpikes(platformStartX, currentX, currentY); // Spawn spikes if difficulty is not Easy
            }
        }
        return new Vector2(currentX - 1f, currentY);
    }

    private Vector2 CreateWallJumpSection(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        // Entry platform (5 tiles)
        PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
        currentX += 1f; // Move to next position

        for (int i=0; i < 10; i++) {
            // Use the selected tile in the method call
            PaintWallTile(currentX - 3f, currentY+i+3, new TileBase[] { leftBottomWallTile, leftTopWallTile }[Random.Range(0, 2)]);
            PaintWallTile(currentX - 3f + 1f, currentY+i+3, new TileBase[] { rightBottomWallTile, rightTopWallTile }[Random.Range(0, 2)]);
        }

        currentX += 1f; // Move to next position

        float wallGap = 5f; // Gap between walls
        // CreateWall(currentX - 4f + wallGap, currentY + 5f); // Create the second wall with a gap

        for (int i=0; i < 10; i++) {
            // Use the selected tile in the method call
            PaintWallTile(currentX - 4f + wallGap, currentY+i, new TileBase[] { leftBottomWallTile, leftTopWallTile }[Random.Range(0, 2)]); // Paint right end tile
            PaintWallTile(currentX - 4f + 1 + wallGap, currentY+i, new TileBase[] { rightBottomWallTile, rightTopWallTile }[Random.Range(0, 2)]); // Paint right end tile
        }

        float wallHeight = 9; // Get the height of the wall prefab
        float exitPlatformY = currentY + wallHeight; // Determine Y position for the exit platform
        int exitPlatformLength = 2; // Length of the exit platform

        float exitPlatformX = currentX - 4f + wallGap + 1f; // Starting X position for the exit platform

        // Paint the exit platform tiles
        for (int i = 0; i < exitPlatformLength; i++)
        {
            if (exitPlatformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile); // Paint left end tile
                else if (i == exitPlatformLength - 1)
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, centerTile); // Paint center tile
            }
            exitPlatformX += 1f; // Move to next position
        }

        float finalX = exitPlatformX - 1f; // Final X position of the exit platform
        float finalY = exitPlatformY; // Final Y position of the exit platform

        SpawnEnemies(finalX - (exitPlatformLength * 1f), finalX, finalY); // Spawn enemies near the end of the exit platform
        SpawnSpikes(finalX - (exitPlatformLength * 1f), finalX, finalY); // Spawn spikes near the end of the exit platform

        return new Vector2(finalX, finalY); // Return the end position of the exit platform
    }

    // New downward jump similar to CreateJump but forces a downward movement
    private Vector2 CreateDownJump(float x, float y)
    {
        float currentX = x; // Current X position
        // Force a larger downward drop
        float deltaY = Random.Range(-4, -2) * 2f; // Larger Y delta for downward jump
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY); // Clamp Y within bounds

        float gapSize = Random.Range(4f, 6f); // Determine the size of the gap
        while (!MomentumJumpTest(gapSize, currentY - y)) // Ensure the jump is feasible
        {
            gapSize -= 0.5f; // Decrease gap size if jump is not feasible
            if (gapSize <= 1f) break; // Minimum gap size
        }

        currentX += gapSize; // Move past the gap
        int minPlatformLength = GetMinimumPlatformLength(); // Determine minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2); // Randomize platform length

        float platformStartX = currentX; // Starting X position for the next platform

        // Paint the downward platform tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position of the platform
        float finalY = currentY; // Final Y position of the platform

        SpawnEnemies(platformStartX, currentX, currentY); // Spawn enemies on the platform
        if (currentDifficulty != Difficulty.Easy)
            SpawnSpikes(platformStartX, currentX, currentY); // Spawn spikes if difficulty is not Easy

        return new Vector2(finalX, finalY); // Return the end position of the platform
    }

    // New downward wall jump section
    private Vector2 CreateWallDownJumpSectionDown(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        // Entry platform (shorter)
        PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
        currentX += 1f; // Move to next position
        PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
        currentX += 1f; // Move to next position

        // Place a wall above
        // CreateWall(currentX - 1f, currentY - 5f); // Create the first wall
        // CreateWall(currentX + gapSize, currentY + 1f); // Create the second wall with a gap



        for (int i=0; i < 10; i++) {
            // Use the selected tile in the method call
            PaintWallTile(currentX - 1f, currentY-i-2f, new TileBase[] { leftBottomWallTile, leftTopWallTile }[Random.Range(0, 2)]);
            PaintWallTile(currentX - 1f + 1f, currentY-i-2f, new TileBase[] { rightBottomWallTile, rightTopWallTile }[Random.Range(0, 2)]);
        }

        float wallGap = 5f; // Gap size between walls
        currentX += 1f; // Move to next position

        for (int i=0; i < 10; i++) {
            // Use the selected tile in the method call
            PaintWallTile(currentX + wallGap, currentY-i+1f, new TileBase[] { leftBottomWallTile, leftTopWallTile }[Random.Range(0, 2)]); // Paint right end tile
            PaintWallTile(currentX + 1 + wallGap, currentY-i+1f, new TileBase[] { rightBottomWallTile, rightTopWallTile }[Random.Range(0, 2)]); // Paint right end tile
        }

        // float wallHeight = 9; // Get the height of the wall prefab

        // Move downward more significantly
        float deltaY = Random.Range(-4, -3) * 2f; // Larger Y delta for downward jump
        float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds

        while (!MomentumJumpTest(wallGap, nextY - currentY)) // Ensure the jump is feasible
        {
            wallGap -= 0.5f; // Decrease gap size if jump is not feasible
            if (wallGap < 1f) wallGap = 1f; // Minimum gap size
        }

        currentX += wallGap; // Move past the gap
        currentY = nextY; // Update Y position

        int platformLength = 3; // Length of the exit platform
        float platformStartX = currentX; // Starting X position for the exit platform

        // Paint the exit platform tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position of the exit platform
        float finalY = currentY; // Final Y position of the exit platform

        SpawnEnemies(platformStartX, currentX, currentY); // Spawn enemies on the platform
        if (currentDifficulty != Difficulty.Easy)
            SpawnSpikes(platformStartX, currentX, finalY); // Spawn spikes if difficulty is not Easy

        return new Vector2(finalX, finalY); // Return the end position of the exit platform
    }

    // private void CreateWall(float x, float startY)
    // {
    //     if (wallPrefab != null)
    //     {
    //         GameObject wall = Instantiate(wallPrefab, new Vector3(x, startY, 0f), Quaternion.identity, transform); // Instantiate wall prefab
    //         generatedObjects.Add(wall); // Add wall to the list of generated objects
    //     }
    // }

    private Vector2 CreateEndChunk(float x, float y)
    {
        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        int platformLength = 5; // Length of the ending platform

        // Paint the ending platform tiles
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile); // Paint left end tile
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile); // Paint right end tile
                else
                    PaintGroundTile(currentX, currentY, centerTile); // Paint center tile
            }
            currentX += 1f; // Move to next position
        }

        if (flagPrefab != null)
        {
            // Instantiate the flag at the end of the platform
            GameObject flag = Instantiate(
                flagPrefab,
                new Vector3(currentX - 6f, currentY + 5f, 0f),
                flagPrefab.transform.rotation,
                transform
            );
            flag.name = "Flag"; // Name the flag object
            generatedObjects.Add(flag); // Add flag to the list of generated objects

            // Assign the flag as the goal for all PlatformerAgent instances
            PlatformerAgent[] agents = FindObjectsOfType<PlatformerAgent>();
            if (agents.Length > 0)
            {
                foreach (PlatformerAgent agent in agents)
                {
                    agent.goalTransform = flag.transform; // Set the goal transform to the flag
                }
            }
            else
            {
                Debug.LogWarning("PlatformerAgent not found in the scene."); // Warn if no PlatformerAgent is found
            }
        }

        float finalX = currentX - 1f; // Final X position of the ending platform
        float finalY = currentY; // Final Y position of the ending platform
        return new Vector2(finalX, finalY); // Return the end position of the ending platform
    }

    private void CreateDeathZone(float x, float minY)
    {
        if (deathZonePrefab != null)
        {
            float deathZoneX = ((x + 10f) / 2f) - 10f; // Calculate X position for the death zone

            // Instantiate the death zone prefab
            GameObject deathZone = Instantiate(
                deathZonePrefab,
                new Vector3(deathZoneX, minY - 4f, 0f),
                Quaternion.identity,
                transform
            );

            deathZone.transform.localScale = new Vector3(x + 70f, 2f, 1f); // Set the scale of the death zone
            deathZone.name = "DeathZone"; // Name the death zone object
            generatedObjects.Add(deathZone); // Add death zone to the list of generated objects
        }
    }

    private void PaintGroundTile(float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        groundTilemap.SetTile(tilePos, tile); // Set the ground tile at the specified position
    }
    private void PaintWallTile(float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        wallTilemap.SetTile(tilePos, tile); // Set the ground tile at the specified position
    }

    private void PaintSpikeTile(float x, float y)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        hazardTilemap.SetTile(tilePos, spikeTile); // Set the spike tile at the specified position
    }

    private Vector3Int WorldToTilePosition(float x, float y)
    {
        return groundTilemap.WorldToCell(new Vector3(x, y, 0f)); // Convert world coordinates to tilemap cell position
    }

    private void SpawnSpikes(float startX, float endX, float y)
    {
        float xPos = startX; // Starting X position
        while (xPos < endX)
        {
            if (Random.value < spikeSpawnChance) // Check if spikes should spawn
            {
                for (int i = 0; i < 3; i++) // Spawn 3 spikes
                {
                    PaintSpikeTile(xPos, y + 1f); // Paint spike tile above the platform
                    xPos += 1f; // Move to next position
                    if (xPos >= endX)
                        break; // Exit if end of platform is reached
                }
            }
            else
            {
                xPos += 1f; // Move to next position without spawning spikes
            }
        }
    }

    private void SpawnEnemies(float startX, float endX, float y)
    {
        float platformLength = (endX - startX); // Calculate the length of the platform
        if (platformLength <= 2)
            return; // Exit if the platform is too short to spawn enemies

        float centerX = startX + (endX - startX) / 2f; // Calculate the center X position of the platform
        float spawnStartX = centerX - 1f; // Starting X position for enemy spawning
        float spawnEndX = centerX + 1f; // Ending X position for enemy spawning

        spawnStartX = Mathf.Max(spawnStartX, startX + 1f); // Clamp spawn start X within platform bounds
        spawnEndX = Mathf.Min(spawnEndX, endX - 1f); // Clamp spawn end X within platform bounds

        List<GameObject> enemyPrefabs = null; // List to hold the appropriate enemy prefabs based on difficulty

        // Select the appropriate enemy prefabs based on current difficulty
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                enemyPrefabs = easyEnemyPrefabs;
                break;
            case Difficulty.Medium:
                enemyPrefabs = mediumEnemyPrefabs;
                break;
            case Difficulty.Hard:
                enemyPrefabs = hardEnemyPrefabs;
                break;
        }

        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            for (float sx = spawnStartX; sx < spawnEndX; sx += 1f) // Iterate through spawn range
            {
                if (Random.value < EnemySpawnChance) // Check if an enemy should spawn
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; // Select a random enemy prefab
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(sx, y + 1f, 0f), Quaternion.identity, transform); // Instantiate the enemy
                    generatedObjects.Add(enemy); // Add enemy to the list of generated objects
                }
            }
        }
    }

    private int GetMinimumPlatformLength()
    {
        // Determine the minimum platform length based on current difficulty
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Medium:
                return 3;
            case Difficulty.Hard:
                return 2;
            default:
                return 2;
        }
    }

    private float GetMinimumVerticalSpacing()
    {
        // Determine the minimum vertical spacing based on current difficulty
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Medium:
                return 2f;
            case Difficulty.Hard:
                return 1f;
            default:
                return 2f;
        }
    }

    private bool MomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.07367866f; // Coefficient for X^2 in the jump equation
        float b = 1.05197485f; // Coefficient for X in the jump equation
        float y = a * ground_x * ground_x + b * ground_x; // Calculate Y based on the jump equation
        return ground_y <= y; // Return true if the ground Y is within feasible jump range
    }

    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.92504437f; // Coefficient for X^2 in the jump equation
        float b = 4.32346315f; // Coefficient for X in the jump equation
        float y = a * ground_x * ground_x + b * ground_x; // Calculate Y based on the jump equation
        return ground_y <= y; // Return true if the ground Y is within feasible jump range
    }

    private bool WallJumpTest(float player_x, float ground_x, float ground_y)
    {
        if (player_x < ground_x)
        {
            ground_x = ground_x + (player_x - ground_x) * 2f; // Adjust ground X based on player position
        }

        float a = -0.19835401f; // Coefficient for X^2 in the wall jump equation
        float b = 1.45395189f; // Coefficient for X in the wall jump equation
        float y = a * ground_x * ground_x + b * ground_x; // Calculate Y based on the wall jump equation

        return ground_y <= y; // Return true if the ground Y is within feasible jump range
    }
}
