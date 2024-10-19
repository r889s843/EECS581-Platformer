using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    [SerializeField] private float speed;
    [SerializeField] private float jumpHeight;

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        body.linearVelocity = new Vector2(Input.GetAxis("Horizontal") * speed, body.linearVelocity.y);

        if(Input.GetKeyDown(KeyCode.Space)) {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpHeight);
        }
    }
}
