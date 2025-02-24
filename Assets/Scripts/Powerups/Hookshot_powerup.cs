using UnityEngine;

public class Teleport_Powerup : MonoBehaviour
{
    [Header("Teleport Settings")]
    public float teleportRange = 5f;   // How far the player can teleport
    public LayerMask obstacleLayers;   // Layers to test for collision (walls, ground, etc.)

    private PlayerMovement playerMovement;
    private Rigidbody2D body;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
    }

    public void ActivatePowerup()
    {
        // Optional: you can do a Raycast in the direction the player is facing
        float direction = (transform.localScale.x >= 0) ? 1f : -1f;
        Vector2 origin = transform.position;
        Vector2 rayDir = new Vector2(direction, 0f);

        // Perform a raycast to see where we can teleport
        RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, teleportRange, obstacleLayers);

        Vector2 targetPosition;
        if (hit.collider != null)
        {
            // Teleport just before the obstacle
            targetPosition = hit.point - (rayDir * 0.5f);
        }
        else
        {
            // If no obstacle, teleport to full range in that direction
            targetPosition = origin + rayDir * teleportRange;
        }

        // Actually move the player
        transform.position = targetPosition;
    }
}