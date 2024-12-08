using UnityEngine;

public class CoOpInitialize : MonoBehaviour
{
    [SerializeField] private GameObject player2Prefab; //prefab of palayer 2 to create
    private GameObject player2; //new player2's game object
    public static int coopTrigger = 0;
    private bool activated; //tracks whether coop has been activated already or not

    private CameraController cameraControllerScript; //camera controller script to tell that player 2 has been activated

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraControllerScript = Camera.main.GetComponent<CameraController>(); //get camera controller script
    }

    // Update is called once per frame
    void Update()
    {
        if(coopTrigger == 1  && !activated) {
            player2 = Instantiate(player2Prefab, new Vector3(3,2,0), Quaternion.identity) as GameObject;
            activated = true;

            //tell camera controller that player 2 is active
            cameraControllerScript.player2Active = true;
        }
    }
}
