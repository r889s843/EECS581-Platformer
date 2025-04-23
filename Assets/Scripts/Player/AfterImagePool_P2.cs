// AfterImagePool_2.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 4/13/2025
// Course: EECS 582
// Purpose: Supporting functions for P2 AfterImage

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterPool_P2 : MonoBehaviour
{
    [SerializeField] private GameObject afterImagePrefab;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static PlayerAfterPool_P2 Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (afterImagePrefab == null)
        {
            Debug.LogWarning("PlayerAfterPool_P2: afterImagePrefab is not assigned in the Inspector.");
            return;
        }
        GrowPool();
    }

    private void GrowPool()
    {
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var instanceToAdd = Instantiate(afterImagePrefab, transform);
                AddToPool(instanceToAdd);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"PlayerAfterPool_P2: Failed to instantiate afterImagePrefab: {e.Message}");
            }
        }
    }

    public void AddToPool(GameObject instance)
    {
        if (instance == null)
            return;

        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool()
    {
        if (afterImagePrefab == null)
        {
            Debug.LogWarning("PlayerAfterPool_P2: afterImagePrefab is not assigned, cannot grow pool.");
            return null;
        }

        if (availableObjects.Count == 0)
        {
            GrowPool();
        }

        if (availableObjects.Count == 0)
        {
            Debug.LogWarning("PlayerAfterPool_P2: No objects available in pool after growing.");
            return null;
        }

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}