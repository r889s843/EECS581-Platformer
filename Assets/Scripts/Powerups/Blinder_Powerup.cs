// Blinder_Powerup.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Defines the blind screen power up
using UnityEngine;

public class Blinder_Powerup : MonoBehaviour
{
    private BoxCollider2D boxCollider; //this object's box collider
    private Renderer spriteRenderer; //this object's renderer
    [SerializeField] private GameObject blinderPrefab; //prefab of blinder object for instantiation when activated
    private GameObject blinder; //actual instantiated blinder
    private SpriteRenderer blinderRenderer; //sprite renderer for blinder 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //calls when powerup is collided with
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Player") { //if it is the player that collided
            boxCollider.enabled = false; //remove the collider
            spriteRenderer.enabled = false; //remove renderer (invisible)

            //instantiate blinder object
            blinder = Instantiate(blinderPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            blinderRenderer = blinder.GetComponent<SpriteRenderer>(); //get renderer for fadeout
            
            //destroy blinder
            Destroy(blinder, 2f); //2sec time
        }
    }

}
