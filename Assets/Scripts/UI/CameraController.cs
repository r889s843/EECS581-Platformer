// CameraController.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track player 1 

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
    [SerializeField] private float lookDirChangeSpeed; //speed that the camera changes looking direction

    [Header("Static Settings")]
    [SerializeField] private float minZoom; //min size of camera
    [SerializeField] private float maxZoom; //max size of camera
    [SerializeField] private float aheadDistance; //distance camera centers ahead of player
    [SerializeField] private float rayDistanceAhead; //distance that the raycast will look ahead to detect lower ledges

    //cam pos & zoom
    private float camHeight; //y value hight of camera
    private float lookDir; //value between -1 and 1 to dictate looking direction
    private float groundLevel; //y value of ground in level
    private float zoom; //camera's zoom value
    private float zoomOffset; //vertical offset to move camera to account for zooming - keeps camera anchored on ground

    //interpolation
    private float zoomCurrentVelo;
    private float heightCurrentVelo;
    private float lookAheadCurrentVelo;
    private float lookDirDampVelo;

    [Header("P2 Objects")]
    [SerializeField] private GameObject p2cam;


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
        //camera direction
        lookDir = Mathf.SmoothDamp(lookDir, player.localScale.x, ref lookDirDampVelo, lookDirChangeSpeed);

        //update ground level
        groundLevel = calcGroundLevel();

        //update zoom
        zoom = calcZoom();
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref zoomCurrentVelo, zoomSpeed); //set new zoom

        //update camera position
        zoomOffset = zoom - minZoom;
        camHeight = Mathf.SmoothDamp(camHeight, (groundLevel + 4.0f + zoomOffset), ref heightCurrentVelo, heightChangeSpeed);
        transform.position = new Vector3(player.position.x + (aheadDistance * lookDir), camHeight, transform.position.z); 
    }

    //returns new camera zoom based on distance from ground level
    private float calcZoom()
    {
        float newZoom = 0.0f;

        newZoom = (player.position.y - 0.5f) - groundLevel; //zoom is proportional to player's distance from the ground
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom); //restrict zoom to min

        return newZoom;
    }

    //returns new ground level
    private float calcGroundLevel()
    {
        float newGroundLevel = groundLevel;

        //if a platform ahead of the player is below current ground level -> lower it to there
        Vector2 boxSize = new Vector2(rayDistanceAhead,1);
        Vector2 rayOrigin = Vector2.zero;
        rayOrigin = new Vector2(player.position.x + (boxSize.x / 2), player.position.y - 6); //-6 is to keep cast below current ground when player jumps

        RaycastHit2D[] hits = Physics2D.BoxCastAll(rayOrigin, boxSize, 0f, Vector2.down);//raycast
        for(int i = 0; i < hits.Length; i++) {
            RaycastHit2D hit = hits[i];
            if(hit && hit.collider.gameObject.name != "DeathZone"){
                if(hit.point.y < newGroundLevel){
                    newGroundLevel = hit.point.y;
                }
            }
        }

        return newGroundLevel;
    }

    public void UpdateCameraTarget(Transform newPlayer, Transform newPlayer2 = null)
    {
        // player = newPlayer;
        // player2 = newPlayer2;
        // player2Active = newPlayer2 != null;
    }

    public void startCoop()
    {
        Instantiate(p2cam, new Vector3(player.position.x, player.position.y, mainCamera.transform.position.z), Quaternion.identity); //instantiate p2 camera

        mainCamera.rect = new Rect(0.0f, 0.5f, 1.0f, 0.5f); //change main cam to split sceen view
    }

}
