using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    // References to the Purchase buttons and Lock icons for each ability (Item1 to Item5)
    [Header("Ability 1 (Dash)")]
    [SerializeField] private Button purchaseButton1; // Purchase button for Item1
    [SerializeField] private GameObject lockIcon1;   // Item1_Locked GameObject

    [Header("Ability 2 (DoubleJump)")]
    [SerializeField] private Button purchaseButton2; // Purchase button for Item2
    [SerializeField] private GameObject lockIcon2;   // Item2_Locked GameObject

    [Header("Ability 3 (Teleport)")]
    [SerializeField] private Button purchaseButton3; // Purchase button for Item3
    [SerializeField] private GameObject lockIcon3;   // Item3_Locked GameObject

    [Header("Ability 4 (Invincibility)")]
    [SerializeField] private Button purchaseButton4; // Purchase button for Item4
    [SerializeField] private GameObject lockIcon4;   // Item4_Locked GameObject

    [Header("Ability 5 (AIStop)")]
    [SerializeField] private Button purchaseButton5; // Purchase button for Item5
    [SerializeField] private GameObject lockIcon5;   // Item5_Locked GameObject

    // Cache the previous state of abilitiesCanBePurchased to detect changes
    private bool[] previousAbilitiesCanBePurchased;

    private void Start()
    {
        // Initialize the previous state array
        if (PlayerManager.Instance != null && PlayerManager.Instance.playerData != null)
        {
            previousAbilitiesCanBePurchased = new bool[PlayerManager.Instance.playerData.abilitiesCanBePurchased.Length];
            System.Array.Copy(PlayerManager.Instance.playerData.abilitiesCanBePurchased, previousAbilitiesCanBePurchased, previousAbilitiesCanBePurchased.Length);
        }
        else
        {
            previousAbilitiesCanBePurchased = new bool[5]; // Default to 5 abilities if PlayerManager isn't available
            Debug.LogWarning("ShopUIManager: PlayerManager or PlayerData is not available. Using default array size of 5.");
        }

        // Update the UI based on the initial state of unlockedAbilities
        UpdateShopUI();
    }

    private void Update()
    {
        // Check if PlayerManager and PlayerData are available
        if (PlayerManager.Instance == null || PlayerManager.Instance.playerData == null)
        {
            Debug.LogWarning("ShopUIManager: PlayerManager or PlayerData is not available. Cannot update shop UI.");
            return;
        }

        // Check if abilitiesCanBePurchased has changed (e.g., after a purchase)
        bool hasChanged = false;
        for (int i = 0; i < PlayerManager.Instance.playerData.abilitiesCanBePurchased.Length; i++)
        {
            if (i < previousAbilitiesCanBePurchased.Length && PlayerManager.Instance.playerData.abilitiesCanBePurchased[i] != previousAbilitiesCanBePurchased[i])
            {
                hasChanged = true;
                break;
            }
        }

        // If a change is detected, update the UI and cache the new state
        if (hasChanged)
        {
            UpdateShopUI();
            System.Array.Copy(PlayerManager.Instance.playerData.abilitiesCanBePurchased, previousAbilitiesCanBePurchased, previousAbilitiesCanBePurchased.Length);
        }
    }

    // Updates the shop UI based on the current state of unlockedAbilities and abilitiesCanBePurchased
    private void UpdateShopUI()
    {
        // Ensure PlayerManager and PlayerData are available
        if (PlayerManager.Instance == null || PlayerManager.Instance.playerData == null)
        {
            Debug.LogWarning("ShopUIManager: PlayerManager or PlayerData is not available. Cannot update shop UI.");
            return;
        }

        // Ensure the arrays are the expected length
        if (PlayerManager.Instance.playerData.abilitiesUnlocked.Length < 5 || PlayerManager.Instance.playerData.abilitiesCanBePurchased.Length < 5)
        {
            Debug.LogWarning($"ShopUIManager: Expected arrays of length 5. Got unlockedAbilities: {PlayerManager.Instance.playerData.abilitiesUnlocked.Length}, abilitiesCanBePurchased: {PlayerManager.Instance.playerData.abilitiesCanBePurchased.Length}");
            return;
        }

        // Update each ability's UI elements
        // Ability 1 (Dash)
        UpdateAbilityUI(purchaseButton1, lockIcon1, 0);

        // Ability 2 (DoubleJump)
        UpdateAbilityUI(purchaseButton2, lockIcon2, 1);

        // Ability 3 (Teleport)
        UpdateAbilityUI(purchaseButton3, lockIcon3, 2);

        // Ability 4 (Invincibility)
        UpdateAbilityUI(purchaseButton4, lockIcon4, 3);

        // Ability 5 (AIStop)
        UpdateAbilityUI(purchaseButton5, lockIcon5, 4);
    }

    // Helper method to update the UI for a single ability
    private void UpdateAbilityUI(Button purchaseButton, GameObject lockIcon, int abilityIndex)
    {
        // Skip if UI elements are not assigned
        if (purchaseButton == null || lockIcon == null)
        {
            Debug.LogWarning($"ShopUIManager: Purchase button or lock icon for ability {abilityIndex} is not assigned.");
            return;
        }

        bool isUnlocked = PlayerManager.Instance.playerData.abilitiesUnlocked[abilityIndex];
        bool canBePurchased = PlayerManager.Instance.playerData.abilitiesCanBePurchased[abilityIndex];

        // Update the Purchase button: enable if the ability can be purchased and isn't unlocked
        purchaseButton.interactable = canBePurchased && !isUnlocked;

        // Update the Lock icon: show if the ability is not unlocked
        lockIcon.SetActive(!isUnlocked);
    }
}