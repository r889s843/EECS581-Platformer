// ProcGen.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/01/2024
// Course: EECS 581
// Purpose: Level Generator
using UnityEngine;
using System.Collections.Generic;

public class ProcGen : MonoBehaviour
{
    // Singleton instance
    public static ProcGen Instance { get; private set; }

    // Prefabs and Resources
    public GameObject groundPrefab;
    public GameObject flagPrefab;
    public GameObject deathZonePrefab;

    // Level Generation Settings
    public int numberOfChunks = 10;
    public float startX = 6f;
    public float startY = -2f;
    public float minY = -2f;
    public float maxY = 2f;

    // Difficulty Levels
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    // Enemy prefabs for each difficulty
    public List<GameObject> easyEnemyPrefabs;
    public List<GameObject> mediumEnemyPrefabs;
    public List<GameObject> hardEnemyPrefabs;

    // Enemy spawn chances for each difficulty
    [Range(0f, 1f)]
    public float easyEnemySpawnChance = 0.2f;
    [Range(0f, 1f)]
    public float mediumEnemySpawnChance = 0.5f;
    [Range(0f, 1f)]
    public float hardEnemySpawnChance = 0.8f;

    public Difficulty currentDifficulty = Difficulty.Easy;

    private int GetMinimumPlatformLength()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
            case Difficulty.Medium:
                return 2;
            case Difficulty.Hard:
                return 1;
            default:
                return 2;
        }
    }

    private float GetMinimumVerticalSpacing()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return 2f;
            case Difficulty.Medium:
                return 2f;
            case Difficulty.Hard:
                return 1f;
            default:
                return 2f;
        }
    }



    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateNewLevel()
    {
        ClearExistingLevel();

        float x = startX;
        float y = startY;
        float minYReached = y;

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
        // Destroy all child objects (generated level elements)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private Vector2 CreateSafeChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 3); // Adjusted range

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        return new Vector2(currentX, currentY);
    }


    private Vector2 CreateDangerChunk(float x, float y)
    {
        int randomValue = Random.Range(0, 3);

        switch (randomValue)
        {
            case 0:
                return CreateGap(x, y);
            case 1:
                return CreateJump(x, y);
            case 2:
                return CreateShortJump(x, y);
            // case 3:
            //     return CreateBackJump(x, y);
            default:
                return new Vector2(x, y);
        }
    }

    private Vector2 CreateGap(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // Determine gap size
        float gapSize = Random.Range(5f, 8f);

        // Check if the gap is passable without momentum
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
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        float platformEndX = currentX;

        SpawnEnemies(platformStartX, platformEndX, currentY);

        return new Vector2(currentX, currentY);
    }


    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x;
        float deltaY = Random.Range(0, 2) == 0 ? 2f : -2f;
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY);

        // Determine gap size
        float gapSize = Random.Range(4f, 6f);

        // Check if the jump is possible with momentum
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
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        float platformEndX = currentX;

        SpawnEnemies(platformStartX, platformEndX, currentY);

        return new Vector2(currentX, currentY);
    }


    private Vector2 CreateShortJump(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int numPlatforms = Random.Range(2, 4);

        for (int i = 0; i < numPlatforms; i++)
        {
            // Small gaps between platforms
            float gapSize = 2f;

            // Random elevation change within constraints
            float deltaY = Random.Range(-1, 2) * GetMinimumVerticalSpacing();
            float nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);

            // Ensure platforms are not overlapping vertically
            if (Mathf.Abs(nextY - currentY) < GetMinimumVerticalSpacing())
            {
                deltaY = GetMinimumVerticalSpacing() * Mathf.Sign(deltaY);
                nextY = Mathf.Clamp(currentY + deltaY, minY, maxY);
            }

            // Check if the jump is possible without momentum
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

            // Platform length
            int minPlatformLength = GetMinimumPlatformLength();
            int platformLength = Random.Range(minPlatformLength, minPlatformLength + 1);

            for (int j = 0; j < platformLength; j++)
            {
                CreateGround(currentX, currentY);
                currentX += 2f;
            }
        }

        return new Vector2(currentX, currentY);
    }




    private Vector2 CreateBackJump(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        // Adjusted to ensure the platform is reachable via wall jump
        float deltaX = -6f;
        float deltaY = 4f;
        float targetX = currentX + deltaX;
        float targetY = Mathf.Clamp(currentY + deltaY, minY, maxY);

        // Check if wall jump is possible
        if (!WallJumpTest(currentX, deltaX, deltaY))
        {
            deltaY = 0f; // Adjust if not possible
            targetY = currentY;
        }

        currentX = targetX;
        currentY = targetY;

        int minPlatformLength = GetMinimumPlatformLength();
        int platformLength = Random.Range(minPlatformLength, minPlatformLength + 1);

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX -= 2f;
        }

        float edgeX = currentX - deltaX;
        float edgeY = Mathf.Clamp(currentY + 2f, minY, maxY);

        return new Vector2(edgeX, edgeY);
    }


    private void CreateEndChunk(float x, float y)
    {
        float currentX = x;
        float currentY = y;

        int platformLength = 5;

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        // Place the flag
        if (flagPrefab != null)
        {
            GameObject flag = Instantiate(
                flagPrefab,
                new Vector3(currentX - 6f, currentY + 6f, 0f),
                flagPrefab.transform.rotation,
                transform
            );
            flag.name = "Flag";

            // Update the agent's goalTransform
            PlatformerAgent agent = FindAnyObjectByType<PlatformerAgent>();
            if (agent != null)
            {
                agent.goalTransform = flag.transform;
            }
        }
    }

    private void CreateDeathZone(float x, float minY)
    {
        if (deathZonePrefab != null)
        {
            GameObject deathZone = Instantiate(deathZonePrefab, new Vector3((x + 10f) / 2f, minY - 4f, 0f), Quaternion.identity, transform);
            deathZone.transform.localScale = new Vector3(x + 30f, 2f, 1f);
            deathZone.name = "DeathZone";
        }
    }

    private void CreateGround(float x, float y)
    {
        if (groundPrefab != null)
        {
            Instantiate(groundPrefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
        }
    }

    // Jump Test Methods

    private bool MomentumJumpTest(float ground_x, float ground_y)
    {
        // Constants
        float a = -0.07367866f;
        float b = 1.05197485f;

        // Calculate y predicted by the x value
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y;
    }

    private bool NoMomentumJumpTest(float ground_x, float ground_y)
    {
        // Constants
        float a = -0.92504437f;
        float b = 4.32346315f;

        // Calculate y predicted by the x value
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y;
    }

    private bool WallJumpTest(float player_x, float ground_x, float ground_y)
    {
        // Adjust ground_x if player is ahead of the platform
        if (player_x < ground_x)
        {
            ground_x = ground_x + (player_x - ground_x) * 2f;
        }

        // Constants
        float a = -0.19835401f;
        float b = 1.45395189f;

        // Calculate y predicted by the x value
        float y = a * ground_x * ground_x + b * ground_x;

        return ground_y <= y;
    }

    private void SpawnEnemies(float startX, float endX, float y)
    {
        float spawnChance = 0f;
        List<GameObject> enemyPrefabs = null;

        // Determine spawn chance and enemy prefabs based on difficulty
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                spawnChance = easyEnemySpawnChance;
                enemyPrefabs = easyEnemyPrefabs;
                break;
            case Difficulty.Medium:
                spawnChance = mediumEnemySpawnChance;
                enemyPrefabs = mediumEnemyPrefabs;
                break;
            case Difficulty.Hard:
                spawnChance = hardEnemySpawnChance;
                enemyPrefabs = hardEnemyPrefabs;
                break;
        }

        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            for (float x = startX; x < endX; x += 2f)
            {
                if (Random.value < spawnChance)
                {
                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
                    Instantiate(enemyPrefab, new Vector3(x, y + 1f, 0f), Quaternion.identity, transform);
                }
            }
        }
    }

}
