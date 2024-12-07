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
    public static ProcGen Instance { get; private set; }

    public GameObject wallPrefab;        
    public GameObject flagPrefab;        
    public GameObject deathZonePrefab;   

    public Tilemap groundTilemap;        
    public Tilemap hazardTilemap;        

    public TileBase leftTile;            
    public TileBase centerTile;          
    public TileBase rightTile;           
    public TileBase spikeTile;           

    public int numberOfChunks = 4;       
    public float startX = 0f;            
    public float startY = 0f;            
    public float minY = -10f;            
    public float maxY = 20f;             

    // public int totalCompletions = 0;
    // public int completionsForGradualIncrease = 5; 
    // public int completionsForLevelUpgrade = 100;  

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Hard;  

    public List<GameObject> easyEnemyPrefabs;
    public List<GameObject> mediumEnemyPrefabs;
    public List<GameObject> hardEnemyPrefabs;

    [Range(0f, 1f)]
    public float spikeSpawnChance = 0f;

    [Range(0f, 1f)]
    public float EnemySpawnChance = 0f;

    private List<GameObject> generatedObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateNewLevel();
    }

    public void GenerateNewLevel()
    {
        ClearExistingLevel();

        Vector2 initialPlatformEnd = CreateInitialPlatform();
        float x = initialPlatformEnd.x;      
        float y = initialPlatformEnd.y;      
        float minYReached = y;               

        // Retrieve current difficulty settings from LevelManager
        Difficulty currentDifficulty = (Difficulty)LevelManager.Instance.currentDifficulty;
        int numberOfChunks = LevelManager.Instance.numberOfChunks;
        float spikeSpawnChance = LevelManager.Instance.spikeSpawnChance;
        float EnemySpawnChance = LevelManager.Instance.EnemySpawnChance;

        this.currentDifficulty = currentDifficulty;
        this.numberOfChunks = numberOfChunks;
        this.spikeSpawnChance = spikeSpawnChance;
        this.EnemySpawnChance = EnemySpawnChance;

        for (int i = 0; i < numberOfChunks; i++)
        {
            Vector2 newCoords = CreateSafeChunk(x, y);    
            x = newCoords.x;
            y = newCoords.y;

            newCoords = CreateDangerChunk(x, y);          
            x = newCoords.x;
            y = newCoords.y;

            minYReached = Mathf.Min(minYReached, y);
        }

        CreateEndChunk(x, y);
        CreateDeathZone(x, minYReached);
    }

    private void ClearExistingLevel()
    {
        groundTilemap.ClearAllTiles();
        hazardTilemap.ClearAllTiles();

        foreach (GameObject obj in generatedObjects)
        {
            Destroy(obj);
        }
        generatedObjects.Clear();
    }

    private Vector2 CreateInitialPlatform()
    {
        float currentX = 0f;
        float currentY = 0f;

        PaintGroundTile(currentX, currentY, leftTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, rightTile);

        float finalX = currentX; // right tile is placed at currentX=3f
        float finalY = currentY;
        currentX += 1f;

        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateSafeChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3);

        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else
                    PaintGroundTile(currentX, currentY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);
                else
                    PaintGroundTile(currentX, currentY, centerTile);
            }
            currentX += 1f;
        }

        float finalX = currentX - 1f; // last tile placed at currentX-1f
        float finalY = currentY;
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateDangerChunk(float x, float y)
    {
        // If near max height, spawn downward chunks
        bool nearMaxHeight = y >= maxY - 1f;

        if (currentDifficulty == Difficulty.Easy)
        {
            if (nearMaxHeight)
            {
                // Force downward jump if near maxY
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y);
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 3);
            switch (randomValue)
            {
                case 0: return CreateGap(x, y);
                case 1: return CreateJump(x, y);
                case 2: return CreateShortJump(x, y);
                default: return new Vector2(x, y);
            }
        }
        else if (currentDifficulty == Difficulty.Medium)
        {
            if (nearMaxHeight)
            {
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y);
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 4);
            switch (randomValue)
            {
                case 0: return CreateGap(x, y);
                case 1: return CreateJump(x, y);
                case 2: return CreateShortJump(x, y);
                case 3: return CreateWallJumpSection(x, y);
                default: return new Vector2(x, y);
            }
        }
        else // Difficulty.Hard
        {
            if (nearMaxHeight)
            {
                // if (Random.value < 0.5f)
                return CreateDownJump(x, y);
                // else
                //     return CreateWallDownJumpSectionDown(x, y);
            }
            int randomValue = Random.Range(0, 3);
            switch (randomValue)
            {
                case 0: return CreateJump(x, y);
                case 1: return CreateShortJump(x, y);
                case 2: return CreateWallJumpSection(x, y);
                default: return new Vector2(x, y);
            }
        }
    }

    private Vector2 CreateGap(float x, float y)
    {
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

        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2);

        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else
                    PaintGroundTile(currentX, currentY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);
                else
                    PaintGroundTile(currentX, currentY, centerTile);
            }
            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;

        SpawnEnemies(platformStartX, currentX, currentY);

        if (currentDifficulty != Difficulty.Easy)
        {
            SpawnSpikes(platformStartX, currentX, currentY);
        }

        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x;
        float deltaY = Random.Range(0, 2) == 0 ? 2f : -2f;
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
        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2);

        float platformStartX = currentX;
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else
                    PaintGroundTile(currentX, currentY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);
                else
                    PaintGroundTile(currentX, currentY, centerTile);
            }
            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;

        SpawnEnemies(platformStartX, currentX, currentY);
        return new Vector2(finalX, finalY);
    }

    private Vector2 CreateShortJump(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int numPlatforms = Random.Range(2, 4);

        for (int i = 0; i < numPlatforms; i++)
        {
            float gapSize = 2f;
            float deltaY = Random.Range(-1, 2) * GetMinimumVerticalSpacing();
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

            if (Mathf.Abs(nextY - currentY) < GetMinimumVerticalSpacing())
            {
                deltaY = GetMinimumVerticalSpacing() * Mathf.Sign(deltaY);
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

            int minPlatformLength = GetMinimumPlatformLength();
            int platformLength = Random.Range(minPlatformLength, minPlatformLength + 1);

            float platformStartX = currentX;
            for (int j = 0; j < platformLength; j++)
            {
                if (platformLength == 2)
                {
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile);
                    else
                        PaintGroundTile(currentX, currentY, rightTile);
                }
                else
                {
                    if (j == 0)
                        PaintGroundTile(currentX, currentY, leftTile);
                    else if (j == platformLength - 1)
                        PaintGroundTile(currentX, currentY, rightTile);
                    else
                        PaintGroundTile(currentX, currentY, centerTile);
                }
                currentX += 1f;
            }

            float finalX = currentX - 1f;
            float finalY = currentY;
            SpawnEnemies(platformStartX, currentX, currentY);

            if (currentDifficulty != Difficulty.Easy)
            {
                SpawnSpikes(platformStartX, currentX, currentY);
            }
        }

        return new Vector2(currentX - 1f, currentY);
    }

    private Vector2 CreateWallJumpSection(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // Entry platform (5 tiles)
        PaintGroundTile(currentX, currentY, leftTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, rightTile);
        currentX += 1f;

        CreateWall(currentX - 3f, currentY + 7f); 
        currentX += 1f; 

        float wallGap = 5f;
        CreateWall(currentX - 4f + wallGap, currentY + 5f);

        float wallHeight = wallPrefab.GetComponent<Renderer>().bounds.size.y;
        float exitPlatformY = currentY + wallHeight;
        int exitPlatformLength = 2;

        float exitPlatformX = currentX - 4f + wallGap + 1f;

        for (int i = 0; i < exitPlatformLength; i++)
        {
            if (exitPlatformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile);
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(exitPlatformX, exitPlatformY, leftTile);
                else if (i == exitPlatformLength - 1)
                    PaintGroundTile(exitPlatformX, exitPlatformY, rightTile);
                else
                    PaintGroundTile(exitPlatformX, exitPlatformY, centerTile);
            }
            exitPlatformX += 1f;
        }

        float finalX = exitPlatformX - 1f;
        float finalY = exitPlatformY;

        SpawnEnemies(finalX - (exitPlatformLength * 1f), finalX, finalY);
        SpawnSpikes(finalX - (exitPlatformLength * 1f), finalX, finalY);

        return new Vector2(finalX, finalY);
    }

    // New downward jump similar to CreateJump but forces a downward movement
    private Vector2 CreateDownJump(float x, float y)
    {
        float currentX = x;
        // Force a larger downward drop
        float deltaY = Random.Range(-4, -2) * 2f;
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY);

        float gapSize = Random.Range(4f, 6f);
        while (!MomentumJumpTest(gapSize, currentY - y))
        {
            gapSize -= 0.5f;
            if (gapSize <= 1f) break;
        }

        currentX += gapSize; 
        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 2);

        float platformStartX = currentX;
        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else
                    PaintGroundTile(currentX, currentY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);
                else
                    PaintGroundTile(currentX, currentY, centerTile);
            }
            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;

        SpawnEnemies(platformStartX, currentX, currentY);
        if (currentDifficulty != Difficulty.Easy)
            SpawnSpikes(platformStartX, currentX, currentY);

        return new Vector2(finalX, finalY);
    }

    // New downward wall jump section
    private Vector2 CreateWallDownJumpSectionDown(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // Entry platform (shorter)
        PaintGroundTile(currentX, currentY, leftTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, centerTile);
        currentX += 1f;
        PaintGroundTile(currentX, currentY, rightTile);
        currentX += 1f;

        // Place a wall above
        CreateWall(currentX - 1f, currentY - 5f);
        float gapSize = 5f;
        CreateWall(currentX + gapSize, currentY + 1f);
        // Move downward more significantly
        float deltaY = Random.Range(-4, -3) * 2f;
        float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

        while (!MomentumJumpTest(gapSize, nextY - currentY))
        {
            gapSize -= 0.5f;
            if (gapSize < 1f) gapSize = 1f;
        }

        currentX += gapSize;
        currentY = nextY;

        int platformLength = 3;
        float platformStartX = currentX;
        for (int i = 0; i < platformLength; i++)
        {
            if (i == 0)
                PaintGroundTile(currentX, currentY, leftTile);
            else if (i == platformLength - 1)
                PaintGroundTile(currentX, currentY, rightTile);
            else
                PaintGroundTile(currentX, currentY, centerTile);
            currentX += 1f;
        }

        float finalX = currentX - 1f;
        float finalY = currentY;

        SpawnEnemies(platformStartX, currentX, currentY);
        if (currentDifficulty != Difficulty.Easy)
            SpawnSpikes(platformStartX, currentX, currentY);

        return new Vector2(finalX, finalY);
    }

    private void CreateWall(float x, float startY)
    {
        if (wallPrefab != null)
        {
            GameObject wall = Instantiate(wallPrefab, new Vector3(x, startY, 0f), Quaternion.identity, transform);
            generatedObjects.Add(wall);
        }
    }

    private Vector2 CreateEndChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int platformLength = 5;

        for (int i = 0; i < platformLength; i++)
        {
            if (platformLength == 2)
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else
                    PaintGroundTile(currentX, currentY, rightTile);
            }
            else
            {
                if (i == 0)
                    PaintGroundTile(currentX, currentY, leftTile);
                else if (i == platformLength - 1)
                    PaintGroundTile(currentX, currentY, rightTile);
                else
                    PaintGroundTile(currentX, currentY, centerTile);
            }
            currentX += 1f;
        }

        if (flagPrefab != null)
        {
            GameObject flag = Instantiate(
                flagPrefab,
                new Vector3(currentX - 6f, currentY + 5f, 0f),
                flagPrefab.transform.rotation,
                transform
            );
            flag.name = "Flag";
            generatedObjects.Add(flag);

            PlatformerAgent[] agents = FindObjectsOfType<PlatformerAgent>();
            if (agents.Length > 0)
            {
                foreach (PlatformerAgent agent in agents)
                {
                    agent.goalTransform = flag.transform; 
                }
            }
            else
            {
                Debug.LogWarning("PlatformerAgent not found in the scene.");
            }
        }

        float finalX = currentX - 1f;
        float finalY = currentY;
        return new Vector2(finalX, finalY);
    }

    private void CreateDeathZone(float x, float minY)
    {
        if (deathZonePrefab != null)
        {
            float deathZoneX = ((x + 10f) / 2f) - 10f;

            GameObject deathZone = Instantiate(
                deathZonePrefab,
                new Vector3(deathZoneX, minY - 4f, 0f),
                Quaternion.identity,
                transform
            );

            deathZone.transform.localScale = new Vector3(x + 70f, 2f, 1f);
            deathZone.name = "DeathZone";
            generatedObjects.Add(deathZone);
        }
    }

    private void PaintGroundTile(float x, float y, TileBase tile)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y);
        groundTilemap.SetTile(tilePos, tile);
    }

    private void PaintSpikeTile(float x, float y)
    {
        Vector3Int tilePos = WorldToTilePosition(x, y);
        hazardTilemap.SetTile(tilePos, spikeTile);
    }

    private Vector3Int WorldToTilePosition(float x, float y)
    {
        return groundTilemap.WorldToCell(new Vector3(x, y, 0f));
    }

    private void SpawnSpikes(float startX, float endX, float y)
    {
        float xPos = startX;
        while (xPos < endX)
        {
            if (Random.value < spikeSpawnChance)
            {
                for (int i = 0; i < 3; i++)
                {
                    PaintSpikeTile(xPos, y + 1f);
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

    private void SpawnEnemies(float startX, float endX, float y)
    {
        float platformLength = (endX - startX);
        if (platformLength <= 2)
            return;

        float centerX = startX + (endX - startX) / 2f;
        float spawnStartX = centerX - 1f;
        float spawnEndX = centerX + 1f;

        spawnStartX = Mathf.Max(spawnStartX, startX + 1f);
        spawnEndX = Mathf.Min(spawnEndX, endX - 1f);

        List<GameObject> enemyPrefabs = null;

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
            for (float sx = spawnStartX; sx < spawnEndX; sx += 1f)
            {
                if (Random.value < EnemySpawnChance)
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(sx, y + 1f, 0f), Quaternion.identity, transform);
                    generatedObjects.Add(enemy);
                }
            }
        }
    }

    private int GetMinimumPlatformLength()
    {
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
        float a = -0.07367866f;
        float b = 1.05197485f;
        float y = a * ground_x * ground_x + b * ground_x;
        return ground_y <= y;
    }

    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        float a = -0.92504437f;
        float b = 4.32346315f;
        float y = a * ground_x * ground_x + b * ground_x;
        return ground_y <= y;
    }

    private bool WallJumpTest(float player_x, float ground_x, float ground_y)
    {
        if (player_x < ground_x)
        {
            ground_x = ground_x + (player_x - ground_x) * 2f;
        }

        float a = -0.19835401f;
        float b = 1.45395189f;
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y;
    }
}
