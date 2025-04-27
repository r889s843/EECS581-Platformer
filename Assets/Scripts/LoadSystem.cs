using System.IO;
using UnityEngine;

public static class LoadSystem {
    public static SaveData LoadGameData(int slotIndex) {
        try {
            string filePath = Application.persistentDataPath + "/save" + slotIndex + ".json";
            if (File.Exists(filePath)) {
                string fileContent = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(fileContent);
                return saveData;
            }
            return null;
        } catch {
            return null;
        }
    }
}