using UnityEngine;

public class PlayerManager : MonoBehaviour {
    public static PlayerManager Instance { get; private set; }
    public PlayerData playerData;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPlayerData();
        } else {
            Destroy(gameObject);
        }
    }

    public void LoadPlayerData() {
        SaveData saveData = LoadSystem.LoadGameData();
        if (saveData != null) {
            playerData = saveData.playerData;
        } else {
            InitializeNewPlayerData();
        }
    }

    public void SavePlayerData() {
        SaveSystem.SaveGameState(playerData);
    }

    public void ResetPlayerData() {
        InitializeNewPlayerData();
        SaveSystem.DeleteSaveData();
    }

    private void InitializeNewPlayerData() {
        playerData = new PlayerData {
            money = 0f,
            levelProgress = new bool[6], // 6 levels, all false
            abilitiesUnlocked = new bool[4], // 4 abilities, all false
            bestFreerunDistance = 0f,
            procGenCompletionCount = 0,
            bestStoryTime = 0f // Lower time is better
        };
    }

    private void OnApplicationQuit() {
        SavePlayerData();
    }
}