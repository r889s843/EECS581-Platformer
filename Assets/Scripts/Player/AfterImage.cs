// AfterImage.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/09/2025
// Course: EECS 582
// Purpose: Creates dashing effect

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour{
    private Transform player;
    private float timeActivated;
    private float activeTime = 0.3f;
    private float alpha;
    private float alphaSet = 0.8f;
    private float alphaMultipler = 0.95f;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR; 

    private Color color;


    private void OnEnable()
    {
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();
        alpha = alphaSet;

        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        transform.localScale = player.localScale;
        timeActivated = Time.time;
    }

    private void Update()
    {
        alpha *= alphaMultipler;
        color = new Color(1f, 1f, 1f, alpha);
        SR.color = color;

        if(Time.time >= (timeActivated + activeTime)){
            PlayerAfterPool.Instance.AddToPool(gameObject);
        }
    }
}