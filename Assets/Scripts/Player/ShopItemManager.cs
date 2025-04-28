using UnityEngine;

public class ShopItemManager : MonoBehaviour
{
    // References to the shop item GameObjects
    [Header("Shop Items")]
    [SerializeField] private GameObject item1; // Item1 (Dash)
    [SerializeField] private GameObject item2; // Item2 (DoubleJump)
    [SerializeField] private GameObject item3; // Item3 (Teleport)
    [SerializeField] private GameObject item4; // Item4 (Invincibility)
    [SerializeField] private GameObject item5; // Item5 (AIStop)

    private void Start()
    {
        // Update the shop items based on the unlockedAbilities array
        UpdateShopItems();
    }

    // Updates the visibility of shop items based on unlockedAbilities
    private void UpdateShopItems()
    {
        // Ensure PlayerManager and PlayerData are available
        if (PlayerManager.Instance == null || PlayerManager.Instance.playerData == null)
        {
            Debug.LogWarning("ShopItemManager: PlayerManager or PlayerData is not available. Cannot update shop items.");
            return;
        }

        // Ensure the array is the expected length
        if (PlayerManager.Instance.playerData.abilitiesUnlocked.Length < 5)
        {
            Debug.LogWarning($"ShopItemManager: Expected abilitiesUnlocked array of length 5. Got length: {PlayerManager.Instance.playerData.abilitiesUnlocked.Length}");
            return;
        }

        // Update each item's visibility
        UpdateItemVisibility(item1, 0); // Item1 (Dash)
        UpdateItemVisibility(item2, 1); // Item2 (DoubleJump)
        UpdateItemVisibility(item3, 2); // Item3 (Teleport)
        UpdateItemVisibility(item4, 3); // Item4 (Invincibility)
        UpdateItemVisibility(item5, 4); // Item5 (AIStop)
    }

    // Helper method to update the visibility of a single shop item
    private void UpdateItemVisibility(GameObject item, int abilityIndex)
    {
        if (item == null)
        {
            Debug.LogWarning($"ShopItemManager: Item for ability {abilityIndex} is not assigned.");
            return;
        }

        bool isUnlocked = PlayerManager.Instance.playerData.abilitiesUnlocked[abilityIndex];
        item.SetActive(isUnlocked);
    }
}