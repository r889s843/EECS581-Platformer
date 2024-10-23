using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private BoxCollider2D boxCollider; //reference to player's box collider
    [SerializeField] private LayerMask groundLayer; //holds ground layer mask
    [SerializeField] private LayerMask wallLayer; //holds wall layer mask
    [SerializeField] private float speed; //player movement speed
    [SerializeField] private float jumpHeight; //player jump height
    private float horizontalInput; //horizontal input on player
    private float wallJumpCooldown; //timer to cool down wall jump

    private void Awake() {
        //Get components and store on startup
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        horizontalInput = Input.GetAxis("Horizontal"); //stores horizontal input
        body.linearVelocity = new Vector2(horizontalInput * speed, body.linearVelocity.y); //moves player left and right

        //flip player when moving
        if (horizontalInput > 0.01f) { //if player is moving right
            transform.localScale = Vector3.one;//face player right
        }
        else if (horizontalInput < -0.01f) {//if player is moving left
            transform.localScale = new Vector3(-1, 1, 1);//flip player left
        }

        if (wallJumpCooldown > 0.1f) {
            //wall jumping
            if (onWall() && !isGrounded()) {
                body.gravityScale = 4.5f;
                body.linearVelocity = Vector2.zero;
            }
            else {
                body.gravityScale = 5;
            }

            //if space is pressed then jump
            if(Input.GetKeyDown(KeyCode.Space)) { 
                jump();
            }
        }
        else {
            wallJumpCooldown += Time.deltaTime; 
        }

    }

    //jump handler
    private void jump() {
        if (isGrounded()) { //allows jump only if on ground
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpHeight); //jump
        }
        else if (onWall() && !isGrounded()) { //on a wall and not on ground
            if (horizontalInput == 0) {
                body.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 60, 20);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else {
                body.linearVelocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 60, 20);
            }
        }

        wallJumpCooldown = 0;
    }

    //returns true if player is on the ground
    private bool isGrounded() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.5f, groundLayer);
        return raycastHit.collider != null;
    }

    //returns true if player is on a wall
    private bool onWall() {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.05f, wallLayer);
        return raycastHit.collider != null;
    }

}
