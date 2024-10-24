using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
// using Platformer.Mechanics;  // Reference to KinematicObject

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

    // IsGrounded to check if the player can jump so they don't fly
    private bool IsGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center, 
                        GetComponent<BoxCollider2D>().bounds.size, 0, Vector2.down, 0.5f, platformLayer);
        return raycastHit.collider != null;
    }


    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        // kinematicObject = GetComponent<KinematicObject>();  // Get the KinematicObject component
        startPosition = transform.position;
        previousPosition = startPosition;
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent and environment
        transform.position = startPosition;
        rigidbody.linearVelocity = Vector2.zero;
        previousPosition = transform.position;

        // Optionally randomize the level
        // LevelGenerator.Instance.GenerateLevel();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collecting 2 observations for agent's position
        sensor.AddObservation(transform.localPosition / 10f); // Normalize positions (2 values)
        
        // Collecting 2 observations for agent's velocity
        sensor.AddObservation(rigidbody.linearVelocity / 10f); // Normalize velocities (2 values)
        
        // Collecting 2 observations for the direction to the goal
        Vector2 directionToGoal = (goalTransform.position - transform.position) / 10f;
        sensor.AddObservation(directionToGoal); // (2 values)
        
        // Collecting 1 observation for grounded status using IsGrounded from KinematicObject
        sensor.AddObservation(IsGrounded() ? 1f : 0f); // (1 value)

        // Raycast Debugging - visual feedback to ensure the raycasts are hitting the platforms
        Debug.DrawRay(transform.position, Vector2.right * raycastDistance, Color.green, 1.0f);
        Debug.DrawRay(transform.position, Vector2.left * raycastDistance, Color.green, 1.0f);
        Debug.DrawRay(transform.position, Vector2.down * 2f, Color.red, 1.0f);


        
        // Collecting 2 observations for raycast distances
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, platformLayer);
        sensor.AddObservation(hitRight ? hitRight.distance / raycastDistance : 1f); // (1 value)
        
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, platformLayer);
        sensor.AddObservation(hitLeft ? hitLeft.distance / raycastDistance : 1f); // (1 value)
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0];
        int jumpAction = actionBuffers.DiscreteActions[1];

        // Debugging to check the grounded state and actions
        // Debug.Log($"Move Action: {moveAction}, Jump Action: {jumpAction}, IsGrounded: {IsGrounded()}");

        Vector2 velocity = rigidbody.linearVelocity;

        // Horizontal Movement
        if (moveAction == 0)
        {
            velocity.x = 0f;
        }
        else if (moveAction == 1)
        {
            velocity.x = -speed;
        }
        else if (moveAction == 2)
        {
            velocity.x = speed;
        }

        // Jumping logic using IsGrounded from KinematicObject
        if (jumpAction == 1 && IsGrounded())
        {
            velocity.y = jumpHeight;
        }

        rigidbody.linearVelocity = velocity;

        // Rewards
        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position);
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position);
        float distanceReward = previousDistanceToGoal - distanceToGoal;
        AddReward(distanceReward * 0.1f);

        // Time penalty
        AddReward(-0.001f);

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
            Debug.Log($"Goal reached. Final reward: {GetCumulativeReward()}");
            EndEpisode();
        }
        else if (collision.CompareTag("DeathZone"))
        {
            SetReward(-5.0f);
            Debug.Log($"Agent hit the DeathZone. Final reward: {GetCumulativeReward()}");
            EndEpisode();
        }
    }
}
