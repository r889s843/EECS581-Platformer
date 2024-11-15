// PlatformerAgent.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
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
    public Transform goalTransform; // Reference to the goal object (flag)
    public LayerMask platformLayer; // Layer mask for platform detection
    public LayerMask detectableLayers; // Layer mask for objects detectable by raycasts

    public float raycastDistance = 15f; // Maximum distance for raycasts
    private Rigidbody2D body; // Reference to the agent's Rigidbody2D component
    private Vector2 previousPosition; // Stores the agent's previous position
    private Vector2 startPosition; // Starting position of the agent

    private PlayerMovement playerMovement; // Reference to the PlayerMovement script
    private bool levelCompleted = false; // Indicates if the level has been completed

    // Completion tracking
    public int levelCompletionThreshold = 100; // Number of times to complete the level before moving on
    private int currentLevelCompletions = 0; // Tracks how many times the current level has been completed

    public override void Initialize()
    {
        body = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        startPosition = transform.position; // Store the starting position
        previousPosition = startPosition; // Initialize previous position
        playerMovement = GetComponent<PlayerMovement>(); // Get the PlayerMovement component
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition; // Reset position to start
        body.linearVelocity = Vector2.zero; // Reset velocity
        previousPosition = transform.position; // Reset previous position

        if (levelCompleted)
        {
            levelCompleted = false; // Reset level completion flag
            currentLevelCompletions = 0; // Reset completion count
            LevelManager.Instance.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
        }

        // Update the goalTransform to the new flag
        // GameObject flagObject = GameObject.Find("Flag");
        // goalTransform = flagObject.transform;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's normalized position and velocity
        sensor.AddObservation(transform.localPosition / 10f);
        sensor.AddObservation(body.linearVelocity / 10f);

        // Normalized direction to the goal
        Vector2 directionToGoal = (goalTransform.position - transform.position) / 10f;
        sensor.AddObservation(directionToGoal);

        // Grounded state observation
        int groundedState = GetGroundedState();
        sensor.AddObservation(groundedState);  // 0: floor, 1: left wall, 2: right wall, -1: air

        // Perform raycasts around the agent
        int numRaycasts = 36;
        float angleIncrement = 360f / numRaycasts;

        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;  // Convert angle to radians
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));  // Direction vector

            // Raycast in the specified direction
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, detectableLayers);

            if (hit.collider != null)
            {
                // If hit, add normalized distance and object type
                sensor.AddObservation(hit.distance / raycastDistance);
                sensor.AddObservation(GetObjectType(hit.collider.tag));
            }
            else
            {
                // If no hit, add max distance and unknown object type
                sensor.AddObservation(1f); // Max distance
                sensor.AddObservation(0f); // No object detected
            }
        }
    }

    private float GetObjectType(string tag)
    {
        // Map object tags to numerical values for observations
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
                return 0f;  // Unknown object
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0]; // Movement action: 0=none, 1=left, 2=right
        int jumpAction = actionBuffers.DiscreteActions[1]; // Jump action: 0=no jump, 1=jump

        float horizontal = 0f; // Horizontal input
        bool jump = false; // Jump input

        if (moveAction == 1) horizontal = -1f;  // Move left
        else if (moveAction == 2) horizontal = 1f;  // Move right

        if (jumpAction == 1) jump = true; // Set jump to true

        playerMovement.SetInput(horizontal, jump); // Apply inputs to the player movement script

        // Reward shaping based on progress towards the goal
        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position);
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position);
        float progress = previousDistanceToGoal - distanceToGoal; // Calculate progress made
        AddReward(progress * 0.2f); // Reward for getting closer to the goal
        AddReward(-0.001f); // Small time penalty to encourage efficiency

        previousPosition = transform.position; // Update previous position
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        float moveInput = Input.GetAxisRaw("Horizontal");  // Get horizontal input from player
        discreteActionsOut[0] = moveInput > 0 ? 2 : (moveInput < 0 ? 1 : 0);  // Map input to discrete actions
        discreteActionsOut[1] = Input.GetButton("Jump") ? 1 : 0; // Map jump input to action
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flag"))
        {
            SetReward(5.0f);  // Reward for reaching the goal
            Debug.Log($"Agent Won. Final reward: {GetCumulativeReward()}");

            currentLevelCompletions++;  // Increment completion count
            Debug.Log($"Current Count: {currentLevelCompletions}");

            if (currentLevelCompletions >= levelCompletionThreshold)
            {
                levelCompleted = true;  // Mark level as completed
                EndEpisode();  // End the episode
            }
            else
            {
                // Reset agent's position without ending the episode
                transform.position = startPosition;
                body.linearVelocity = Vector2.zero;
            }
        }
        else if (collision.CompareTag("DeathZone") || collision.CompareTag("Enemy") || collision.CompareTag("Hazard") || collision.CompareTag("Projectile"))
        {
            SetReward(-1.0f);  // Negative reward for dying or hitting a hazard
            Debug.Log($"Agent Died. Final reward: {GetCumulativeReward()}");
            EndEpisode(); // End the episode
        }
    }

    private int GetGroundedState()
    {
        Bounds bounds = GetComponent<BoxCollider2D>().bounds;  // Get collider bounds

        // Check for ground below
        RaycastHit2D hitDown = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, 0.02f, platformLayer);
        // Check for wall on the left
        RaycastHit2D hitLeft = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.left, 0.02f, platformLayer);
        // Check for wall on the right
        RaycastHit2D hitRight = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.right, 0.02f, platformLayer);

        if (hitDown.collider != null)
            return 0;  // On floor
        if (hitLeft.collider != null)
            return 1;  // On left wall
        if (hitRight.collider != null)
            return 2;  // On right wall

        return -1;     // In air
    }
}
