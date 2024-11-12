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
    public Transform goalTransform;
    public LayerMask platformLayer;
    public LayerMask detectableLayers;

    public float raycastDistance = 15f;
    private Rigidbody2D body;
    private Vector2 previousPosition;
    private Vector2 startPosition;

    private PlayerMovement playerMovement;
    private bool levelCompleted = false;

    // Completion tracking
    public int levelCompletionThreshold = 3; // Number of times to complete the level before moving on
    private int currentLevelCompletions = 0; // Tracks how many times the current level has been completed

    public override void Initialize()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        previousPosition = startPosition;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        body.linearVelocity = Vector2.zero;
        previousPosition = transform.position;

        if (levelCompleted)
        {
            levelCompleted = false;
            currentLevelCompletions = 0;
            LevelManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Update the goalTransform to the new flag
        // GameObject flagObject = GameObject.Find("Flag");
        // goalTransform = flagObject.transform;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position and velocity
        sensor.AddObservation(transform.localPosition / 10f);
        sensor.AddObservation(body.linearVelocity / 10f);

        // Direction to the goal
        Vector2 directionToGoal = (goalTransform.position - transform.position) / 10f;
        sensor.AddObservation(directionToGoal);

        // Grounded status
        int groundedState = GetGroundedState();
        sensor.AddObservation(groundedState); // Values: 0 (floor), 1 (left wall), 2 (right wall), -1 (air)

        // Raycasts around the agent
        int numRaycasts = 36;
        float angleIncrement = 360f / numRaycasts;

        for (int i = 0; i < numRaycasts; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, raycastDistance, detectableLayers);

            if (hit.collider != null)
            {
                sensor.AddObservation(hit.distance / raycastDistance);
                sensor.AddObservation(GetObjectType(hit.collider.tag));
            }
            else
            {
                sensor.AddObservation(1f); // Max distance
                sensor.AddObservation(0f); // No object detected
            }
        }
    }

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
                return 0f; // Unknown
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

        playerMovement.SetInput(horizontal, jump);

        // Reward shaping
        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position);
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position);
        float progress = previousDistanceToGoal - distanceToGoal;
        AddReward(progress * 0.1f);

        AddReward(-0.001f); // Small time penalty

        previousPosition = transform.position;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        float moveInput = Input.GetAxisRaw("Horizontal");
        discreteActionsOut[0] = moveInput > 0 ? 2 : (moveInput < 0 ? 1 : 0);
        discreteActionsOut[1] = Input.GetButton("Jump") ? 1 : 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flag"))
        {
            SetReward(5.0f);

            currentLevelCompletions++;

            if (currentLevelCompletions >= levelCompletionThreshold)
            {
                levelCompleted = true;
                EndEpisode();
            }
            else
            {
                // Continue the episode but reset the agent's position
                transform.position = startPosition;
                body.linearVelocity = Vector2.zero;
            }
        }
        else if (collision.CompareTag("DeathZone") || collision.CompareTag("Enemy") || collision.CompareTag("Hazard") || collision.CompareTag("Projectile"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    private int GetGroundedState()
    {
        Bounds bounds = GetComponent<BoxCollider2D>().bounds;

        RaycastHit2D hitDown = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.down, 0.02f, platformLayer);
        RaycastHit2D hitLeft = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.left, 0.02f, platformLayer);
        RaycastHit2D hitRight = Physics2D.BoxCast(bounds.center, bounds.size, 0f, Vector2.right, 0.02f, platformLayer);

        if (hitDown.collider != null)
            return 0; // On floor
        if (hitLeft.collider != null)
            return 1; // On left wall
        if (hitRight.collider != null)
            return 2; // On right wall

        return -1; // In air
    }
}
