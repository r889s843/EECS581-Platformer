using UnityEngine;

public class RaceController : MonoBehaviour
{
    //player
    [SerializeField] private GameObject player;
    private Rigidbody2D playerBody;
    private PlayerDeath playerDeath;

    //ai
    [SerializeField] private GameObject ai;
    private Animator aiAnimator;
    private PlayerMovement aiMovement;
    private Rigidbody2D aiBody;
    private Vector2 startPos;

    //flag
    [SerializeField] private GameObject flag;
    private BoxCollider2D flagCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get player components
        playerBody = player.GetComponent<Rigidbody2D>();
        playerDeath = player.GetComponent<PlayerDeath>();

        //get ai components
        aiAnimator = ai.GetComponent<Animator>();
        aiMovement = ai.GetComponent<PlayerMovement>();
        aiBody = ai.GetComponent<Rigidbody2D>();
        startPos = ai.transform.position;

        //get flag components
        flagCollider = flag.GetComponent<BoxCollider2D>();

        //set ai inactive to start
        aiActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerBody.linearVelocity.x > 0) {
            aiActive(true);
        }

        if (playerDeath.getIsDead()) {
            aiActive(false);
            ai.transform.position = startPos;
            Debug.Log("Player Died");
        }
    }

    private void aiActive(bool active) 
    {
        aiAnimator.enabled = active;
        aiMovement.enabled = active;

        if (!active) {
            aiBody.linearVelocity = Vector2.zero;
        }
    }
}
