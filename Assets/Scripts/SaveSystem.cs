using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]


public class SaveSystem : MonoBehaviour
{
    private const int MaxSlots = 3;
    private List<SaveData> saveSlots = new List<SaveData>();

    public void SaveGame(int slotIndex, GameManager gameManager)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;

        SaveData data = new SaveData
        {
            gameTime = gameManager.timeSystem.gameTime,
            playerStats = gameManager.playerStats,
            inventory = gameManager.inventory,
            triggeredEvents = gameManager.randomEventManager.GetTriggeredEvents(),
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (saveSlots.Count <= slotIndex) saveSlots.Add(data);
        else saveSlots[slotIndex] = data;

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText($"SaveSlot{slotIndex}.json", json);
    }

    public class SaveData
{
    public GameTime gameTime; // 時間
    public PlayerStats playerStats; // 主角數值
    public Inventory inventory; // 物品
    public List<string> triggeredEvents; // 已觸發事件
    public string timestamp; // 存檔時間戳
}

    public SaveData LoadGame(int slotIndex)
    {
        string path = $"SaveSlot{slotIndex}.json";
        if (!System.IO.File.Exists(path)) return null;

        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public List<SaveData> GetSaveSlots()
    {
        return saveSlots;
    }
}