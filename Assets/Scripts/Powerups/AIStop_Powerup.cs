// AIStop_Powerups.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Defines the stop AI player powerup collision and behavior

using UnityEngine;

public class AIStop_Powerup : MonoBehaviour
{
    // private BoxCollider2D boxCollider; //this object's box collider
    // private Renderer spriteRenderer; //this object's renderer
    private GameObject aiObject; //object of AI character
    private PlayerMovement aiMovement; //player movement component (script) of AI character
    private Rigidbody2D aiBody; //ai's rigidbody

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // boxCollider = GetComponent<BoxCollider2D>();
        // spriteRenderer = GetComponent<Renderer>();
        aiObject = GameObject.Find("AI");
        aiMovement = aiObject.GetComponent<PlayerMovement>();
        aiBody = aiObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //calls when powerup is collided with
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Player") { //if it is the player that collided
            // boxCollider.enabled = false; //remove the collider
            // spriteRenderer.enabled = false; //remove renderer (invisible)
            ActivatePowerup(); // activate it
        }
    }

    public void ActivatePowerup()
    {
        //disable ai movement
        aiMovement.enabled = false;
        aiBody.linearVelocity = new Vector2(0, 0);

        //after a delay of 3 sec, re-enable ai movement
        Invoke("delay", 3);
    }

    void delay() {
        Debug.Log("butts");
        aiMovement.enabled = true;
    }
}
