// FreeRun.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 12/05/2024
// Course: EECS 581
// Purpose: FreeRun mode generator with Dynamic Difficulty Adjustment

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class FreerunProcGen : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform playerTransform; // The player's transform
    public Transform player2Transform; // The player's transform

    [Header("Ground/Chunk Settings")]
    public GameObject wallPrefab;        // Prefab for walls
    public GameObject deathZonePrefab;   // Prefab for the death zone area

    // Tilemap references
    public Tilemap groundTilemap;        // Assign Ground Tilemap in Inspector
    public Tilemap  wallTilemap;        // Tilemap for walls
    public Tilemap hazardTilemap;        // Assign Hazard Tilemap in Inspector

    // Tile references for ground
    public TileBase leftTile;            // Tile for the left end of a platform
    public TileBase centerTile;          // Tile for the center of a platform
    public TileBase rightTile;           // Tile for the right end of a platform

    public TileBase leftBottomWallTile;           // Tile for the left end of the wall tile
    public TileBase rightBottomWallTile;           // Tile for the right end of the wall tile
    public TileBase leftTopWallTile;           // Tile for the left end of the wall tile
    public TileBase rightTopWallTile;           // Tile for the right end of the wall tile

    // Tile reference for hazards
    public TileBase spikeTile;           // Spike/hazard tile

    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs; 
    public float baseEnemySpawnChance = 0.0f; 
    public float maxEnemySpawnChance = 0.6f;  
    public float difficultyIncrementDistance = 100f; 
    public float enemyDifficultyStep = 0.05f;
    public float spikeDifficultyStep = 0.01f;

    [Header("Distance Tracking")]
    public float playerDistance = 0f; 

    [Header("Generation Logic")]
    public float chunkSpawnThreshold = 15f; 

    // Internal state
    private float currentEnemySpawnChance; 
    private float nextDifficultyThreshold = 0f;
    private float lastPlatformEndX = 0f;
    private float lastPlatformEndY = 0f;

    public float startX = 0f;            
    public float startY = 0f;            
    public float minY = -10f;            
    public float maxY = 20f;             

    float pGap = 0.3f;       
    float pJump = 0.3f;      
    float pShortJump = 0.2f; 

    float spikeSpawnChance = 0.0f;
    float maxSpikeSpawnChance = 0.2f;

    [System.Serializable]
    public class GeneratedChunk {
        public float startX;
        public float endX;
        public List<Vector3Int> tilePositions = new List<Vector3Int>();
        public List<GameObject> spawnedObjects = new List<GameObject>();
    }

    private List<GeneratedChunk> generatedChunks = new List<GeneratedChunk>();

    public float cleanupDistance = 50f;

    public TMPro.TextMeshProUGUI distanceText;

    private void Start()
    {
        StartFreeRunMode(); // Initialize FreeRun mode
    }

    private Vector2 CreateInitialPlatform()
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = 0f;

        float currentX = 0f; // Current X position
        float currentY = 0f; // Current Y position

        AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
        currentX += 1f; // Move to next position

        AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, centerTile); // Add another center tile
        currentX += 1f; // Move to next position

        AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
        float finalX = currentX; // Final X position
        float finalY = currentY; // Final Y position

        currentX += 1f; // Move to next position

        chunk.endX = finalX; // Set end X of chunk
        generatedChunks.Add(chunk); // Add chunk to list

        return new Vector2(finalX, finalY); // Return end position
    }

    private void AddGroundTile(GeneratedChunk chunk, float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert to tile position
        groundTilemap.SetTile(tilePos, tile); // Set tile in ground tilemap
        chunk.tilePositions.Add(tilePos); // Add tile position to chunk
    }

    private void AddWallTile(GeneratedChunk chunk, float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert world position to tile position
        wallTilemap.SetTile(tilePos, tile); // Set the ground tile at the specified position
        chunk.tilePositions.Add(tilePos); // Add tile position to chunk
    }

    private void AddSpikeTile(GeneratedChunk chunk, float x, float y)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y); // Convert to tile position
        hazardTilemap.SetTile(tilePos, spikeTile); // Set spike tile in hazard tilemap
        chunk.tilePositions.Add(tilePos); // Add tile position to chunk
    }

    private Vector3Int WorldToTilePosition(float x, float y)
    {
        return groundTilemap.WorldToCell(new Vector3(x, y, 0f)); // Convert world position to tile position
    }

    private void SpawnSpikes(GeneratedChunk chunk, float startX, float endX, float y)
    {
        float xPos = startX; // Starting X position
        while (xPos < endX)
        {
            if (UnityEngine.Random.value < .9f) // 90% chance to spawn spikes
            {
                for (int i = 0; i < 3; i++) // Spawn 3 spikes
                {
                    AddSpikeTile(chunk, xPos, y + 1f); // Add spike tile
                    xPos += 1f; // Move to next position
                    if (xPos >= endX)
                        break; // Exit if end reached
                }
            }
            else
            {
                xPos += 1f; // Move to next position without spawning spikes
            }
        }
    }

    public void StartFreeRunMode()
    {
        Vector2 endPos = CreateInitialPlatform(); // Create the initial platform
        lastPlatformEndX = endPos.x; // Set last platform end X
        lastPlatformEndY = endPos.y; // Set last platform end Y
        currentEnemySpawnChance = baseEnemySpawnChance; // Initialize enemy spawn chance
        nextDifficultyThreshold = difficultyIncrementDistance; // Set next difficulty threshold

        endPos = CreateGap(lastPlatformEndX, lastPlatformEndY); // Create first gap
        lastPlatformEndX = endPos.x; // Update last platform end X
        lastPlatformEndY = endPos.y; // Update last platform end Y
    }

    private void CleanupBehindPlayer() {

        float playerX;

        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

        if (player2 != null && player2.activeInHierarchy == true){
            playerX = Mathf.Min(playerTransform.position.x, player2Transform.position.x); // Get player's X position
        } else {
            playerX = playerTransform.position.x;
        }

        for (int i = generatedChunks.Count - 1; i >= 0; i--) { // Iterate through chunks backwards
            GeneratedChunk chunk = generatedChunks[i];
            if (chunk.endX < playerX - cleanupDistance) { // Check if chunk is behind cleanup distance
                
                // Remove tiles from tilemaps
                foreach (var tilePos in chunk.tilePositions) {
                    groundTilemap.SetTile(tilePos, null); // Remove ground tile
                    hazardTilemap.SetTile(tilePos, null); // Remove hazard tile
                    wallTilemap.SetTile(tilePos, null); // Remove ground tile
                }

                // Destroy spawned objects like enemies and walls
                foreach (var obj in chunk.spawnedObjects) {
                    if (obj != null) {
                        Destroy(obj); // Destroy object
                    }
                }

                // Remove the chunk from the list
                generatedChunks.RemoveAt(i); // Remove chunk
            }
        }
    }

    private void Update()
    {
        // if (!playerAlive) return; // Exit if player is not alive

        float playerX = Mathf.Max(playerTransform.position.x, player2Transform.position.x);

        if (playerX > playerDistance)
        {
            playerDistance = playerX; // Update player distance
        }

        float previousBest = PlayerPrefs.GetFloat("BestDistance", 0f); // Get previous best distance
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {playerDistance:F2}\nBest: {previousBest:F2}"; // Update distance UI
        }


        if (playerX + chunkSpawnThreshold > lastPlatformEndX) // Check if new chunk needs to be spawned
        {
            Vector2 endPos;
            bool nearMaxHeight = lastPlatformEndY >= maxY - 1f; // Check if near maximum height

            float r = UnityEngine.Random.value; // Random value for decision
            if (nearMaxHeight)
            {
                if (UnityEngine.Random.value < 0.5f)
                    endPos = CreateWallDownJumpSection(lastPlatformEndX, lastPlatformEndY); // Create wall down jump section
                else
                    endPos = CreateDownJumpSection(lastPlatformEndX, lastPlatformEndY); // Create down jump section
            }
            else
            {
                if (r < pGap)
                {
                    endPos = CreateGap(lastPlatformEndX, lastPlatformEndY); // Create gap
                }
                else if (r < pGap + pJump)
                {
                    endPos = CreateJump(lastPlatformEndX, lastPlatformEndY); // Create jump
                }
                else if (r < pGap + pJump + pShortJump)
                {
                    endPos = CreateShortJump(lastPlatformEndX, lastPlatformEndY); // Create short jump
                }
                else
                {
                    endPos = CreateWallJumpSection(lastPlatformEndX, lastPlatformEndY); // Create wall jump section
                }
            }

            lastPlatformEndX = endPos.x; // Update last platform end X
            lastPlatformEndY = endPos.y; // Update last platform end Y

            UpdateDeathZone(); // Update death zone position
        }

        if (playerDistance >= nextDifficultyThreshold) // Check if difficulty should increase
        {
            IncreaseDifficulty(); // Increase game difficulty
            nextDifficultyThreshold += difficultyIncrementDistance; // Set next difficulty threshold
        }

        CleanupBehindPlayer(); // Clean up chunks behind the player
    }

    private Vector2 CreateGap(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        float gapSize = UnityEngine.Random.Range(5f, 8f); // Random gap size

        while (!NoMomentumJumpTest(gapSize, 0f)) // Ensure the gap is jumpable
        {
            gapSize -= 0.5f; // Decrease gap size
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move past the gap
        float platformStartX = currentX; // Start X for the next platform

        int minPlatformLength = 3; // Minimum platform length
        int platformLength = UnityEngine.Random.Range(minPlatformLength, minPlatformLength + 5); // Random platform length

        for (int i = 0; i < platformLength; i++) // Create platform tiles
        {
            if (i == 0)
                AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
            else
                AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile

            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position
        float finalY = currentY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk

        if (UnityEngine.Random.value < spikeSpawnChance) // Chance to spawn spikes
        {
            SpawnSpikes(chunk, finalX - 2f, finalX, finalY); // Spawn spikes near end
        }

        SpawnEnemies(chunk, platformStartX, currentX, currentY); // Spawn enemies on platform

        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }

    private Vector2 CreateJump(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float deltaY = UnityEngine.Random.Range(-1, 2) * 2f; // Random Y delta
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY); // Clamp Y within bounds

        float gapSize = UnityEngine.Random.Range(4f, 6f); // Random gap size

        while (!MomentumJumpTest(gapSize, currentY - y)) // Ensure the jump is feasible
        {
            gapSize -= 0.5f; // Decrease gap size
            if (gapSize <= 0f)
            {
                gapSize = 1f; // Minimum gap size
                break;
            }
        }

        currentX += gapSize; // Move past the gap

        int minPlatformLength = 3; // Minimum platform length
        int platformLength = UnityEngine.Random.Range(minPlatformLength, minPlatformLength + 3); // Random platform length

        float platformStartX = currentX; // Start X for the next platform

        for (int i = 0; i < platformLength; i++) // Create elevated platform tiles
        {
            if (i == 0)
                AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
            else
                AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile

            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position
        float finalY = currentY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk

        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }

    private Vector2 CreateShortJump(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        int numPlatforms = UnityEngine.Random.Range(2, 4); // Number of small platforms

        for (int i = 0; i < numPlatforms; i++) // Create multiple small platforms
        {
            float gapSize = 2f; // Fixed small gap size
            float deltaY = UnityEngine.Random.Range(-1, 1) * 2f; // Random Y delta
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds

            if (Mathf.Abs(nextY - currentY) < 2f) // Ensure sufficient vertical change
            {
                deltaY = 2f * Mathf.Sign(deltaY); // Adjust Y delta
                nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds
            }

            while (!NoMomentumJumpTest(gapSize, nextY - currentY)) // Ensure the jump is feasible
            {
                gapSize -= 0.5f; // Decrease gap size
                if (gapSize <= 0f)
                {
                    gapSize = 1f; // Minimum gap size
                    break;
                }
            }

            currentX += gapSize; // Move past the gap
            currentY = nextY; // Update Y position

            int minPlatformLength = 2; // Minimum platform length
            int platformLength = UnityEngine.Random.Range(minPlatformLength, minPlatformLength + 2); // Random platform length

            for (int j = 0; j < platformLength; j++) // Create platform tiles
            {
                if (j == 0)
                    AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
                else if (j == platformLength - 1)
                    AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
                else
                    AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile

                currentX += 1f; // Move to next position
            }
        }

        float finalX = currentX - 1f; // Final X position
        float finalY = currentY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk

        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }

    private Vector2 CreateWallJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        // Entry platform (5 tiles)
        AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
        currentX += 1f; // Move to next position

        for (int i=0; i < 10; i++) {
            // Create walls as obstacles
            AddWallTile(chunk, currentX - 2f, currentY + i + 3f, new TileBase[] { leftBottomWallTile, leftTopWallTile }[UnityEngine.Random.Range(0, 2)]); // Create first wall
            AddWallTile(chunk, currentX - 1f, currentY + i + 3f, new TileBase[] { rightBottomWallTile, rightTopWallTile }[UnityEngine.Random.Range(0, 2)]);
        }

        float wallGap = 4f; // Gap between walls
        
        for (int i=0; i < 10; i++) {
            // Create walls as obstacles
            AddWallTile(chunk, currentX + wallGap, currentY + i, new TileBase[] { leftBottomWallTile, leftTopWallTile }[UnityEngine.Random.Range(0, 2)]); // Create first wall
            AddWallTile(chunk, currentX + wallGap+ 1f, currentY + i, new TileBase[] { rightBottomWallTile, rightTopWallTile }[UnityEngine.Random.Range(0, 2)]);
        }

        // Exit platform
        float wallHeight = 9f; // Get wall height
        float exitPlatformY = currentY + wallHeight; // Set exit platform Y position
        int exitPlatformLength = 3; // Exit platform length

        float exitPlatformX = currentX + wallGap + 2f; // Exit platform X position

        for (int i = 0; i < exitPlatformLength; i++) // Create exit platform tiles
        {
            if (i == 0)
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, leftTile); // Add left tile
            else if (i == exitPlatformLength - 1)
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, rightTile); // Add right tile
            else
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, centerTile); // Add center tile

            exitPlatformX += 1f; // Move to next position
        }

        float finalX = exitPlatformX - 1f; // Final X position
        float finalY = exitPlatformY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk

        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }


    private Vector2 CreateDownJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        int numDownJumps = UnityEngine.Random.Range(2, 4); // Number of down jumps
        for (int i = 0; i < numDownJumps; i++) // Create down jump platforms
        {
            float gapSize = UnityEngine.Random.Range(3f, 6f); // Random gap size
            float deltaY = UnityEngine.Random.Range(-3, -1) * 2f; // Random Y delta for descending
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds

            while (!MomentumJumpTest(gapSize, nextY - currentY)) // Ensure the jump is feasible
            {
                gapSize -= 0.5f; // Decrease gap size
                if (gapSize < 1f) gapSize = 1f; // Minimum gap size
            }

            currentX += gapSize; // Move past the gap
            currentY = nextY; // Update Y position

            int platformLength = UnityEngine.Random.Range(2, 4); // Random platform length
            for (int j = 0; j < platformLength; j++) // Create platform tiles
            {
                if (j == 0)
                    AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
                else if (j == platformLength - 1)
                    AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
                else
                    AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile

                currentX += 1f; // Move to next position
            }
        }

        float finalX = currentX - 1f; // Final X position
        float finalY = currentY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk
        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }

    private Vector2 CreateWallDownJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk(); // Create a new chunk
        chunk.startX = x;

        float currentX = x; // Current X position
        float currentY = y; // Current Y position

        // Entry platform
        AddGroundTile(chunk, currentX, currentY, leftTile); // Add left tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, centerTile); // Add center tile
        currentX += 1f; // Move to next position
        AddGroundTile(chunk, currentX, currentY, rightTile); // Add right tile
        currentX += 1f; // Move to next position

        for (int i=0; i < 10; i++) {
            // Create walls as obstacles
            AddWallTile(chunk, currentX, currentY - i, new TileBase[] { leftBottomWallTile, leftTopWallTile }[UnityEngine.Random.Range(0, 2)]); // Create first wall
            AddWallTile(chunk, currentX+1f, currentY - i, new TileBase[] { rightBottomWallTile, rightTopWallTile }[UnityEngine.Random.Range(0, 2)]);
        }

        float wallGap = 6f; // Gap between walls
        currentX += wallGap; // Move to next position
        
        for (int i=0; i < 10; i++) {
            // Create walls as obstacles
            AddWallTile(chunk, currentX, currentY - i+2f, new TileBase[] { leftBottomWallTile, leftTopWallTile }[UnityEngine.Random.Range(0, 2)]); // Create first wall
            AddWallTile(chunk, currentX + 1f, currentY - i+2f, new TileBase[] { rightBottomWallTile, rightTopWallTile }[UnityEngine.Random.Range(0, 2)]);
        }

        float deltaY = UnityEngine.Random.Range(-4, -3) * 2f; // Random Y delta for descending
        float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY); // Clamp Y within bounds

        while (!MomentumJumpTest(wallGap, nextY - currentY)) // Ensure the jump is feasible
        {
            wallGap -= 0.5f; // Decrease gap size
            if (wallGap < 1f) wallGap = 1f; // Minimum gap size
        }

        currentY = nextY; // Update Y position

        // Exit platform after walls
        int platformLength = 4; // Exit platform length
        for (int i = 0; i < platformLength; i++) // Create exit platform tiles
        {
            if (i == 0)
                AddGroundTile(chunk, currentX + 1f, currentY-4f, leftTile); // Add left tile
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX + 1f, currentY-4f, rightTile); // Add right tile
            else
                AddGroundTile(chunk, currentX + 1f, currentY-4f, centerTile); // Add center tile

            currentX += 1f; // Move to next position
        }

        float finalX = currentX - 1f; // Final X position
        float finalY = currentY; // Final Y position
        chunk.endX = finalX; // Set end X of chunk

        generatedChunks.Add(chunk); // Add chunk to list
        return new Vector2(finalX, finalY); // Return end position
    }

    // private GameObject CreateWall(float x, float startY)
    // {
    //     if (wallPrefab != null)
    //     {
    //         GameObject wall = Instantiate(wallPrefab, new Vector3(x, startY, 0f), Quaternion.identity, transform); // Instantiate wall
    //         return wall; // Return wall object
    //     }
    //     return null; // Return null if prefab not set
    // }

    private void SpawnEnemies(GeneratedChunk chunk, float startX, float endX, float y)
    {
        float platformLength = (endX - startX); // Calculate platform length
        if (platformLength <= 2)
            return; // Exit if platform too short

        float centerX = startX + (endX - startX) / 2f; // Calculate center X
        float spawnStartX = centerX - 1f; // Start X for spawning enemies
        float spawnEndX = centerX + 1f; // End X for spawning enemies

        spawnStartX = Mathf.Max(spawnStartX, startX + 1f); // Clamp spawn start X
        spawnEndX = Mathf.Min(spawnEndX, endX - 1f); // Clamp spawn end X

        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            for (float sx = spawnStartX; sx < spawnEndX; sx += 1f) // Iterate through spawn range
            {
                if (UnityEngine.Random.value < currentEnemySpawnChance) // Check spawn chance
                {
                    GameObject enemyPrefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)]; // Select random enemy prefab
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(sx, y + 1f, 0f), Quaternion.identity, transform); // Instantiate enemy
                    chunk.spawnedObjects.Add(enemy); // Add enemy to chunk
                }
            }
        }
    }

    private void IncreaseDifficulty()
    {
        currentEnemySpawnChance = Mathf.Min(currentEnemySpawnChance + enemyDifficultyStep, maxEnemySpawnChance); // Increase enemy spawn chance
        spikeSpawnChance = Mathf.Min(spikeSpawnChance + spikeDifficultyStep, maxSpikeSpawnChance); // Increase spike spawn chance
        Debug.Log($"Difficulty increased. Enemy Spawn Chance: {currentEnemySpawnChance}, Spike Spawn Chance: {spikeSpawnChance}, Distance: {playerDistance}"); // Log difficulty increase
    }


    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.92504437f; // Coefficient for X^2
        float b = 4.32346315f; // Coefficient for X
        float y = a * ground_x * ground_x + b * ground_x; // Calculate Y based on formula
        return ground_y <= y; // Return if ground Y is within feasible jump
    }

    private bool MomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.07367866f; // Coefficient for X^2
        float b = 1.05197485f; // Coefficient for X
        float y = a * ground_x * ground_x + b * ground_x; // Calculate Y based on formula
        return ground_y <= y; // Return if ground Y is within feasible jump
    }

    private void UpdateDeathZone()
    {
        if (deathZonePrefab != null)
        {
            float deathZoneLength = lastPlatformEndX + 10f; // Calculate death zone length
            float deathZoneX = deathZoneLength / 2f; // Calculate death zone X position

            GameObject deathZone = GameObject.Find("DeathZone"); // Find existing death zone
            if (deathZone == null)
            {
                deathZone = Instantiate(deathZonePrefab, new Vector3(deathZoneX, minY - 4f, 0f), Quaternion.identity); // Instantiate new death zone
                deathZone.name = "DeathZone"; // Set name
            }
            else
            {
                deathZone.transform.position = new Vector3(deathZoneX, minY - 4f, 0f); // Update position
                deathZone.transform.localScale = new Vector3(deathZoneLength, 2f, 1f); // Update scale
            }
        }
    }

}
