// Parallax.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/24/2024
// Course: EECS 581
// Purpose: Track the change in movement as time goes on to help move the camera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;          // Length of the sprite for looping
    private float startpos;        // Starting X position of the sprite
    public GameObject cam;         // Reference to the main camera
    public float parallexEffect;   // Effect multiplier for parallax movement

    void Start()
    {
        startpos = transform.position.x; // Initialize the starting position
        length = GetComponent<SpriteRenderer>().bounds.size.x; // Get the width of the sprite
    }

    void Update()
    {
        // Calculate temporary position based on camera movement and parallax effect
        float temp = (cam.transform.position.x * (1 - parallexEffect));
        // Calculate distance to move based on parallax effect
        float dist = (cam.transform.position.x * parallexEffect);
        // Update the position of the parallax layer
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
        
        // Loop the sprite to create an infinite scrolling effect
        if (temp > startpos + length)
            startpos += length; // Reset start position to the right
        else if (temp < startpos - length)
            startpos -= length; // Reset start position to the left
    }
}
