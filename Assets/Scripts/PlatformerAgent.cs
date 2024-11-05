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

    public float raycastDistance = 20f;
    private Rigidbody2D rigidbody;
    private Vector2 previousPosition;
    private Vector2 startPosition;

    private PlayerMovement playerMovement;

    // Completion tracking
    public int levelCompletionThreshold = 100; // Number of times to complete the level before moving on
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
        sensor.AddObservation(IsGrounded() ? 1f : 0f);

        // Wall status
        int wallSide = OnWall();
        sensor.AddObservation(wallSide); // -1 for left wall, 1 for right wall, 0 for none

        // Raycasts in all directions to detect platforms, enemies, and death zones
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, detectableLayers);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, detectableLayers);
        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, detectableLayers);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, detectableLayers);

        // Process right raycast result
        if (hitRight.collider != null)
        {
            sensor.AddObservation(hitRight.distance / raycastDistance);
            sensor.AddObservation(GetObjectType(hitRight.collider.tag));
        }
        else
        {
            sensor.AddObservation(1f); // No object detected within range
            sensor.AddObservation(0f); // No object type detected
        }

        // Process left raycast result
        if (hitLeft.collider != null)
        {
            sensor.AddObservation(hitLeft.distance / raycastDistance);
            sensor.AddObservation(GetObjectType(hitLeft.collider.tag));
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        // Process up raycast result
        if (hitUp.collider != null)
        {
            sensor.AddObservation(hitUp.distance / raycastDistance);
            sensor.AddObservation(GetObjectType(hitUp.collider.tag));
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        // Process down raycast result
        if (hitDown.collider != null)
        {
            sensor.AddObservation(hitDown.distance / raycastDistance);
            sensor.AddObservation(GetObjectType(hitDown.collider.tag));
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        // Debugging raycasts
        Debug.DrawRay(transform.position, Vector2.right * raycastDistance, Color.green, 0.1f);
        Debug.DrawRay(transform.position, Vector2.left * raycastDistance, Color.green, 0.1f);
        Debug.DrawRay(transform.position, Vector2.up * raycastDistance, Color.green, 0.1f);
        Debug.DrawRay(transform.position, Vector2.down * raycastDistance, Color.red, 0.1f);
    }

    // Function to convert object tags to observation values
    private float GetObjectType(string tag)
    {
        switch (tag)
        {
            case "Platform":
                return 1f;
            case "Enemy":
                return 2f;
            case "DeathZone":
                return 3f;
            default:
                return 0f; // No object detected or unknown tag
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

        // Reset level completion counter for the new level
        currentLevelCompletions = 0;
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
            if (currentLevelCompletions >= levelCompletionThreshold)
            {
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
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.down, 0.5f, platformLayer
        );
        return raycastHit.collider != null;
    }

    // Check if the agent is on a wall
    private int OnWall()
    {
        RaycastHit2D hitLeft = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.left, 0.2f, platformLayer
        );

        RaycastHit2D hitRight = Physics2D.BoxCast(
            GetComponent<BoxCollider2D>().bounds.center,
            GetComponent<BoxCollider2D>().bounds.size, 0,
            Vector2.right, 0.2f, platformLayer
        );

        if (hitLeft.collider != null)
        {
            return -1; // On left wall
        }
        if (hitRight.collider != null)
        {
            return 1; // On right wall
        }

        return 0; // Not on a wall
    }
}
