// CoOpInitialize.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/24/2024
// Course: EECS 581
// Purpose: Intializes the game state to render, spawn, and update the 2nd player

using UnityEngine;

public class CoOpInitialize : MonoBehaviour
{
    [SerializeField] private GameObject player2Prefab; //prefab of palayer 2 to create
    private GameObject player2; //new player2's game object
    public static int coopTrigger = 0;
    private bool activated; //tracks whether coop has been activated already or not
    private LivesUI livesUIScript; // Reference to the LivesUI script

    private CameraController cameraControllerScript; //camera controller script to tell that player 2 has been activated

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraControllerScript = Camera.main.GetComponent<CameraController>(); //get camera controller script

        livesUIScript = FindObjectOfType<LivesUI>();
    }

    // Update is called once per frame
    void Update()
    {
        //activate coop for testing
        if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            coopTrigger = 1;
        }

        if(coopTrigger == 1  && !activated) {
            // GameObject aiObject = GameObject.FindGameObjectWithTag("AI");
            // Destroy(aiObject);

            player2 = Instantiate(player2Prefab, new Vector3(3,2,0), Quaternion.identity) as GameObject;
            activated = true;

            //tell camera controller that player 2 is active
            cameraControllerScript.startCoop();

            livesUIScript.P2 = true;
            livesUIScript.UpdateLivesDisplayP2(); // Update P2 lives display
        }
    }
}
