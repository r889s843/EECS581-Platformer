using UnityEngine;

public class NPCManager : MonoBehaviour
{
    // References to the NPC GameObjects in the shop level
    [Header("NPCs")]
    [SerializeField] private GameObject turing;  // Level 1: Dash
    [SerializeField] private GameObject asimov;  // Level 2: DoubleJump
    [SerializeField] private GameObject cloud;   // Level 3: AIStop
    [SerializeField] private GameObject darwin;  // Level 4: Invincibility
    [SerializeField] private GameObject zip;     // Level 5: Teleport

    private void Start()
    {
        // Update NPC visibility based on the level progress
        UpdateNPCs();
    }

    // Updates the visibility of NPCs based on the level progress array
    private void UpdateNPCs()
    {
        // Ensure PlayerManager and PlayerData are available
        if (PlayerManager.Instance == null || PlayerManager.Instance.playerData == null)
        {
            Debug.LogWarning("NPCManager: PlayerManager or PlayerData is not available. Cannot update NPCs.");
            return;
        }

        // Ensure the levelProgress array is the expected length
        if (PlayerManager.Instance.playerData.levelProgress == null || PlayerManager.Instance.playerData.levelProgress.Length < 5)
        {
            Debug.LogWarning($"NPCManager: Expected levelProgress array of length 5. Got length: {(PlayerManager.Instance.playerData.levelProgress?.Length ?? 0)}");
            return;
        }

        // Update each NPC's visibility based on the corresponding level progress
        UpdateNPCVisibility(turing, 0);  // Turing (Level 1: Dash)
        UpdateNPCVisibility(asimov, 1);  // Asimov (Level 2: DoubleJump)
        UpdateNPCVisibility(cloud, 2);   // Cloud (Level 3: AIStop)
        UpdateNPCVisibility(darwin, 3);  // Darwin (Level 4: Invincibility)
        UpdateNPCVisibility(zip, 4);     // Zip (Level 5: Teleport)
    }

    // Helper method to update the visibility of a single NPC
    private void UpdateNPCVisibility(GameObject npc, int levelIndex)
    {
        if (npc == null)
        {
            Debug.LogWarning($"NPCManager: NPC for level index {levelIndex} is not assigned.");
            return;
        }

        // Enable the NPC if the corresponding level has been completed
        bool levelCompleted = PlayerManager.Instance.playerData.levelProgress[levelIndex];
        npc.SetActive(levelCompleted);
    }
}