// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track the player while the game is going.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance; //distance that camera will look ahead
    [SerializeField] private float cameraSpeed; //camera speed for switching look ahead directions
    private float lookAhead; //distance and direction for camera to look ahead and not be centered on player

    private void Awake() {
        //sets camera position to correct lookahead on startup
        lookAhead = aheadDistance * player.localScale.x;
        transform.position = new Vector3(player.position.x + lookAhead, transform.position.y, transform.position.z);
    }

    private void Update() {
        //follow player
        transform.position = new Vector3(player.position.x + lookAhead, transform.position.y, transform.position.z); //actually moves camera to player's position
        lookAhead = Mathf.Lerp(lookAhead, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed); //slowly transition camera to final lookahead
    }
}
