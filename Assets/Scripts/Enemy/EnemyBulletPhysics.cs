// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/08/2024
// Course: EECS 581
// Purpose: Bullet Physics for Enemy guns

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletPhysics : MonoBehaviour
{
    public GameObject player;
    private Rigidbody2D bullet;
    public float force;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bullet = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        Vector2 direction = player.transform.position - transform.position;
        bullet.linearVelocity = new Vector2(direction.x, direction.y).normalized * force;

        float rotation = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotation + 180);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 5){
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")){
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Ground")){
            Destroy(gameObject);
        }
    }
}
