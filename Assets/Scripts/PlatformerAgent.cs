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
    public float raycastDistance = 20f;
    public float speed = 10f;
    public float jumpHeight = 20f;
    private Rigidbody2D rigidbody;
    private Vector2 previousPosition;
    private Vector2 startPosition;

    // Completion tracking
    public int levelCompletionThreshold = 100; // Number of times to complete the level before moving on
    private int currentLevelCompletions = 0; // Tracks how many times the current level has been completed

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center,
                        GetComponent<BoxCollider2D>().bounds.size, 0, Vector2.down, 0.5f, platformLayer);
        return raycastHit.collider != null;
    }

    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        previousPosition = startPosition;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
        rigidbody.linearVelocity = Vector2.zero;
        previousPosition = transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition / 10f);
        sensor.AddObservation(rigidbody.linearVelocity / 10f);
        Vector2 directionToGoal = (goalTransform.position - transform.position) / 10f;
        sensor.AddObservation(directionToGoal);
        sensor.AddObservation(IsGrounded() ? 1f : 0f);

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, platformLayer);
        sensor.AddObservation(hitRight ? hitRight.distance / raycastDistance : 1f);

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, platformLayer);
        sensor.AddObservation(hitLeft ? hitLeft.distance / raycastDistance : 1f);

        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, platformLayer);
        sensor.AddObservation(hitUp ? hitUp.distance / raycastDistance : 1f);

        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, platformLayer);
        sensor.AddObservation(hitDown ? hitDown.distance / raycastDistance : 1f);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        int jumpAction = actionBuffers.DiscreteActions[1];

        Vector2 velocity = rigidbody.linearVelocity;

        if (moveAction == 0) velocity.x = 0f;
        else if (moveAction == 1) velocity.x = -speed;
        else if (moveAction == 2) velocity.x = speed;

        if (jumpAction == 1 && IsGrounded())
        {
            velocity.y = jumpHeight;
        }

        rigidbody.linearVelocity = velocity;

        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position);
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position);
        float distanceReward = previousDistanceToGoal - distanceToGoal;
        AddReward(distanceReward * 0.1f);

        AddReward(-0.01f);

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
}
