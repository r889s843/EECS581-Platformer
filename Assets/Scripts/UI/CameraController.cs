// CameraController.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track the player while the game is going.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera; //camera game object
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance; //distance camera centers ahead of player
    private float cameraHeight; //y value hight of camera
    private float groundLevel; //y value of ground in level
    private float zoomOffset; //distance to move camera upward when zooming out so ground stays anchored at bottom of screen
    private float zoomPreClamp;
    private float zoom; //camera's zoom value

    //2 player variables
    private Transform player2; //player2's transform
    public bool player2Active; //tracks whether game is single or 2 player at the moment
    // [SerializeField] private float edgeBuffer; //space of player to edge
    [SerializeField] private float minZoom = 5.0f; //min size of camera
    [SerializeField] private float maxZoom = 20.0f; //max size of camera
    [SerializeField] private float zoomSpeed = 0.2f; //zoom speed
    private float smoothTime;


    private void Awake() {
        //if player object is empty then find it
        if(player == null) {
            player = GameObject.Find("Player").transform;
        }

        mainCamera = Camera.main;
        groundLevel = player.transform.position.y - 0.5f;
        cameraHeight = groundLevel + 4.0f;
        zoomOffset = 0.0f;
    }

    private void Update() {
        //get player 2 transform right as its activated
        if(player2 == null && player2Active) { //executes only once
            player2 = GameObject.Find("Player2(Clone)").transform; //assign player2's transform
        }

        //update zoom
        updateZoom();
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref smoothTime, zoomSpeed); //set new zoom

        //need to smooth damp zoomOffset here 
        //zoomOffset = zoom / 2 idea works - just need zoom before clamp here

        //update camera position
        if(player2Active) { //multiplayer
            Vector3 avgPos = getNewPosition(); //get average position
            transform.position = new Vector3(avgPos.x, avgPos.y, transform.position.z); //set new position
        }
        else { //single player
            cameraHeight = (groundLevel + 4.0f) + zoomOffset; //calculate new camera height
            transform.position = new Vector3(player.position.x + aheadDistance, cameraHeight, transform.position.z); //set new position
        }

    }

    //returns average position of both players
    private Vector3 getNewPosition()
    {
        Vector3 averagePosition = new Vector3();

        //add both player's positions together
        averagePosition += player.position;
        averagePosition += player2.position;

        //calculate average
        averagePosition /= 2;

        return averagePosition;
    }

    //returns new camera zoom
    //single player -> calculates based on distance from ground level
    //2player -> calculates based on distance between players
    private void updateZoom(){
        //2 player
        if(player2Active){
            zoom = Vector3.Distance(player.position, player2.position);//zoom is proportional to distance between players
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom); //restrict zoom to min and max
        }
        else { //single player
            zoom = (player.position.y - 0.5f) - groundLevel; //zoom is proportional to player's distance from the ground
            zoom = Mathf.Clamp(zoom, minZoom, zoom); //restrict zoom to min
        }
    }

    public void UpdateCameraTarget(Transform newPlayer, Transform newPlayer2 = null)
    {
        player = newPlayer;
        player2 = newPlayer2;
        player2Active = newPlayer2 != null;
    }

}
