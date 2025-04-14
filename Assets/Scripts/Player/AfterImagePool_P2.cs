// AfterImagePool.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/09/2025
// Course: EECS 582
// Purpose: Supporting functions for AfterImage

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterPool_P2 : MonoBehaviour{

    [SerializeField] private GameObject afterImagePrefab;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public static PlayerAfterPool_P2 Instance { get; private set;}

    private void Awake()
    {
        Instance = this;
        GrowPool();
    }

    private void GrowPool(){
        for (int i = 0; i < 10; i++){
            var instanceToAdd = Instantiate(afterImagePrefab);
            instanceToAdd.transform.SetParent(transform);
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance){
        instance.SetActive(false);
        availableObjects.Enqueue(instance);
    }

    public GameObject GetFromPool(){
        if (availableObjects.Count == 0){
            GrowPool();
        }

        var instance = availableObjects.Dequeue();
        instance.SetActive(true);
        return instance;
    }
}