using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FreerunProcGen : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerTransform; // The player's transform

    [Header("Ground/Chunk Settings")]

    public GameObject wallPrefab;        // Prefab for walls
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

    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs; // Pool of enemies to spawn
    public float baseEnemySpawnChance = 0.1f; // Base probability of spawning an enemy per chunk
    public float maxEnemySpawnChance = 0.6f;  // Max spawn probability at max difficulty
    public float difficultyIncrementDistance = 100f; // Every 100 units traveled, difficulty increases
    public float difficultyStep = 0.05f;      // Amount to increase enemy spawn chance per difficulty increment

    [Header("Distance Tracking")]
    public float playerDistance = 0f; // Track how far the player has traveled
    private float bestDistance = 0f;   // Track best run (for leaderboard)
    
    [Header("Generation Logic")]
    public float chunkSpawnThreshold = 15f; // How close the player must get to the last chunk end before spawning a new one

    // Internal state
    private float currentEnemySpawnChance; 
    private float nextDifficultyThreshold = 0f;
    private float lastPlatformEndX = 0f;
    private float lastPlatformEndY = 0f;
    private bool playerAlive = true;
    private List<GameObject> generatedObjects = new List<GameObject>(); // List to track instantiated GameObjects
    public float startX = 0f;            // Starting X position
    public float startY = 0f;            // Starting Y position
    public float minY = -10f;            // Minimum Y position
    public float maxY = 10f;             // Maximum Y position

    float pGap = 0.3f;      // 40%
    float pJump = 0.3f;     // 40%
    float pShortJump = 0.2f; // 20%
    float pWallJump = 0.2f; // 20%

    float spikeSpawnChance = 0.0f;
    float maxSpikeSpawnChance = 0.3f;


    private void Start()
    {
        StartFreeRunMode();
    }

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

    private void SpawnSpikes(float startX, float endX, float y)
    {
        float xPos = startX;
        while (xPos < endX)
        {
            // Decide whether to spawn a triplet at this position
            if (Random.value < .9)
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

    public void StartFreeRunMode()
    {

        Vector2 endPos = CreateInitialPlatform();
        lastPlatformEndX = endPos.x;      // Initialize current X position after initial platform
        lastPlatformEndY = endPos.y;      // Initialize current Y position after initial platform
        // Initialize difficulty parameters
        currentEnemySpawnChance = baseEnemySpawnChance;
        nextDifficultyThreshold = difficultyIncrementDistance;

        endPos = CreateGap(lastPlatformEndX, lastPlatformEndY);
        lastPlatformEndX = endPos.x;
        lastPlatformEndY = endPos.y;
    }

    private void Update()
    {
        if (!playerAlive) return;

        // Update player distance traveled
        float playerX = playerTransform.position.x;
        if (playerX > playerDistance)
        {
            playerDistance = playerX;
        }

        // Check if we need to spawn another chunk
        if (playerX + chunkSpawnThreshold > lastPlatformEndX)
        {
            // Generate a new chunk starting from the last platform end position
            float r = Random.value; // random number between 0 and 1
            Vector2 endPos;

            if (r < pGap)
            {
                endPos = CreateGap(lastPlatformEndX, lastPlatformEndY);
            }
            else if (r < pGap + pJump)
            {
                endPos = CreateJump(lastPlatformEndX, lastPlatformEndY);
            }
            else if (r < pGap + pJump + pShortJump)
            {
                endPos = CreateShortJump(lastPlatformEndX, lastPlatformEndY);
            }
            else
            {
                endPos = CreateWallJumpSection(lastPlatformEndX, lastPlatformEndY);
            }

            lastPlatformEndX = endPos.x;
            lastPlatformEndY = endPos.y;

            // Update the death zone
            UpdateDeathZone();
        }

        // Check if difficulty should increase
        if (playerDistance >= nextDifficultyThreshold)
        {
            IncreaseDifficulty();
            nextDifficultyThreshold += difficultyIncrementDistance;
        }
    }

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

        int minPlatformLength = 3;                        // Get minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 5); // Randomize platform length

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

        float platformEndX = currentX;
        float platformEndY = currentY;

        // SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform
        if (spikeSpawnChance < Random.Range(0,1)){
            SpawnSpikes(currentX - (2 * 1f), currentX, currentY); // Adjusted multiplier to 1f
        }
        SpawnEnemies(currentX - (2 * 1f), currentX, currentY); // Adjusted multiplier to 1f

        // if (currentDifficulty != Difficulty.Easy)
        // {
        //     // Spawn spikes on the platform for Medium and Hard difficulties
        //     SpawnSpikes(platformStartX, platformEndX, currentY);
        // }

        return new Vector2(platformEndX, platformEndY); // Return the new position after the platform
    }

     // Creates a jump between platforms
    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x;
        float deltaY = Random.Range(-1, 2) * 2f; // Randomly decide to jump up or down
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

        int minPlatformLength = 3;                        // Get minimum platform length
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3); // Randomize platform length

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

        // SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform

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
            float deltaY = Random.Range(-1, 1) * 2f;
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y position within limits

            // Ensure sufficient vertical spacing
            if (Mathf.Abs(nextY - currentY) < 2f)
            {
                deltaY = 2f * Mathf.Sign(deltaY);
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

            int minPlatformLength = 2;                        // Get minimum platform length
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

            // SpawnEnemies(platformStartX, platformEndX, currentY); // Spawn enemies on the platform

            // if (currentDifficulty != Difficulty.Easy)
            // {
            //     // Spawn spikes on the platform for Medium and Hard difficulties
            //     SpawnSpikes(platformStartX, platformEndX, currentY);
            // }
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
        int exitPlatformLength = 3;                           // Number of tiles in the exit platform

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

        // SpawnEnemies(currentX - (exitPlatformLength * 1f), currentX, currentY); // Adjusted multiplier to 1f
        // SpawnSpikes(currentX - (exitPlatformLength * 1f), currentX, currentY); // Adjusted multiplier to 1f

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

        // Ensure there are enemy prefabs available
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            // Iterate through the spawn range with a step of 1f (tile spacing)
            for (float x = spawnStartX; x < spawnEndX; x += 1f)
            {
                if (Random.value < currentEnemySpawnChance) // Check if an enemy should spawn at this position
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)]; // Select a random enemy prefab
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(x, y + 1f, 0f), Quaternion.identity, transform); // Instantiate the enemy
                    generatedObjects.Add(enemy); // Track the instantiated enemy for cleanup
                }
            }
        }
    }

    private void IncreaseDifficulty()
    {
        // Increase the enemy spawn chance up to a max cap
        currentEnemySpawnChance = Mathf.Min(currentEnemySpawnChance + difficultyStep, maxEnemySpawnChance);
        spikeSpawnChance = Mathf.Min(spikeSpawnChance + difficultyStep, maxSpikeSpawnChance);
        Debug.Log($"Difficulty increased. Enemy Spawn Chance: {currentEnemySpawnChance}, Spike Spawn Chance: {spikeSpawnChance}, Distance: {playerDistance}");
    }

    public void OnPlayerDeath()
    {
        playerAlive = false;
        // Save best run distance if it's a record
        float previousBest = PlayerPrefs.GetFloat("BestDistance", 0f);
        if (playerDistance > previousBest)
        {
            PlayerPrefs.SetFloat("BestDistance", playerDistance);
            PlayerPrefs.Save();
            Debug.Log("New Best Distance: " + playerDistance);
        }
        else
        {
            Debug.Log("Distance: " + playerDistance + ", Best: " + previousBest);
        }

        // You can reset the game or load a menu here if you want
        // e.g. SceneManager.LoadScene("MainMenu");
    }

    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        // Constants for the no momentum jump curve
        float a = -0.92504437f;
        float b = 4.32346315f;

        // Calculate predicted y based on ground_x
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y; // Return true if the ground_y is below the curve
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

    private void UpdateDeathZone()
    {
        if (deathZonePrefab != null)
        {
            // Position the death zone to cover the level up to 10 units past the last platform
            float deathZoneLength = lastPlatformEndX + 10f; // Stop 10 units past the last platform
            float deathZoneX = deathZoneLength / 2f; // Center the death zone horizontally

            // Check if the death zone exists; if not, instantiate it
            if (GameObject.Find("DeathZone") == null)
            {
                GameObject deathZone = Instantiate(deathZonePrefab, new Vector3(deathZoneX, minY - 4f, 0f), Quaternion.identity);
                deathZone.name = "DeathZone";
            }
            else
            {
                GameObject deathZone = GameObject.Find("DeathZone");
                deathZone.transform.position = new Vector3(deathZoneX, minY - 4f, 0f); // Adjust the position
                deathZone.transform.localScale = new Vector3(deathZoneLength, 2f, 1f); // Adjust the scale
            }
        }
    }

}

