using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerDeath : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("DeathZone")){
            Destroy(gameObject);
            Debug.Log("AYYYY");
        }
        // if (collision.gameObject.CompareTag("Enemy")){
        //     Destroy(gameObject);
        // }
    }
}
