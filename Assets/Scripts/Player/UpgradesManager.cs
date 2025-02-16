// UpgradesManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/16/2025
// Course: EECS 582
// Purpose: Adds the functionality to upgrade abilities in the store

using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    public int upgradeCost = 10;

    public void PurchaseUpgrade()
    {
        if (PlayerMoneyManager.Instance.SpendMoney(upgradeCost))
        {
            Debug.Log("Upgrade Purchased!");
            // Apply upgrade logic (e.g., increase power-up duration)
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}
