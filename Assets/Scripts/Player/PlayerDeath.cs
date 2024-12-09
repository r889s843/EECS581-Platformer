// PlayerDeath.cs
// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 10/17/2024
// Course: EECS 581
// Purpose: Manage Player death in human-only mode.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeath : MonoBehaviour
{
    // detect any collisions with enemies or DeathZone and trigger death events (destroy)
    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("DeathZone")){ // track if the player hit the no no zone.
            Destroy(gameObject); // kill the player
        }
        // if (collision.gameObject.CompareTag("Enemy")){ // same but with enemies.
        //     Destroy(gameObject); // kill the player
        // } // WE WILL NEED TO ADD MORE HERE AS WE ADD SPIKES / ENEMY PROJECTILES
    }
}
