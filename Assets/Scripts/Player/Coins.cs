// Coins.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/16/2025
// Course: EECS 582
// Purpose: Adds money to the game and manages their collection

using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 10; // Value of the coin

    public float amp = 0.1f;
    public float freq = 3f;
    Vector3 initPos;
    private void Start()
    {
        initPos = transform.position;
    }

    private void Update()
    {
        transform.position = new Vector3(initPos.x, Mathf.Sin(Time.time * freq) * amp + initPos.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Make sure Player has "Player" tag
        {
            PlayerMoneyManager.Instance.AddMoney(coinValue);
            Destroy(gameObject); // Remove coin after collection
        }
    }
}
