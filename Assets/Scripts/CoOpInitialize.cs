// CoOpInitialize.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/24/2024
// Course: EECS 581
// Purpose: Intializes the game state to render, spawn, and update the 2nd player

using Unity.VisualScripting;
using UnityEngine;

public class CoOpInitialize : MonoBehaviour
{
    // [SerializeField] private GameObject player2Prefab; //prefab of palayer 2 to create
    [SerializeField] private GameObject player2;
    public static int coopTrigger = 0;
    private bool activated; //tracks whether coop has been activated already or not
    // Inspector-assigned references
    [SerializeField] private LivesUI livesUIScript; // Assign in Inspector
    [SerializeField] private PowerUpManager powerUpManager; // Assign in Inspector
    private CameraController cameraControllerScript; // Assign in Inspector

    [SerializeField] private GameObject inventories; // Player 1 inventory
    [SerializeField] private GameObject inventoriesP2; // Player 2 inventory

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraControllerScript = Camera.main.GetComponent<CameraController>(); // Get camera controller script
    }

    // Update is called once per frame
    void Update()
    {
        //activate coop for testing
        if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            coopTrigger = 1;
        }

        if(coopTrigger == 1  && !activated) {

            GameObject player1 = GameObject.FindGameObjectWithTag("Player");

            // GameObject aiObject = GameObject.FindGameObjectWithTag("AI");
            // Destroy(aiObject);

            Vector3 newPos = player1.transform.position;
            newPos.y += 2f;
            player2.transform.position = newPos;

            // if (player2 != null)
            player2.SetActive(true);

            // player2 = Instantiate(player2Prefab, new Vector3(3,2,0), Quaternion.identity) as GameObject;
            activated = true;

            //tell camera controller that player 2 is active
            cameraControllerScript.startCoop();

            livesUIScript.P2 = true;
            livesUIScript.UpdateLivesDisplayP2(); // Update P2 lives display

            // Relocate UI elements
            HandleInventoryUI();
        }
    }

    private void HandleInventoryUI()
    {
        if (inventories != null)
        {
            // Move Player 1 inventory to the left side (e.g., top-left corner)
            inventories.GetComponent<RectTransform>().anchoredPosition = new Vector2(-800, 60);
        }

        if (inventoriesP2 != null)
        {
            // Enable Player 2 inventory
            inventoriesP2.SetActive(true);
            // Move Player 2 inventory to the right side (e.g., top-right corner)
            inventoriesP2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-800, -475);
        }
    }
}
