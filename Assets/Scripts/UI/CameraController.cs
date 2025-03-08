// CameraController.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track the player while the game is going.

//TODO
//camera lookahead and raycast in player direction - super buggy idk why
//multiplayer cam

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera; //camera game object
    private Transform player; //player's transform
    private Rigidbody2D playerBody; //player's rigidbody
    private PlayerMovement playerMovement; //player's movement script

    [Header("Dynamic Settings")]
    [SerializeField] private float heightChangeSpeed; //speed that cam changes height
    [SerializeField] private float zoomSpeed; //zoom speed

    [Header("Static Settings")]
    [SerializeField] private float minZoom; //min size of camera
    [SerializeField] private float maxZoom; //max size of camera
    [SerializeField] private float aheadDistance; //distance camera centers ahead of player
    [SerializeField] private float rayDistanceAhead; //distance that the raycast will look ahead to detect lower ledges

    //height & zoom
    private float camHeight; //y value hight of camera
    private float groundLevel; //y value of ground in level
    private float zoom; //camera's zoom value
    private float zoomOffset; //vertical offset to move camera to account for zooming - keeps camera anchored on ground

    //interpolation
    private float zoomCurrentVelo;
    private float heightCurrentVelo;
    private float lookAheadCurrentVelo;

    //2 player variables
    private Transform player2; //player2's transform
    public bool player2Active; //tracks whether game is single or 2 player at the moment


    private void Awake() {
        //if player object is empty then find it
        if(player == null) {
            GameObject playerGameObject;
            playerGameObject = GameObject.Find("Player");
            player = playerGameObject.transform;
            playerMovement = playerGameObject.GetComponent<PlayerMovement>();
        }
        playerBody = player.GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;
        groundLevel = player.transform.position.y - 0.5f;
        camHeight = groundLevel + 4.0f;
    }

    private void Update() {
        //get player 2 transform right as its activated
        if(player2 == null && player2Active) { //executes only once
            player2 = GameObject.Find("Player2(Clone)").transform; //assign player2's transform
        }

        /**
        //calculate look ahead
        float lookAhead = 0.0f;
        if(player.localScale.x > 0) { //player facing forward
            //lookAhead = Mathf.SmoothDamp(lookAhead, aheadDistance, ref smoothTimeLookAhead, zoomSpeed);
            lookAhead = aheadDistance;
        }
        else { //player facing backward
            lookAhead = Mathf.SmoothDamp(lookAhead, -1*aheadDistance, ref smoothTimeLookAhead, zoomSpeed);
        }dont forget to change aheadDistance to lookAhead in actual transform.pos change
        **/

        //update ground level
        groundLevel = calcGroundLevel();

        //update zoom
        zoom = calcZoom();
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref zoomCurrentVelo, zoomSpeed); //set new zoom

        //update camera position
        if(player2Active) { //multiplayer
            Vector3 avgPos = getNewPosition(); //get average position
            transform.position = new Vector3(avgPos.x, avgPos.y, transform.position.z); //set new position
        }
        else { //single player
            zoomOffset = zoom - minZoom;
            camHeight = Mathf.SmoothDamp(camHeight, (groundLevel + 4.0f + zoomOffset), ref heightCurrentVelo, heightChangeSpeed);
            //lerp camHeight instead of smoothdamp

            

            transform.position = new Vector3(player.position.x + aheadDistance, camHeight, transform.position.z); 
        }
    }

    //returns new camera zoom 
    //single player -> calculates based on distance from ground level
    //2player -> calculates based on distance between players
    private float calcZoom()
    {
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

    //returns new ground level
    private float calcGroundLevel()
    {
        float newGroundLevel = groundLevel;

        //if player is too far up -> move gl up to them
        if(zoom > maxZoom) {
            //set new groundLevel based on player's position only when they are on ground
            if(playerMovement.getGrounded()) {
                newGroundLevel = player.transform.position.y - 0.5f;
            }
        }

        //tester box to visualize


        //if a platform ahead of the player is below current ground level -> lower it to there
        Vector2 boxSize = new Vector2(rayDistanceAhead,1);
        Vector2 rayOrigin = Vector2.zero;
        rayOrigin = new Vector2(player.position.x + (boxSize.x / 2), player.position.y - 6f); //-6 is to keep cast below current ground when player jumps

        RaycastHit2D[] hits = Physics2D.BoxCastAll(rayOrigin, boxSize, 0f, Vector2.down);//raycast
        for(int i = 0; i < hits.Length; i++) {
            RaycastHit2D hit = hits[i];
            if(hit && hit.collider.gameObject.name != "DeathZone"){
                newGroundLevel = hit.point.y;
            }
        }

        /*
        if(detector && detector.collider.gameObject.name != "DeathZone") { //set new gl if ground detected
            Debug.Log("detected " + detector.point);
            newGroundLevel = detector.point.y;
        }*/

        return newGroundLevel;
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

    public void UpdateCameraTarget(Transform newPlayer, Transform newPlayer2 = null)
    {
        player = newPlayer;
        player2 = newPlayer2;
        player2Active = newPlayer2 != null;
    }

}
