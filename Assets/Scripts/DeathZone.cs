// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: This creates the DeathZone object which unity uses to track what kills a player. This is defined in every level separately and is being used to track when the player falls off the platform into the abyss.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DeathZone components mark a collider which will schedule a event when the player enters the trigger.
public class DeathZone : MonoBehaviour
{
    public Transform Respawn;

    // THIS WORKS BUT I HAVE IT TURNED OFF FOR THE AI. THIS SHOULDN'T NEED MUCH UPDATING UNLESS WE PLAN TO ADD HEALTH
    // void OnTriggerEnter2D(Collider2D collider)
    // {
    //     collider.gameObject.GetComponent<PlayerMovement>(); // this checks the players movement when a collison occurs
    //     collider.transform.position = Respawn.position; // this teleports the player back to the respawn point.
    // }
}
// }