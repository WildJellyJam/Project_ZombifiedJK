using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomEventManager : MonoBehaviour
{
    // 已觸發事件的紀錄
    private List<string> triggeredEvents = new List<string>();

    [Header("Cutscene UI")]
    [Tooltip("專門用來顯示事件面板的 Canvas（請在 Inspector 指定）")]
    public Canvas cutSceneCanvas;              // 專用 Canvas

    [Tooltip("目前場景中已生成的事件面板物件（程式會自動填）")]
    public GameObject cutScenePanel;

    [Tooltip("事件面板裡顯示文字的 TextMeshProUGUI（程式會自動抓 displayText）")]
    public TextMeshProUGUI cutScenePanelTextUI;


    // 觸發隨機事件（基於隨機碼）
    public void TriggerRandomEvent()
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";

        if (randomCode <= 30) // 低難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(5f);
            eventName = "LowDifficultyEvent";
        }
        else if (randomCode <= 60) // 中難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(10f);
            eventName = "MediumDifficultyEvent";
        }
        else if (randomCode <= 90) // 高難度
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(20f);
            eventName = "HighDifficultyEvent";
        }
        else // 特殊事件
        {
            eventName = "SpecialEvent";
        }

        // 這裡示範用固定的 prefab 路徑
        ShowEventPanel(eventName, "panelPrefabs/cutScenePanelPrefab");

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        // 事件完成後推進時間（1小時）
        // newGameManager.Instance.timeSystem.AddEventTime(1f);

        // 觸發下一個隨機事件
        // newGameManager.Instance.TriggerNextEvent();
    }

    public void TriggerRandomEvent_home()
    {
        int randomCode = UnityEngine.Random.Range(0, 99);
        string eventName = "";

        switch (randomCode)
        {
            case 0:
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;

            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_shop()
    {
        int randomCode = UnityEngine.Random.Range(100, 199);
        string eventName = "";

        switch (randomCode)
        {
            case 0:
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;

            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_outside()
    {
        int randomCode = UnityEngine.Random.Range(200, 299);
        string eventName = "";

        switch (randomCode)
        {
            case 0:
            case 1:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;

            default:
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                eventName = "LowDifficultyEvent";
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_sleep()
    {
        int randomCode = UnityEngine.Random.Range(100, 199); // 目前沒用到 randomCode，但保留
        string eventName = "SleepEvent";

        // 事件完成後推進時間（5小時）
        newGameManager.Instance.timeSystem.AddEventTime(5f);

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_milk()
    {
        string eventName = "BuyMilk";

        // 直接走固定事件邏輯
        TriggerEvent(eventName, true);

        Debug.Log($"觸發事件：{eventName}");
        Debug.Log(newGameManager.Instance == null);
        Debug.Log(newGameManager.Instance.timeUI == null);
        Debug.Log(newGameManager.Instance.timeSystem == null);
    }

    public void atHomeEvent()
    {
        newGameManager.Instance.playerStats.nextAction = NextAction.goBackHome;
        newGameManager.Instance.OnTimeManuallyUpdated();
        newGameManager.Instance.playerStats.nextAction = NextAction.none;
    }

    public void toSchoolEvent()
    {
        newGameManager.Instance.playerStats.nextAction = NextAction.goOut;
        newGameManager.Instance.playerStats.updateDailyState();

        newGameManager.Instance.timeSystem.AddEventTime(1f);
        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);

        newGameManager.Instance.playerStats.nextAction = NextAction.none;
    }

    // 觸發固定事件（由 newGameManager.Instance.timeSystem 調用）
    public void TriggerEvent(string eventName, bool isMandatory)
    {
        if (!isMandatory) return;

        // 避免重複觸發（BuyMilk 允許重複）
        if (triggeredEvents.Contains(eventName) && eventName != "BuyMilk") return;

        triggeredEvents.Add(eventName);

        switch (eventName)
        {
            case "ReceiveMessage":
                Debug.Log("收到訊息！");
                newGameManager.Instance.playerStats.UpdateAnxiety(2f);
                FindObjectOfType<gameUIManager>().ShowMessage("xxxxxxxxxxxx");
                // newGameManager.Instance.timeSystem.AddEventTime(1f);
                break;

            case "BuyMilk":
                Debug.Log("去超市買牛奶！");
                Inventory.PickupItem("Milk");
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                FindObjectOfType<gameUIManager>().ShowMilk();
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
                break;

            case "ReceiveCatVideo":
                Debug.Log("收到貓咪影片！");
                newGameManager.Instance.playerStats.UpdateAnxiety(-5f);
                ShowEventPanel("你收到了超可愛的貓咪影片，心情稍微放鬆了一點。", "panelPrefabs/cutScenePanelPrefab");
                break;

            case "ParentsArgue":
                Debug.Log("爸媽吵架了...");
                newGameManager.Instance.playerStats.UpdateAnxiety(10f);
                break;

            case "MeetSido":
                Debug.Log("探險中遇到希多！");
                newGameManager.Instance.playerStats.UpdateAnxiety(8f);
                break;

            case "LowPopularityEvent":
                Debug.Log("校園熱門度太低，觸發事件！");
                break;
        }

        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    /// <summary>
    /// 顯示事件用的 Panel，並把它生成在「指定的 cutSceneCanvas」底下
    /// </summary>
    public void ShowEventPanel(string description, string path)
    {
        if (cutSceneCanvas == null)
        {
            Debug.LogError("CutSceneCanvas 沒有指定！請在 RandomEventManager 的 Inspector 把 Canvas 拖進來。");
            return;
        }

        Debug.Log("find panel from Resources: " + path);

        GameObject prefab = Resources.Load<GameObject>(path); // 路徑不含 Resources 與副檔名
        if (prefab == null)
        {
            Debug.LogError($"找不到 Prefab：{path}，請確認 Resources 下的路徑與檔名。");
            return;
        }

        // 把 panel 生在指定的 CutSceneCanvas 之下
        cutScenePanel = Instantiate(prefab, cutSceneCanvas.transform, false);

        // 找到文字物件（假設子物件叫 "displayText"）
        Transform textTransform = cutScenePanel.transform.Find("displayText");
        if (textTransform == null)
        {
            Debug.LogError("在 cutScenePanel 裡找不到 'displayText' 物件，請確認 prefab 結構。");
            return;
        }

        cutScenePanelTextUI = textTransform.GetComponent<TextMeshProUGUI>();
        if (cutScenePanelTextUI == null)
        {
            Debug.LogError("'displayText' 上沒有 TextMeshProUGUI 組件。");
            return;
        }

        Debug.Log("display dynamic panel");
        cutScenePanelTextUI.text = description;

        cutScenePanel.SetActive(true);
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
