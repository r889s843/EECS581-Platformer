// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates a camera to track the player while the game is going.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera; //camera game object
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance; //distance that camera will look ahead
    [SerializeField] private float aboveDistance; //distance that cameral will look above

    //2 player variables
    private Transform player2; //player2's transform
    public bool player2Active; //tracks whether game is single or 2 player at the moment
    // [SerializeField] private float edgeBuffer; //space of player to edge
    [SerializeField] private float minZoom = 5.0f; //min size of camera
    [SerializeField] private float maxZoom = 20.0f; //max size of camera
    [SerializeField] private float zoomSpeed = 0.2f; //zoom speed
    private float smoothTime;//


    private void Awake() {
        mainCamera = Camera.main;

        //if player object is empty then find it
        if(player == null) {
            player = GameObject.Find("Player").transform;
        }
    }

    private void Update() {
        //get player 2 transform right as its activated
        if(player2 == null && player2Active) { //executes only once
            player2 = GameObject.Find("Player2(Clone)").transform; //assign player2's transform
        }

        //make camera follow player
        if(player2Active) { //multiplayer
            Vector3 avgPos = getNewPosition(); //get average position
            transform.position = new Vector3(avgPos.x, avgPos.y, transform.position.z); //set new position

            //set new size
            mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, getNewZoom(), ref smoothTime, zoomSpeed);
        }
        else { //single player
            transform.position = new Vector3(player.position.x + aheadDistance, player.position.y + aboveDistance, transform.position.z);
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
    //calculates based on distance between players
    private float getNewZoom(){
        float zoom = 0.0f;

        //zoom is proportional to distance between players
        zoom = Vector3.Distance(player.position, player2.position);

        //restrict zoom to min and max
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        return zoom;
    }
}
