// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: AI controls and management. This controls how the AI is rewarded, sees the world, and how it moves.

using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlatformerAgent : Agent
{
    public Transform goalTransform;
    public LayerMask platformLayer;
    public LayerMask detectableLayers;

    public float raycastDistance = 15f;
    private Rigidbody2D rigidbody;
    private Vector2 previousPosition;
    private Vector2 startPosition;

    private PlayerMovement playerMovement;

    // Completion tracking
    public int levelCompletionThreshold = 3; // Number of times to complete the level before moving on
    private int currentLevelCompletions = 0; // Tracks how many times the current level has been completed

    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        previousPosition = startPosition;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        rigidbody.linearVelocity = Vector2.zero;
        previousPosition = transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position and velocity
        sensor.AddObservation(transform.localPosition / 10f);
        sensor.AddObservation(rigidbody.linearVelocity / 10f);

        // Direction to the goal
        Vector2 directionToGoal = (goalTransform.position - transform.position) / 10f;
        sensor.AddObservation(directionToGoal);

        // Grounded status
        int groundedState = onGround();
        sensor.AddObservation(groundedState); // Values: 0, 1, 2, -1

        // Number of raycasts for 360-degree coverage
        int numRaycasts = 36;
        float angleIncrement = 360f / numRaycasts;

        // Cast rays in 360 degrees around the AI
        for (int i = 0; i < numRaycasts; i++)
        {
            // Calculate the angle in radians
            float angle = i * angleIncrement;
            float radian = angle * Mathf.Deg2Rad;

            // Calculate the direction for this ray
            Vector2 direction = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;

            // Perform the raycast
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, detectableLayers);

            if (hit.collider != null)
            {
                // Normalized distance observation
                sensor.AddObservation(hit.distance / raycastDistance);
                // Object type observation
                sensor.AddObservation(GetObjectType(hit.collider.tag));
            }
            else
            {
                // No object detected within range
                sensor.AddObservation(1f); // Full distance
                sensor.AddObservation(0f); // No object type detected
            }

            // Debugging raycasts (visualize in Scene view)
            Debug.DrawRay(transform.position, direction * raycastDistance, Color.red, 0.1f);
        }
    }


    // Function to convert object tags to observation values
    private float GetObjectType(string tag)
    {
        switch (tag)
        {
            case "Ground":
                return 1f;
            case "Enemy":
                return 2f;
            case "Projectile":
                return 3f;
            case "Hazard":
                return 4f;
            case "DeathZone":
                return 5f;
            case "Flag":
                return 6f;
            case "FallingPlatform":
                return 7f;
            default:
                return 0f; // No object or unknown tag
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        int jumpAction = actionBuffers.DiscreteActions[1];

        float horizontal = 0f;
        bool jump = false;

        if (moveAction == 1) horizontal = -1f;
        else if (moveAction == 2) horizontal = 1f;

        if (jumpAction == 1) jump = true;

        // Set the input in the PlayerMovement script
        playerMovement.SetInput(horizontal, jump);

        // Update rewards and previous position
        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position);
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position);
        float distanceReward = previousDistanceToGoal - distanceToGoal;
        AddReward(distanceReward * 0.1f);

        AddReward(-0.01f); // Time penalty

        previousPosition = transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        float moveInput = Input.GetAxisRaw("Horizontal");
        discreteActionsOut[0] = moveInput > 0 ? 2 : (moveInput < 0 ? 1 : 0);
        discreteActionsOut[1] = Input.GetButton("Jump") ? 1 : 0;
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("Looping back to Level1.");
            SceneManager.LoadScene(0); // Go back to Level1 if we complete the last level
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flag"))
        {
            SetReward(5.0f);
            Debug.Log($"Goal reached. Final reward: {GetCumulativeReward()}");

            // Increment the completion count
            currentLevelCompletions++;

            Debug.Log($"Current Count: {currentLevelCompletions}");

            // Check if the level completion count meets the threshold
            if (levelCompletionThreshold < currentLevelCompletions)
            {
                // Reset level completion counter for the new level
                currentLevelCompletions = 0;
                LoadNextLevel();
            }

            EndEpisode();
        }
        else if (collision.CompareTag("DeathZone"))
        {
            SetReward(-5.0f);
            Debug.Log($"Agent hit the DeathZone. Final reward: {GetCumulativeReward()}");
            EndEpisode();
        }
        else if (collision.CompareTag("Enemy"))
        {
            SetReward(-4.0f);
            Debug.Log($"Agent hit the Enemy. Final reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }

    // Check if the agent is grounded
    private int onGround()
    {
        RaycastHit2D hitDown = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.down, 0.02f, platformLayer
        );

        RaycastHit2D hitLeft = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.left, 0.02f, platformLayer
        );

        RaycastHit2D hitRight = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.right, 0.02f, platformLayer
        );

        if (hitDown.collider != null)
        {
            return 0; // On floor
        }
        if (hitLeft.collider != null)
        {
            return 1; // On left wall
        }
        if (hitRight.collider != null)
        {
            return 2; // On right wall
        }

        return -1; // Not grounded
    }
    
}