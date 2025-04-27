using UnityEngine;

public class ChaseController : MonoBehaviour
{
    //player
    [SerializeField] private GameObject player; //player game object
    private Rigidbody2D playerBody; //player's physics body
    private PlayerDeath playerDeath; //player's death script

    //enemy chaser
    [SerializeField] private GameObject chaser; //chaser enemy's game object
    private EnemyMovement enemyMovement; //enemy movement script
    private EnemyShooter enemyShooter; //enemy shooter script
    private Rigidbody2D chaserBody; //chaser's physics body
    private Animator chaserAnimator; //chaser's animator component
    private Vector2 startPos; //chaser's starting position

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get all components
        playerBody = player.GetComponent<Rigidbody2D>();
        playerDeath = player.GetComponent<PlayerDeath>();
        enemyMovement = chaser.GetComponent<EnemyMovement>();
        enemyShooter = chaser.GetComponent<EnemyShooter>();
        chaserBody = chaser.GetComponent<Rigidbody2D>();
        chaserAnimator = chaser.GetComponent<Animator>();
        startPos = chaser.transform.position;

        //disable enemy movement to wait for player to start
        enemyMovement.enabled = false;
        enemyShooter.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        //start chaser when player first moves
        if (playerBody.linearVelocity.x > 0) {
            chase(true);
        }

        //when player dies diable chaser and move it to start pos
        if (playerDeath.getIsDead()) {
            chase(false);
            chaser.transform.position = startPos;
        }

    }

    private void chase(bool active)
    {
        enemyMovement.enabled = active;
        enemyShooter.enabled = active;
        chaserAnimator.enabled = active;

        if (!active) {
            chaserBody.linearVelocity = Vector2.zero;
        }
    }
}
