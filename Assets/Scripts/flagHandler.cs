using UnityEngine;

public class flagHandler : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
    }

    // This method is called when another collider enters the trigger collider attached to the GameObject where this script is attached.
    void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if the collider belongs to the player
        if (collider.gameObject.CompareTag("Player"))
        {
            // Play the death sound
            if (audioSource != null)
            {
                audioSource.Play();
            }
            // THIS WORKS BUT I HAVE IT TURNED OFF FOR THE AI. THIS SHOULDN'T NEED MUCH UPDATING UNLESS WE PLAN TO ADD HEALTH
            // collider.gameObject.GetComponent<PlayerMovement>(); // this checks the players movement when a collison occurs
            // collider.transform.position = Respawn.position; // this teleports the player back to the respawn point.
        }
    }
}
