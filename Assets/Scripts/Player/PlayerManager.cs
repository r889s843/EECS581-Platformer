using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public PlayerData playerData;
    public int currentSlotIndex = 0; // 0 means no slot selected

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadPlayerData(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        SaveData saveData = LoadSystem.LoadGameData(slotIndex);
        if (saveData != null)
        {
            playerData = saveData.playerData;
        }
        else
        {
            InitializeNewPlayerData();
            playerData.username = ""; // Will be set later
        }
    }

    public void SavePlayerData()
    {
        if (currentSlotIndex > 0)
        {
            SaveSystem.SaveGameState(playerData, currentSlotIndex);
        }
        else
        {
            Debug.LogWarning("Cannot save: No slot selected.");
        }
    }

    private void InitializeNewPlayerData()
    {
        playerData = new PlayerData
        {
            username = "",
            money = 0f,
            levelProgress = new bool[6], // 6 levels, all false
            abilitiesUnlocked = new bool[4], // 4 abilities, all false
            bestFreerunDistance = 0f,
            procGenCompletionCount = 0,
            bestStoryTime = 0f
        };
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }
}