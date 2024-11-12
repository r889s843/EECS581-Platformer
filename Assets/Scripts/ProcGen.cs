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
    public int numberOfChunks = 12;
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
    public Difficulty currentDifficulty = Difficulty.Easy;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Uncomment if you want this object to persist across scenes
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

        int platformLength = Random.Range(4, 6); // Platform will be 4-5 units in length

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        return new Vector2(currentX, currentY);
    }

    private Vector2 CreateDangerChunk(float x, float y)
    {
        int randomValue = Random.Range(0, 4);

        switch (randomValue)
        {
            case 0:
                return CreateGap(x, y);
            case 1:
                return CreateJump(x, y);
            case 2:
                return CreateShortJump(x, y);
            case 3:
                return CreateBackJump(x, y);
            default:
                return new Vector2(x, y);
        }
    }

    private Vector2 CreateGap(float x, float y)
    {
        float currentX = x + Random.Range(5f, 8f);
        float currentY = y;

        int platformLength = Random.Range(4, 6);

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        return new Vector2(currentX, currentY);
    }

    private Vector2 CreateJump(float x, float y)
    {
        float currentX = x + Random.Range(4f, 6f);
        float deltaY = Random.Range(0, 2) == 0 ? 2f : -2f;
        float currentY = Mathf.Clamp(y + deltaY, minY, maxY);

        int platformLength = 5;

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 2f;
        }

        return new Vector2(currentX, currentY);
    }

    private Vector2 CreateShortJump(float x, float y)
    {
        float currentX = x + 2f;
        float currentY = y;

        int numPlatforms = Random.Range(2, 4);

        for (int i = 0; i < numPlatforms; i++)
        {
            CreateGround(currentX, currentY);
            currentX += 4f;
            float deltaY = Random.Range(-1, 2) * 2f;
            currentY = Mathf.Clamp(currentY + deltaY, minY, maxY);
        }

        return new Vector2(currentX, currentY);
    }

    private Vector2 CreateBackJump(float x, float y)
    {
        float currentX = x - 6f;
        float currentY = Mathf.Clamp(y + 4f, minY, maxY);

        int platformLength = Random.Range(2, 4);

        for (int i = 0; i < platformLength; i++)
        {
            CreateGround(currentX, currentY);
            currentX -= 2f;
        }

        float edgeX = currentX + 6f;
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
}
