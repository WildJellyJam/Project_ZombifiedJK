using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class Inventory
{
    public static List<string> heldItems = new List<string>(); // 持有物品
    public static List<string> collectedItems = new List<string>(); // 收集過的物品

    public static void PickupItem(string itemName)
    {
        heldItems.Add(itemName);
        if (!collectedItems.Contains(itemName)) collectedItems.Add(itemName);
    }

    public static void InteractWithItem(string itemName)
    {
        // 觸發事件，例如顯示對話
    }
}