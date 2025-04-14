// AfterImage_P2.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 4/13/2025
// Course: EECS 582
// Purpose: Creates dashing effect for P2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage_P2 : MonoBehaviour{
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
        player = GameObject.FindGameObjectWithTag("Player2").transform;
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
            PlayerAfterPool_P2.Instance.AddToPool(gameObject);
        }
    }
}