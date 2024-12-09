// EnemyShooter.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/08/2024
// Course: EECS 581
// Purpose: Bullet Controller for enemies

using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bullet; // Prefab for the bullet to be fired
    public Transform bulletPos; // Position from where the bullet is instantiated
    private AudioSource audioSource; // Audio source for playing shooting sound
    private float timer; // Timer to control shooting intervals
    private GameObject player; // Reference to the player object

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player by tag
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position); // Calculate distance to player

        timer += Time.deltaTime; // Increment timer by the time elapsed since last frame
        if (distance < 10) // Check if player is within shooting range
        {
            if (timer > 2) // Check if enough time has passed to shoot again
            {
                timer = 0; // Reset timer
                shoot(); // Call the shoot method
            }
        }
    }

    void shoot()
    {
        audioSource.Play(); // Play shooting sound
        Instantiate(bullet, bulletPos.position, Quaternion.identity); // Instantiate bullet at bulletPos with no rotation
    }
}
