// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: AI controls and management. This controls how the AI is rewarded, sees the world, and how it moves.

using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
// AI class
public class PlatformerAgent : Agent
{
    public Transform goalTransform; // goal-of-AI's location. Where the flag is.
    public LayerMask platformLayer; // the ground the AI can walk on.
    public float raycastDistance = 20f; // how far the AI can see around itself.
    public float speed = 10f; // how fast the AI can move left and right
    public float jumpHeight = 20f; // how high the AI can jump. These should all match the human players.
    private Rigidbody2D rigidbody; // the AI's body
    private Vector2 previousPosition; // the location of the AI
    private Vector2 startPosition; // the location of the respawn point.

    // IsGrounded to check if the player can jump so they don't fly
    private bool IsGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center, 
                        GetComponent<BoxCollider2D>().bounds.size, 0, Vector2.down, 0.5f, platformLayer);
        return raycastHit.collider != null;
    }

    // this creates the inital conditions of the AI and manages spawn stuff when the AI dies/wins.
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        // kinematicObject = GetComponent<KinematicObject>();  // Get the KinematicObject component
        startPosition = transform.position; // set spawn point.
        previousPosition = startPosition;
    }

    // when an agent dies or wins, start over.
    public override void OnEpisodeBegin()
    {
        // Reset agent and environment
        transform.position = startPosition;
        rigidbody.linearVelocity = Vector2.zero;
        previousPosition = transform.position;

        // Optionally randomize the level. This will be more useful later when we have procedural generation.
        // LevelGenerator.Instance.GenerateLevel();
    }
    // how the AI sees the world.
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

    // how the AI is rewarded for its actions and how its input choices effect the world (movement).
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int moveAction = actionBuffers.DiscreteActions[0]; // set of wasd commands.
        int jumpAction = actionBuffers.DiscreteActions[1]; // set of jump commands (space bar).

        // Debugging to check the grounded state and actions
        // Debug.Log($"Move Action: {moveAction}, Jump Action: {jumpAction}, IsGrounded: {IsGrounded()}");

        Vector2 velocity = rigidbody.linearVelocity; // grabs how fast the player is moving.

        // Horizontal Movement
        if (moveAction == 0)
        {
            velocity.x = 0f; // if action is 0, do nothing.
        }
        else if (moveAction == 1)
        {
            velocity.x = -speed; // if action is 1 (a), go left
        }
        else if (moveAction == 2)
        {
            velocity.x = speed; // if action is 2 (d), go right
        }

        // Jumping logic using IsGrounded
        if (jumpAction == 1 && IsGrounded()) // if the player is grounded and the action is 1 (space bar), jump
        {
            velocity.y = jumpHeight; // adds the jump amount to the player's y position.
        }

        rigidbody.linearVelocity = velocity; // set player velocity after the changes. Make the player move.

        // Rewards
        float distanceToGoal = Vector2.Distance(transform.position, goalTransform.position); // check how far away the AI is from flag.
        float previousDistanceToGoal = Vector2.Distance(previousPosition, goalTransform.position); // check if the AI is going towards the flag.
        float distanceReward = previousDistanceToGoal - distanceToGoal; // calculate the change.
        AddReward(distanceReward * 0.1f); // increase the reward for the AI going toward the flag.

        // Time penalty
        AddReward(-0.001f); // add penalty to make the AI act as fast as possible instead of being idle. This is the same way you make a maze running AI.

        previousPosition = transform.position; // update the position of the AI per cycle.
    }

    // controlls for the AI. Currently 5 total (no movement, left, right, no jump, jump). Will add down to slide/crouch
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions; // grab the AI's actions
        float moveInput = Input.GetAxisRaw("Horizontal"); // allow it to choose which movement option it wants.
        discreteActionsOut[0] = moveInput > 0 ? 2 : (moveInput < 0 ? 1 : 0); // set that option.
        discreteActionsOut[1] = Input.GetButton("Jump") ? 1 : 0; // set the jump option.
    }
    // trigger collidable events
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Flag")) // if the AI grabs the flag
        {
            SetReward(5.0f); // give it a high reward to incentivize winning.
            Debug.Log($"Goal reached. Final reward: {GetCumulativeReward()}"); //debugging console
            EndEpisode(); // end session and respawn and reset everything.
        }
        else if (collision.CompareTag("DeathZone"))
        {
            SetReward(-5.0f); // give it a high negative reward for dying.
            Debug.Log($"Agent hit the DeathZone. Final reward: {GetCumulativeReward()}"); //debugging console
            EndEpisode(); // end session and respawn and reset everything.
        }
        else if (collision.CompareTag("Enemy"))
        {
            SetReward(-4.0f); // give it a high negative reward for dying.
            Debug.Log($"Agent hit the Enemy. Final reward: {GetCumulativeReward()}"); //debugging console
            EndEpisode(); // end session and respawn and reset everything.
        }
    } // add any more here for enemies, enemy projectiles, and environment hazards.
}
