using UnityEngine;
using UnityEngine.UI;

public class AbilityIconManager : MonoBehaviour
{
    // References to the UI icons for each ability
    [Header("Ability Icons")]
    [SerializeField] private Image dashIcon;        // Icon for Dash (index 0)
    [SerializeField] private Image doubleJumpIcon;  // Icon for DoubleJump (index 1)
    [SerializeField] private Image teleportIcon;    // Icon for Teleport (index 2)
    [SerializeField] private Image invincibilityIcon; // Icon for Invincibility (index 3)
    [SerializeField] private Image aiStopIcon;      // Icon for AIStop (index 4)

    private void Start()
    {
        // Update the UI icons based on the initial state of unlockedAbilities
        UpdateAbilityIcons();
    }

    private void Update()
    {
        // Check for changes to unlockedAbilities (e.g., after a purchase)
        // This could be optimized with an event system if performance becomes an issue
        UpdateAbilityIcons();
    }

    // Updates the visibility of ability icons based on unlockedAbilities
    private void UpdateAbilityIcons()
    {
        // Ensure PlayerManager and PlayerData are available
        if (PlayerManager.Instance == null || PlayerManager.Instance.playerData == null)
        {
            Debug.LogWarning("AbilityIconManager: PlayerManager or PlayerData is not available. Cannot update ability icons.");
            return;
        }

        // Ensure the array is the expected length
        if (PlayerManager.Instance.playerData.abilitiesUnlocked.Length < 5)
        {
            Debug.LogWarning($"AbilityIconManager: Expected abilitiesUnlocked array of length 5. Got length: {PlayerManager.Instance.playerData.abilitiesUnlocked.Length}");
            return;
        }

        // Update each icon's visibility
        UpdateIconVisibility(dashIcon, 0);        // Dash
        UpdateIconVisibility(doubleJumpIcon, 1);  // DoubleJump
        UpdateIconVisibility(teleportIcon, 2);    // Teleport
        UpdateIconVisibility(invincibilityIcon, 3); // Invincibility
        UpdateIconVisibility(aiStopIcon, 4);      // AIStop
    }

    // Helper method to update the visibility of a single icon
    private void UpdateIconVisibility(Image icon, int abilityIndex)
    {
        if (icon == null)
        {
            Debug.LogWarning($"AbilityIconManager: Icon for ability {abilityIndex} is not assigned.");
            return;
        }

        bool isUnlocked = PlayerManager.Instance.playerData.abilitiesUnlocked[abilityIndex];
        icon.gameObject.SetActive(isUnlocked);
    }
}