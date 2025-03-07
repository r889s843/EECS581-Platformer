// Coins.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/16/2025
// Course: EECS 582
// Purpose: Adds money to the game and manages their collection

using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1; // Value of the coin

    public float amp = 0.1f;
    public float freq = 3f;
    public float dissolveSpeed = 2f;
    private Dissolve dissolveEffect;

    private bool triggered = false;


    Vector3 initPos;
    private void Start()
    {
        initPos = transform.position;
        dissolveEffect = GetComponent<Dissolve>();
    }

    private void Update()
    {
        transform.position = new Vector3(initPos.x, Mathf.Sin(Time.time * freq) * amp + initPos.y, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !triggered) // Make sure Player has "Player" tag
        {
            triggered = true;
            PlayerMoneyManager.Instance.AddMoney(coinValue);

            Renderer renderer = GetComponent<Renderer>();
            Material newMat = new Material(dissolveEffect.material);
            newMat.SetFloat("_DissolveAmount", 0f); // Reset dissolve amount
            renderer.material = newMat;
            dissolveEffect.material = newMat;
            dissolveEffect.StartDissolve(dissolveSpeed);
            StartCoroutine(DestroyAfterDissolve());
        }
    }

    private System.Collections.IEnumerator DestroyAfterDissolve()
    {
        yield return new WaitForSeconds(1.5f / dissolveSpeed); // Wait until dissolve completes
        Destroy(gameObject);
        // Debug.Log("I should be dead dead dead");
    }
}
