using System.Collections.Generic;
using UnityEngine;

public class RandomEventManager
{
    private List<string> triggeredEvents = new List<string>();
    // private newGameManager.Instance newGameManager.Instance;
    // private newGameManager.Instance.TimeSystem newGameManager.Instance.timeSystem;


    // 觸發隨機事件（基於隨機碼）
    public void TriggerRandomEvent(PlayerStats playerStats)
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";
        if (randomCode <= 30) // 低難度
        {
            playerStats.UpdateAnxiety(5f);
            eventName = "LowDifficultyEvent";
        }
        else if (randomCode <= 60) // 中難度
        {
            playerStats.UpdateAnxiety(10f);
            eventName = "MediumDifficultyEvent";
        }
        else if (randomCode <= 90) // 高難度
        {
            playerStats.UpdateAnxiety(20f);
            eventName = "HighDifficultyEvent";
        }
        else // 特殊事件
        {
            eventName = "SpecialEvent";
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        // 事件完成後推進時間（1小時）
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        // 觸發下一個隨機事件
        // newnewGameManager.Instance.Instance.TriggerNextEvent();
    }

    // 觸發固定事件（由newGameManager.Instance.TimeSystem調用）
    public void TriggerEvent(string eventName, bool isMandatory)
    {
        if (!isMandatory) return;

        // 避免重複觸發（對於強制事件，需檢查是否已觸發）
        if (triggeredEvents.Contains(eventName) && eventName != "BuyMilk") return; // BuyMilk可能重複觸發

        triggeredEvents.Add(eventName);
        switch (eventName)
        {
            case "ReceiveMessage":
                Debug.Log("收到訊息！");
                newGameManager.Instance.playerStats.UpdateAnxiety(2f); // 小幅增加焦慮
                break;
            case "BuyMilk":
                Debug.Log("去超市買牛奶！");
                newGameManager.Instance.inventory.PickupItem("Milk");
                newGameManager.Instance.sceneManager.SwitchScene(TimePeriod.AtSupermarket); // 切換到超市場景
                newGameManager.Instance.playerStats.UpdateAnxiety(5f); // 增加焦慮
                break;
            case "ReceiveCatVideo":
                Debug.Log("收到貓咪影片！");
                newGameManager.Instance.playerStats.UpdateAnxiety(-5f); // 減少焦慮
                break;
            case "ParentsArgue":
                Debug.Log("爸媽吵架了...");
                newGameManager.Instance.playerStats.UpdateAnxiety(10f); // 增加焦慮
                newGameManager.Instance.sceneManager.SwitchScene(TimePeriod.AtHomeParentsArgue); // 切換到吵架場景
                break;
            case "MeetSido":
                Debug.Log("探險中遇到希多！");
                newGameManager.Instance.playerStats.UpdateAnxiety(8f); // 增加焦慮
                newGameManager.Instance.sceneManager.SwitchScene(TimePeriod.AtAdventure); // 切換到探險場景
                break;
            case "LowPopularityEvent":
                Debug.Log("校園熱門度太低，觸發事件！");
                newGameManager.Instance.playerStats.UpdateAnxiety(15f); // 增加焦慮
                newGameManager.Instance.sceneManager.SwitchScene(TimePeriod.AtSchoolAfterClass); // 切換到放學場景
                break;
        }
        // 固定事件完成後也推進時間（1小時）
        newGameManager.Instance.newGameManager.Instance.timeSystem.AddEventTime(1f);

        // 觸發下一個隨機事件
        newGameManager.Instance.TriggerNextEvent();
    }

    // 檢查事件是否已觸發
    public bool HasTriggered(string eventName)
    {
        return triggeredEvents.Contains(eventName);
    }

    public List<string> GetTriggeredEvents()
    {
        return triggeredEvents;
    }
}