// CameraController.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track the player while the game is going.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera; //camera game object
    [SerializeField] private Transform player; //player's transform
    private Rigidbody2D playerBody; //player's rigidbody
    [SerializeField] private float aheadDistance; //distance camera centers ahead of player
    private float smoothTime;
    private float camHeight; //y value hight of camera
    private float zoomOffset; //vertical offset to move camera to account for zooming - keeps camera anchored on ground
    private float zoom; //camera's zoom value
    [SerializeField] private float groundLevel; //y value of ground in level
    private float targetGroundLevel; //y value of target ground level for smooth damp

    //2 player variables
    private Transform player2; //player2's transform
    public bool player2Active; //tracks whether game is single or 2 player at the moment
    [SerializeField] private float minZoom; //min size of camera
    [SerializeField] private float maxZoom; //max size of camera
    [SerializeField] private float zoomSpeed; //zoom speed

    //NOTES HERE
    //maybe I need to not damp zoomOffset but damp camHeight on old controller




    private void Awake() {
        //if player object is empty then find it
        if(player == null) {
            player = GameObject.Find("Player").transform;
        }
        playerBody = player.GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;
        groundLevel = player.transform.position.y - 0.5f;
        targetGroundLevel = groundLevel;
        camHeight = groundLevel + 4.0f;
        zoomOffset = 0.0f;
    }

    private void Update() {
        //get player 2 transform right as its activated
        if(player2 == null && player2Active) { //executes only once
            player2 = GameObject.Find("Player2(Clone)").transform; //assign player2's transform
        }

        /**NEW CONTROLLER WORKS BUT NOT WITH CHANGING GL
        //calc groundLevel
        //groundLevel = groundLevel;

        zoom = calcZoom(groundLevel, player.position.y); //calc zoom
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref smoothTime, zoomSpeed); //set zoom

        camHeight = Mathf.SmoothDamp(camHeight, calcCamHeight(groundLevel, zoom), ref smoothTime, zoomSpeed); //calc height and smooth damp
        transform.position = new Vector3(player.position.x + aheadDistance, camHeight, transform.position.z); //set position
        **/
        

        ///** OLD CONTROLLER - SUPER UGLY BUT WORKS 
        //update groundLevel
        //if player is too far up -> move gl up to them
        if(zoom > maxZoom) {
            //set new groundLevel based on player's position only when they are on ground
            if(playerBody.linearVelocity.y == 0) {
                targetGroundLevel = player.transform.position.y - 0.5f;
            }
        }
        //groundLevel = Mathf.SmoothDamp(groundLevel, targetGroundLevel, ref smoothTime, zoomSpeed); DOESNT WORK
        groundLevel = targetGroundLevel; //works but not smooth

        //update zoom
        zoom = getZoom();
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref smoothTime, zoomSpeed); //set new zoom

        //update camera position
        if(player2Active) { //multiplayer
            Vector3 avgPos = getNewPosition(); //get average position
            transform.position = new Vector3(avgPos.x, avgPos.y, transform.position.z); //set new position
        }
        else { //single player
            zoomOffset = Mathf.SmoothDamp(zoomOffset, zoom - minZoom, ref smoothTime, zoomSpeed); //calculate and smooth damp zoomOffset
            camHeight = (groundLevel + 4.0f) + zoomOffset; //calculate new camera height
            //camHeight = Mathf.SmoothDamp(camHeight, targetCamHeight, ref smoothTime, zoomSpeed); //smooth damp cam height transition
            transform.position = new Vector3(player.position.x + aheadDistance, camHeight, transform.position.z); //set new position
        }
        //**/
    }

    //calculate camera zoom
    //gl = groundLevel
    //playerY = player's current y pos
    private float calcZoom(float gl, float playerY)
    {
        float newZoom = (playerY - 0.5f) - gl; //zoom proportional to distance player is from gl
        newZoom = Mathf.Clamp(newZoom, minZoom, newZoom); //restrict zoom to min
        return newZoom;
    }

    //calculate camera height
    //baseOffset -> makes camera look above player not directly at it - currently static at 4.0
    //groundLevel = bottom anchor for cam
    //zoomOffset = amount that cam needs to move up while zooming to stay anchored at bottom
    //camHeight = groundLevel + zoomOffset + baseOffset
    private float calcCamHeight(float gl, float zoom)
    {
        float newCamHeight = 0.0f;
        float zoomOffset = zoom - minZoom;
        newCamHeight = gl + zoomOffset + 4.0f;

        return newCamHeight;
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
    private float getZoom(){
        float newZoom = 0.0f;
        //2 player
        if(player2Active){
            newZoom = Vector3.Distance(player.position, player2.position);//zoom is proportional to distance between players
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom); //restrict zoom to min and max
        }
        else { //single player
            newZoom = (player.position.y - 0.5f) - groundLevel; //zoom is proportional to player's distance from the ground
            newZoom = Mathf.Clamp(newZoom, minZoom, newZoom); //restrict zoom to min
        }

        return newZoom;
    }

    public void UpdateCameraTarget(Transform newPlayer, Transform newPlayer2 = null)
    {
        player = newPlayer;
        player2 = newPlayer2;
        player2Active = newPlayer2 != null;
    }

}
