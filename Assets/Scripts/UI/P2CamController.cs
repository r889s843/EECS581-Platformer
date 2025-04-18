// P2CamController.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 3/31/2025
// Course: EECS 582
// Purpose: This controlls p2's camera to track p2

using UnityEngine;

public class P2CamController : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private Transform player; //transform of p2 for the camera to track
    private Rigidbody2D playerBody; //player's rigidbody

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //if player object is empty then find it
        if(player == null) {
            player = GameObject.Find("Player2").transform;
        }

        cam = GetComponent<Camera>();
        playerBody = player.GetComponent<Rigidbody2D>();
        groundLevel = player.transform.position.y - 0.5f;
        camHeight = groundLevel + 4.0f;
    }

    // Update is called once per frame
    void Update()
    {        
        //update ground level
        groundLevel = calcGroundLevel();

        //update zoom
        zoom = calcZoom();
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref zoomCurrentVelo, zoomSpeed); //set new zoom

        zoomOffset = zoom - minZoom;
        camHeight = Mathf.SmoothDamp(camHeight, (groundLevel + 4.0f + zoomOffset), ref heightCurrentVelo, heightChangeSpeed);
        transform.position = new Vector3(player.position.x + aheadDistance, camHeight, transform.position.z); 

    }

    //returns new ground level
    private float calcGroundLevel()
    {
        float newGroundLevel = groundLevel;

        //will implement when p2 movement mechanics are redone
        // //if player is too far up -> move gl up to them
        // if(zoom > maxZoom) {
        //     //set new groundLevel based on player's position only when they are on ground
        //     if(playerMovement.isOnFloor) {
        //         newGroundLevel = player.transform.position.y - 0.5f;
        //     }
        // }

        //if player is too low, lower ground level
        if(player.transform.position.y - groundLevel < 0.5f){
            newGroundLevel = player.transform.position.y - 0.5f;
        }

        return newGroundLevel;
    }

    private float calcZoom()
    {
        float newZoom = 0.0f;

        newZoom = (player.position.y - 0.5f) - groundLevel; //zoom is proportional to player's distance from the ground
        newZoom = Mathf.Clamp(newZoom, minZoom, newZoom); //restrict zoom to min

        return newZoom;
    }
}
