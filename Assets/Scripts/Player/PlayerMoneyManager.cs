// PlayerMoneyManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/16/2025
// Course: EECS 582
// Purpose: Adds money to the game and manages the player's current amount

using UnityEngine;
using TMPro; // Only needed if displaying money in UI

public class PlayerMoneyManager : MonoBehaviour
{
    public static PlayerMoneyManager Instance { get; private set; } // Singleton for global access

    public int currentMoney = 0;
    public TextMeshProUGUI moneyText; // UI element to display money

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    public bool SpendMoney(int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            UpdateUI();
            return true; // Purchase successful
        }
        return false; // Not enough money
    }

    private void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "" + currentMoney;
        }
    }
}
