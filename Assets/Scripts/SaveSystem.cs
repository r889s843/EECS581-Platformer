using System;
using System.IO;
using UnityEngine;

public static class SaveSystem {
    // public const string FILENAME_SAVEDATA = "/savedata.json";

    public static void SaveGameState(PlayerData playerData, int slotIndex) {
        string filePathSaveData = Application.persistentDataPath + "/save" + slotIndex + ".json";
        Debug.Log(filePathSaveData);
        SaveData saveData = new SaveData(playerData);
        string txt = JsonUtility.ToJson(saveData);
        File.WriteAllText(filePathSaveData, txt);
    }

    public static void DeleteSaveData(int slotIndex) {
        string filePath = Application.persistentDataPath + "/save" + slotIndex + ".json";
        if (File.Exists(filePath)) {
            File.Delete(filePath);
        }
    }
}

[Serializable]
public class SaveData{
    public PlayerData playerData;

    public SaveData(PlayerData playerData){
        this.playerData = playerData;
    }
}


[Serializable]
public class PlayerData {
    public string username;
    public float money;              // Total money earned
    public bool[] levelProgress;     // Progress for 6 story levels
    public bool[] abilitiesCanBePurchased; // Tracks if ability can be purchased (set by NPC)
    public bool[] abilitiesUnlocked; // 4 abilities unlocked status
    public float bestFreerunDistance; // Best distance in freerun mode
    public int procGenCompletionCount; // Total procedural level completions
    public float bestStoryTime;      // Best time to complete story mode
}