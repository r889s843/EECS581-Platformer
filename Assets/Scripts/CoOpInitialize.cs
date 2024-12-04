using UnityEngine;

public class CoOpInitialize : MonoBehaviour
{
    [SerializeField] private GameObject player2Prefab; //prefab of palayer 2 to create
    private GameObject player2; //new player2's game object
    private bool activated; //tracks whether coop has been activated already or not

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C) && !activated) {
            player2 = Instantiate(player2Prefab, Vector3.zero, Quaternion.identity) as GameObject;
            activated = true;
        }
    }
}
