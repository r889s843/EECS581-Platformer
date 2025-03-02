// UpgradesManager.cs
// Authors: Chris Harvey, Ian Collins, Ryan Strong, Henry Chaffin, Kenny Meade
// Date: 2/16/2025
// Course: EECS 582
// Purpose: Adds the functionality to upgrade abilities in the store

using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    public PowerUpManager powerUpManager;
    public GameObject abilityToActivate; // optional: a game object that should become active
    public int cost = 10;

    // Which ability are we unlocking?
    public enum AbilityType { Invincibility, Dash, Hookshot }
    public AbilityType abilityToUnlock;

    public void PurchaseUpgrade()
    {
        // Attempt to spend money from the player's money manager
        bool purchased = PlayerMoneyManager.Instance.SpendMoney(cost);
        if (!purchased)
        {
            Debug.Log("Not enough money to purchase this upgrade. Cost: " + cost);
            return;
        }

        // If purchase is successful, unlock the ability in PowerUpManager
        switch (abilityToUnlock)
        {
            case AbilityType.Invincibility:
                powerUpManager.invincibilityUnlocked = true;
                break;

            case AbilityType.Dash:
                powerUpManager.dashUnlocked = true;
                break;

            case AbilityType.Hookshot:
                powerUpManager.teleportUnlocked = true;
                break;
        }

        // If there's a GameObject to activate (e.g., a new UI panel or something)
        if (abilityToActivate != null)
        {
            abilityToActivate.SetActive(true);
        }

        Debug.Log($"{abilityToUnlock} upgrade purchased and unlocked.");
    }
}
