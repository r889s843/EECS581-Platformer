using System.Collections;
using System.Collections.Generic;
// using Platformer.Gameplay;
using UnityEngine;
// using static Platformer.Core.Simulation;

// namespace Platformer.Mechanics
// {
    /// <summary>
    /// DeathZone components mark a collider which will schedule a
    /// PlayerEnteredDeathZone event when the player enters the trigger.
    /// </summary>
public class DeathZone : MonoBehaviour
{
    public Transform Respawn;

    // THIS WORKS BUT I HAVE IT TURNED OFF FOR THE AI. THIS SHOULDN'T NEED MUCH UPDATING UNLESS WE PLAN TO ADD HEALTH
    // void OnTriggerEnter2D(Collider2D collider)
    // {
    //     collider.gameObject.GetComponent<PlayerMovement>();
    //     collider.transform.position = Respawn.position;
    // }
}
// }