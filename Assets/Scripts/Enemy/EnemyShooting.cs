// Name: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 11/08/2024
// Course: EECS 581
// Purpose: Bullet Controller for enemies

using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    public GameObject bullet;
    public Transform bulletPos;
    private AudioSource audioSource;
    private float timer;
    private GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        timer += Time.deltaTime;
        if (distance < 10){
            if (timer > 2){
                timer = 0;
                shoot();
            }
        }
    }

    void shoot()
    {
        audioSource.Play();
        Instantiate(bullet, bulletPos.position, Quaternion.identity);
    }
}
