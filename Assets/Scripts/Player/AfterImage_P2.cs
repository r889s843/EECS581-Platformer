// AfterImage_P2.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 4/13/2025
// Course: EECS 582
// Purpose: Creates dashing effect for P2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage_P2 : MonoBehaviour
{
    private Transform player;
    private float timeActivated;
    private float activeTime = 0.3f;
    private float alpha;
    private float alphaSet = 0.8f;
    private float alphaMultiplier = 0.95f;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR;
    private Color color;
    private bool isInitialized = false;

    private void OnEnable()
    {
        // Defer initialization to ensure objects are available
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        // Wait for the end of the frame to ensure all objects are properly instantiated
        yield return new WaitForEndOfFrame();

        // Initialize components and references
        SR = GetComponent<SpriteRenderer>();
        if (SR == null)
        {
            Debug.LogWarning("AfterImage_P2: SpriteRenderer component not found on this GameObject.");
            PlayerAfterPool_P2.Instance.AddToPool(gameObject);
            yield break;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player2");
        if (playerObj == null)
        {
            Debug.LogWarning("AfterImage_P2: Player2 not found in the scene. Adding back to pool.");
            PlayerAfterPool_P2.Instance.AddToPool(gameObject);
            yield break;
        }

        player = playerObj.transform;
        playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR == null)
        {
            Debug.LogWarning("AfterImage_P2: SpriteRenderer not found on Player2.");
            PlayerAfterPool_P2.Instance.AddToPool(gameObject);
            yield break;
        }

        alpha = alphaSet;
        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        transform.localScale = player.localScale;
        timeActivated = Time.time;

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        alpha *= alphaMultiplier;
        color = new Color(1f, 1f, 1f, alpha);
        SR.color = color;

        if (Time.time >= (timeActivated + activeTime))
        {
            isInitialized = false; // Reset for next use
            PlayerAfterPool_P2.Instance.AddToPool(gameObject);
        }
    }
}