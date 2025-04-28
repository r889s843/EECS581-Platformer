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
    public enum AbilityType { Invincibility, Dash, Hookshot, AIStop, DoubleJump }
    public AbilityType abilityToUnlock;
    private int abilityIndex;

    void Start()
    {
        // Map ability type to PlayerData array index
        switch (abilityToUnlock)
        {
            case AbilityType.Invincibility:
                abilityIndex = 3; // Invincibility
                break;
            case AbilityType.Dash:
                abilityIndex = 0; // Dash
                break;
            case AbilityType.Hookshot:
                abilityIndex = 4; // Teleport (Hookshot)
                break;
            case AbilityType.AIStop:
                abilityIndex = 2; // AIStop
                break;
            case AbilityType.DoubleJump:
                abilityIndex = 1; // DoubleJump
                break;
        }
    }

    public void PurchaseUpgrade()
    {
        // Check if the ability can be purchased (set by NPC)
        if (!PlayerManager.Instance.playerData.abilitiesCanBePurchased[abilityIndex])
        {
            Debug.Log($"Cannot purchase {abilityToUnlock}: Not unlocked by NPC.");
            return;
        }

        // Check if already purchased
        if (PlayerManager.Instance.playerData.abilitiesUnlocked[abilityIndex])
        {
            Debug.Log($"{abilityToUnlock} is already purchased.");
            return;
        }

        // Attempt to spend money
        bool purchased = PlayerMoneyManager.Instance.SpendMoney(cost);
        if (!purchased)
        {
            Debug.Log("Not enough money to purchase this upgrade. Cost: " + cost);
            return;
        }

        // Update PlayerData to mark the ability as purchased
        PlayerManager.Instance.playerData.abilitiesUnlocked[abilityIndex] = true;
        PlayerManager.Instance.SavePlayerData();

        // If there's a GameObject to activate (e.g., a new UI panel or something)
        if (abilityToActivate != null)
        {
            abilityToActivate.SetActive(true);
        }

        Debug.Log($"{abilityToUnlock} upgrade purchased and unlocked.");
    }
}
