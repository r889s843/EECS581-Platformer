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
    public List<GameObject> enemyPrefabs; 
    public float baseEnemySpawnChance = 0.1f; 
    public float maxEnemySpawnChance = 0.6f;  
    public float difficultyIncrementDistance = 100f; 
    public float difficultyStep = 0.05f;      

    [Header("Distance Tracking")]
    public float playerDistance = 0f; 
    private float bestDistance = 0f; 
    
    [Header("Generation Logic")]
    public float chunkSpawnThreshold = 15f; 

    // Internal state
    private float currentEnemySpawnChance; 
    private float nextDifficultyThreshold = 0f;
    private float lastPlatformEndX = 0f;
    private float lastPlatformEndY = 0f;
    private bool playerAlive = true;

    public float startX = 0f;            
    public float startY = 0f;            
    public float minY = -10f;            
    public float maxY = 20f;             

    float pGap = 0.3f;       
    float pJump = 0.3f;      
    float pShortJump = 0.2f; 
    float pWallJump = 0.2f;  

    float spikeSpawnChance = 0.0f;
    float maxSpikeSpawnChance = 0.3f;

    [System.Serializable]
    public class GeneratedChunk {
        public float startX;
        public float endX;
        public List<Vector3Int> tilePositions = new List<Vector3Int>();
        public List<GameObject> spawnedObjects = new List<GameObject>();
    }

    private List<GeneratedChunk> generatedChunks = new List<GeneratedChunk>();

    public float cleanupDistance = 50f; 

    private void Start()
    {
        StartFreeRunMode();
    }

    private Vector2 CreateInitialPlatform()
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = 0f;

        float currentX = 0f;
        float currentY = 0f;

        AddGroundTile(chunk, currentX, currentY, leftTile);
        currentX += 1f;

        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;

        AddGroundTile(chunk, currentX, currentY, rightTile);
        float finalX = currentX;
        float finalY = currentY;

        currentX += 1f;

        chunk.endX = finalX;
        generatedChunks.Add(chunk);

        return new Vector2(finalX, finalY); 
    }

    private void AddGroundTile(GeneratedChunk chunk, float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y);
        groundTilemap.SetTile(tilePos, tile);
        chunk.tilePositions.Add(tilePos);
    }

    private void AddSpikeTile(GeneratedChunk chunk, float x, float y)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y);
        hazardTilemap.SetTile(tilePos, spikeTile);
        chunk.tilePositions.Add(tilePos);
    }

    private Vector3Int WorldToTilePosition(float x, float y)
    {
        return groundTilemap.WorldToCell(new Vector3(x, y, 0f));
    }

    private void SpawnSpikes(GeneratedChunk chunk, float startX, float endX, float y)
    {
        float xPos = startX;
        while (xPos < endX)
        {
            if (Random.value < .9f)
            {
                for (int i = 0; i < 3; i++)
                {
                    AddSpikeTile(chunk, xPos, y + 1f);
                    xPos += 1f;
                    if (xPos >= endX)
                        break;
                }
            }
            else
            {
                xPos += 1f;
            }
        }
    }

    public void StartFreeRunMode()
    {
        Vector2 endPos = CreateInitialPlatform();
        lastPlatformEndX = endPos.x; 
        lastPlatformEndY = endPos.y; 
        currentEnemySpawnChance = baseEnemySpawnChance;
        nextDifficultyThreshold = difficultyIncrementDistance;

        endPos = CreateGap(lastPlatformEndX, lastPlatformEndY);
        lastPlatformEndX = endPos.x;
        lastPlatformEndY = endPos.y;
    }

    private void CleanupBehindPlayer() {
        float playerX = playerTransform.position.x;
        
        for (int i = generatedChunks.Count - 1; i >= 0; i--) {
            GeneratedChunk chunk = generatedChunks[i];
            if (chunk.endX < playerX - cleanupDistance) {
                
                // Remove tiles
                foreach (var tilePos in chunk.tilePositions) {
                    groundTilemap.SetTile(tilePos, null);
                    hazardTilemap.SetTile(tilePos, null);
                }

                // Destroy objects
                foreach (var obj in chunk.spawnedObjects) {
                    if (obj != null) {
                        Destroy(obj);
                    }
                }

                // Remove chunk
                generatedChunks.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        if (!playerAlive) return;

        float playerX = playerTransform.position.x;
        if (playerX > playerDistance)
        {
            playerDistance = playerX;
        }

        if (playerX + chunkSpawnThreshold > lastPlatformEndX)
        {
            Vector2 endPos;
            bool nearMaxHeight = lastPlatformEndY >= maxY - 1f;

            float r = Random.value; 
            if (nearMaxHeight)
            {
                if (Random.value < 0.5f)
                    endPos = CreateWallDownJumpSection(lastPlatformEndX, lastPlatformEndY);
                else
                    endPos = CreateDownJumpSection(lastPlatformEndX, lastPlatformEndY);
            }
            else
            {
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
            }

            lastPlatformEndX = endPos.x;
            lastPlatformEndY = endPos.y;

            UpdateDeathZone();
        }

        if (playerDistance >= nextDifficultyThreshold)
        {
            IncreaseDifficulty();
            nextDifficultyThreshold += difficultyIncrementDistance;
        }

        // Call cleanup every frame (or at some interval)
        CleanupBehindPlayer();
    }

    private Vector2 CreateGap(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float currentY = y;

        float gapSize = Random.Range(5f, 8f);

        while (!NoMomentumJumpTest(gapSize, 0f))
        {
            gapSize -= 0.5f;
            if (gapSize <= 0f)
            {
                gapSize = 1f;
                break;
            }
        }

        currentX += gapSize; 
        float platformStartX = currentX;

        int minPlatformLength = 3;
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 5);

        for (int i = 0; i < platformLength; i++)
        {
            if (i == 0)
                AddGroundTile(chunk, currentX, currentY, leftTile);
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX, currentY, rightTile);
            else
                AddGroundTile(chunk, currentX, currentY, centerTile);

            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        chunk.endX = finalX;

        if (spikeSpawnChance < Random.value)
        {
            // Spawn spikes near the end of platform (example)
            SpawnSpikes(chunk, finalX - 2f, finalX, finalY);
        }

        SpawnEnemies(chunk, platformStartX, currentX, currentY);

        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateJump(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float deltaY = Random.Range(-1, 2) * 2f;
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY);

        float gapSize = Random.Range(4f, 6f);

        while (!MomentumJumpTest(gapSize, currentY - y))
        {
            gapSize -= 0.5f;
            if (gapSize <= 0f)
            {
                gapSize = 1f;
                break;
            }
        }

        currentX += gapSize; 

        int minPlatformLength = 3;
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3);

        float platformStartX = currentX;

        for (int i = 0; i < platformLength; i++)
        {
            if (i == 0)
                AddGroundTile(chunk, currentX, currentY, leftTile);
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX, currentY, rightTile);
            else
                AddGroundTile(chunk, currentX, currentY, centerTile);

            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        chunk.endX = finalX;

        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateShortJump(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float currentY = y;

        int numPlatforms = Random.Range(2, 4); 

        for (int i = 0; i < numPlatforms; i++)
        {
            float gapSize = 2f; 
            float deltaY = Random.Range(-1, 1) * 2f;
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

            if (Mathf.Abs(nextY - currentY) < 2f)
            {
                deltaY = 2f * Mathf.Sign(deltaY);
                nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);
            }

            while (!NoMomentumJumpTest(gapSize, nextY - currentY))
            {
                gapSize -= 0.5f;
                if (gapSize <= 0f)
                {
                    gapSize = 1f;
                    break;
                }
            }

            currentX += gapSize; 
            currentY = nextY;

            int minPlatformLength = 2;
            int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2);

            for (int j = 0; j < platformLength; j++)
            {
                if (j == 0)
                    AddGroundTile(chunk, currentX, currentY, leftTile);
                else if (j == platformLength - 1)
                    AddGroundTile(chunk, currentX, currentY, rightTile);
                else
                    AddGroundTile(chunk, currentX, currentY, centerTile);

                currentX += 1f;
            }
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        chunk.endX = finalX;

        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateWallJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float currentY = y;

        // Entry platform (5 tiles)
        AddGroundTile(chunk, currentX, currentY, leftTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, rightTile);
        currentX += 1f;

        // Create walls
        GameObject w1 = CreateWall(currentX - 3f, currentY + 7f);
        if (w1 != null) chunk.spawnedObjects.Add(w1);

        float wallGap = 6f;
        GameObject w2 = CreateWall(currentX - 4f + wallGap, currentY + 5f);
        if (w2 != null) chunk.spawnedObjects.Add(w2);

        // Exit platform
        float wallHeight = wallPrefab.GetComponent<Renderer>().bounds.size.y;
        float exitPlatformY = currentY + wallHeight;
        int exitPlatformLength = 3;

        float exitPlatformX = currentX - 4f + wallGap + 1f;

        for (int i = 0; i < exitPlatformLength; i++)
        {
            if (i == 0)
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, leftTile);
            else if (i == exitPlatformLength - 1)
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, rightTile);
            else
                AddGroundTile(chunk, exitPlatformX, exitPlatformY, centerTile);

            exitPlatformX += 1f;
        }

        float finalX = exitPlatformX - 1f;
        float finalY = exitPlatformY;
        chunk.endX = finalX;

        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateDownJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float currentY = y;

        int numDownJumps = Random.Range(2, 4);
        for (int i = 0; i < numDownJumps; i++)
        {
            float gapSize = Random.Range(3f, 6f);
            float deltaY = Random.Range(-3, -1) * 2f;
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

            while (!MomentumJumpTest(gapSize, nextY - currentY))
            {
                gapSize -= 0.5f;
                if (gapSize < 1f) gapSize = 1f;
            }

            currentX += gapSize;
            currentY = nextY;

            int platformLength = Random.Range(2, 4);
            for (int j = 0; j < platformLength; j++)
            {
                if (j == 0)
                    AddGroundTile(chunk, currentX, currentY, leftTile);
                else if (j == platformLength - 1)
                    AddGroundTile(chunk, currentX, currentY, rightTile);
                else
                    AddGroundTile(chunk, currentX, currentY, centerTile);

                currentX += 1f;
            }
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        chunk.endX = finalX;
        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateWallDownJumpSection(float x, float y)
    {
        GeneratedChunk chunk = new GeneratedChunk();
        chunk.startX = x;

        float currentX = x;
        float currentY = y;

        // Entry platform
        AddGroundTile(chunk, currentX, currentY, leftTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, centerTile);
        currentX += 1f;
        AddGroundTile(chunk, currentX, currentY, rightTile);
        currentX += 1f;

        // Place walls
        GameObject w1 = CreateWall(currentX - 1f, currentY - 5f); 
        if (w1 != null) chunk.spawnedObjects.Add(w1);

        float wallGap = 5f;
        GameObject w2 = CreateWall(currentX + wallGap, currentY + 1f);
        if (w2 != null) chunk.spawnedObjects.Add(w2);

        float deltaY = Random.Range(-4, -3) * 2f;
        float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

        while (!MomentumJumpTest(wallGap, nextY - currentY))
        {
            wallGap -= 0.5f;
            if (wallGap < 1f) wallGap = 1f;
        }

        currentX += wallGap;
        currentY = nextY;

        // Exit platform
        int platformLength = 3;
        for (int i = 0; i < platformLength; i++)
        {
            if (i == 0)
                AddGroundTile(chunk, currentX, currentY, leftTile);
            else if (i == platformLength - 1)
                AddGroundTile(chunk, currentX, currentY, rightTile);
            else
                AddGroundTile(chunk, currentX, currentY, centerTile);

            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        chunk.endX = finalX;

        generatedChunks.Add(chunk);
        return new Vector2(finalX, finalY);
    }

    private GameObject CreateWall(float x, float startY)
    {
        if (wallPrefab != null)
        {
            GameObject wall = Instantiate(wallPrefab, new Vector3(x, startY, 0f), Quaternion.identity, transform);
            return wall;
        }
        return null;
    }

    private void SpawnEnemies(GeneratedChunk chunk, float startX, float endX, float y)
    {
        float platformLength = (endX - startX);
        if (platformLength <= 2)
            return;

        float centerX = startX + (endX - startX) / 2f;
        float spawnStartX = centerX - 1f; 
        float spawnEndX = centerX + 1f;

        spawnStartX = Mathf.Max(spawnStartX, startX + 1f);
        spawnEndX = Mathf.Min(spawnEndX, endX - 1f);

        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            for (float sx = spawnStartX; sx < spawnEndX; sx += 1f)
            {
                if (Random.value < currentEnemySpawnChance)
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(sx, y + 1f, 0f), Quaternion.identity, transform);
                    chunk.spawnedObjects.Add(enemy);
                }
            }
        }
    }

    private void IncreaseDifficulty()
    {
        currentEnemySpawnChance = Mathf.Min(currentEnemySpawnChance + difficultyStep, maxEnemySpawnChance);
        spikeSpawnChance = Mathf.Min(spikeSpawnChance + difficultyStep, maxSpikeSpawnChance);
        Debug.Log($"Difficulty increased. Enemy Spawn Chance: {currentEnemySpawnChance}, Spike Spawn Chance: {spikeSpawnChance}, Distance: {playerDistance}");
    }

    public void OnPlayerDeath()
    {
        playerAlive = false;
        float previousBest = PlayerPrefs.GetFloat("BestDistance", 0f);
        if (playerDistance > previousBest)
        {
            PlayerPrefs.SetFloat("BestDistance", playerDistance);
            Debug.Log("New Best Distance: " + playerDistance);
        }
        else
        {
            Debug.Log("Distance: " + playerDistance + ", Best: " + previousBest);
        }
    }

    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.92504437f;
        float b = 4.32346315f;
        float y = a * ground_x * ground_x + b * ground_x;
        return ground_y <= y;
    }

    private bool MomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.07367866f;
        float b = 1.05197485f;
        float y = a * ground_x * ground_x + b * ground_x;
        return ground_y <= y;
    }

    private void UpdateDeathZone()
    {
        if (deathZonePrefab != null)
        {
            float deathZoneLength = lastPlatformEndX + 10f;
            float deathZoneX = deathZoneLength / 2f; 

            GameObject deathZone = GameObject.Find("DeathZone");
            if (deathZone == null)
            {
                deathZone = Instantiate(deathZonePrefab, new Vector3(deathZoneX, minY - 4f, 0f), Quaternion.identity);
                deathZone.name = "DeathZone";
            }
            else
            {
                deathZone.transform.position = new Vector3(deathZoneX, minY - 4f, 0f);
                deathZone.transform.localScale = new Vector3(deathZoneLength, 2f, 1f);
            }
        }
    }

}
