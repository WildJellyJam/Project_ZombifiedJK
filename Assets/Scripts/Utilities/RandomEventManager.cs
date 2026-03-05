using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomEventManager : MonoBehaviour
{
    // ====== 記錄已觸發的事件 ======
    private List<string> triggeredEvents = new List<string>();

    [Header("事件用的面板 Prefab")]
    [Tooltip("預設事件面板（如果沒有指定其他 prefab，就會用這個）")]
    public GameObject defaultEventPanelPrefab;

    [Tooltip("例如：收到貓咪影片用的事件面板（可留空，就用預設）")]
    public GameObject receiveCatVideoPanelPrefab;

    [Tooltip("例如：爸媽吵架用的事件面板（可留空，就用預設）")]
    public GameObject parentsArguePanelPrefab;

    // 目前畫面上那個事件 panel（如果有）
    [HideInInspector] public GameObject cutScenePanel;
    [HideInInspector] public TextMeshProUGUI cutScenePanelTextUI;

    // ====== （可選）每個區域的事件表，依 anxiety 篩選 ======
    [System.Serializable]
    public class AreaEventOption
    {
        public string eventName;                 // 事件識別用名稱
        [TextArea] public string description;    // 顯示在面板上的文字
        public GameObject panelPrefab;           // 這個事件用的面板（可為 null → 用 default）

        [Header("Anxiety 條件")]
        public float minAnxiety = 0f;
        public float maxAnxiety = 120f;

        [Header("事件效果")]
        public float anxietyDelta = 0f;
        public float timeCostHours = 0f;
    }

    [Header("各區域事件表（如果有填，會優先用這個）")]
    public List<AreaEventOption> homeEvents = new List<AreaEventOption>();
    public List<AreaEventOption> shopEvents = new List<AreaEventOption>();
    public List<AreaEventOption> outsideEvents = new List<AreaEventOption>();

    // =========================================================
    //  基本隨機事件（全域用）
    // =========================================================
    public void TriggerRandomEvent()
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";

        if (randomCode <= 30) // 低難度
        {

            eventName = "LowDifficultyEvent";

            ShowEventPanel(
                "發生了一件小小的不順心的事情。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 60) // 中難度
        {
            
            eventName = "MediumDifficultyEvent";

            ShowEventPanel(
                "你遇到了一件讓你有點緊張的狀況。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 90) // 高難度
        {
            
            eventName = "HighDifficultyEvent";

            ShowEventPanel(
                "壓力突然大幅襲來，你感到非常不安。",
                GetSafePanelPrefab(null)
            );
        }
        else // 特殊事件
        {
            eventName = "SpecialEvent";

            ShowEventPanel(
                "一件特別的事情發生了……",
                GetSafePanelPrefab(null)
            );
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        // 如果你要在這裡 + 時間，可以開這兩行：
        // newGameManager.Instance.timeSystem.AddEventTime(1f);
        // newGameManager.Instance.OnTimeManuallyUpdated();
    }

    // =========================================================
    //  各區域事件：會優先用 homeEvents/shopEvents/outsideEvents
    //  如果沒設定 List，就走原本簡單的 +5 焦慮 +1 小時
    // =========================================================

    public void TriggerRandomEvent_home()
    {
        if (homeEvents != null && homeEvents.Count > 0)
        {
            TriggerRandomEventFromList(homeEvents, "HOME");
            return;
        }

        // 沒設定 homeEvents，就用原本的 fallback 行為
        int randomCode = UnityEngine.Random.Range(0, 99);
        string eventName = "LowDifficultyEvent";

        newGameManager.Instance.playerStats.UpdateAnxiety(5f);
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        triggeredEvents.Add(eventName);
        Debug.Log($"[HOME Fallback] 觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_shop()
    {
        if (shopEvents != null && shopEvents.Count > 0)
        {
            TriggerRandomEventFromList(shopEvents, "SHOP");
            return;
        }

        int randomCode = UnityEngine.Random.Range(100, 199);
        string eventName = "LowDifficultyEvent";

        newGameManager.Instance.playerStats.UpdateAnxiety(5f);
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        triggeredEvents.Add(eventName);
        Debug.Log($"[SHOP Fallback] 觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_outside()
    {
        if (outsideEvents != null && outsideEvents.Count > 0)
        {
            TriggerRandomEventFromList(outsideEvents, "OUTSIDE");
            return;
        }

        int randomCode = UnityEngine.Random.Range(200, 299);
        string eventName = "LowDifficultyEvent";

        newGameManager.Instance.playerStats.UpdateAnxiety(5f);
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        triggeredEvents.Add(eventName);
        Debug.Log($"[OUTSIDE Fallback] 觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_sleep()
    {
        int randomCode = UnityEngine.Random.Range(100, 199); // 目前沒用到 randomCode，但保留
        string eventName = "SleepEvent";

        newGameManager.Instance.timeSystem.AddEventTime(5f);

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_milk()
    {
        string eventName = "BuyMilk";

        // 直接走固定事件邏輯
        TriggerEvent(eventName, true);

        Debug.Log($"觸發事件：{eventName}");
    }

    // =========================================================
    //  區域事件核心：依 Anxiety 篩選 + 隨機選一個
    // =========================================================

    private void TriggerRandomEventFromList(List<AreaEventOption> options, string areaNameForDebug)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning($"[{areaNameForDebug}] 沒有設定任何事件選項！");
            return;
        }

        float currentAnxiety = newGameManager.Instance.playerStats.anxiety;

        // 1. 先依照 anxiety 篩出候選
        List<AreaEventOption> candidates = new List<AreaEventOption>();
        foreach (var opt in options)
        {
            if (currentAnxiety >= opt.minAnxiety && currentAnxiety <= opt.maxAnxiety)
            {
                candidates.Add(opt);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[{areaNameForDebug}] anxiety={currentAnxiety} 沒有符合條件的事件，改用全部事件隨機。");
            candidates = options;
        }

        // 2. 隨機挑一個
        int index = Random.Range(0, candidates.Count);
        AreaEventOption chosen = candidates[index];

        Debug.Log($"[{areaNameForDebug}] 觸發事件：{chosen.eventName}（anxiety={currentAnxiety}）");

        // 3. 套用事件效果
        if (chosen.anxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(chosen.anxietyDelta);
        }

        if (chosen.timeCostHours != 0f)
        {
            newGameManager.Instance.timeSystem.AddEventTime(chosen.timeCostHours);
            newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        }

        // 4. 顯示面板（prefab 沒指定就用 default）
        GameObject prefabToUse = chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab;
        if (prefabToUse != null)
        {
            ShowEventPanel(chosen.description, prefabToUse);
        }

        triggeredEvents.Add(chosen.eventName);
        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    // =========================================================
    //  固定命名事件（時間系統或其他地方呼叫）
    // =========================================================

    public void TriggerEvent(string eventName, bool isMandatory)
    {
        if (!isMandatory) return;

        // 避免重複觸發（BuyMilk 允許重複）
        if (triggeredEvents.Contains(eventName) && eventName != "BuyMilk") return;

        triggeredEvents.Add(eventName);

        currentEventName = eventName;
        isEventActive = true;

        switch (eventName)
        {
            case "ReceiveMessage":
                Debug.Log("收到訊息！");
                newGameManager.Instance.playerStats.UpdateAnxiety(2f);
                FindObjectOfType<gameUIManager>().ShowMessage("xxxxxxxxxxxx");
                break;

            case "BuyMilk":
                Debug.Log("去超市買牛奶！");
                Inventory.PickupItem("Milk");
                newGameManager.Instance.playerStats.UpdateAnxiety(5f);
                FindObjectOfType<gameUIManager>().ShowMilk();
                newGameManager.Instance.timeSystem.AddEventTime(1f);
                newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);

                ShowEventPanel(
                    "你出門去便利商店買牛奶。",
                    GetSafePanelPrefab(null)
                );
                break;

            case "ReceiveCatVideo":
                Debug.Log("收到貓咪影片！");
                newGameManager.Instance.playerStats.UpdateAnxiety(-5f);

                ShowEventPanel(
                    "你收到了超可愛的貓咪影片，心情稍微放鬆了一點。",
                    GetSafePanelPrefab(receiveCatVideoPanelPrefab)
                );
                break;

            case "ParentsArgue":
                Debug.Log("爸媽吵架了...");
                newGameManager.Instance.playerStats.UpdateAnxiety(10f);

                ShowEventPanel(
                    "你聽到爸媽在客廳吵架，胸口一陣發緊。",
                    GetSafePanelPrefab(parentsArguePanelPrefab)
                );
                break;

            case "MeetSido":
                Debug.Log("探險中遇到希多！");
                newGameManager.Instance.playerStats.UpdateAnxiety(8f);

                ShowEventPanel(
                    "你在路上遇見了希多，氣氛有點微妙。",
                    GetSafePanelPrefab(null)
                );
                break;

            case "LowPopularityEvent":
                Debug.Log("校園熱門度太低，觸發事件！");
                ShowEventPanel(
                    "你感覺自己在學校變得有點透明，心裡有點不是滋味。",
                    GetSafePanelPrefab(null)
                );
                break;
        }

        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    // =========================================================
    //  顯示事件 Panel：直接生成 prefab，不掛在任何 Canvas 底下
    //  （假設 prefab 自己裡面有 Canvas）
    // =========================================================

    public void ShowEventPanel(string description, GameObject panelPrefab)
    {
        if (panelPrefab == null)
        {
            Debug.LogError("[ShowEventPanel] panelPrefab 為 null，請在 Inspector 指定 defaultEventPanelPrefab 或對應事件的 prefab。");
            return;
        }

        // 直接生成，不指定 parent：prefab 自己的 Canvas 會接管顯示
        cutScenePanel = Instantiate(panelPrefab);
        cutScenePanel.name = panelPrefab.name + "_Instance";

        // 找子物件 "displayText"
        Transform textTransform = cutScenePanel.transform.Find("displayText");
        if (textTransform == null)
        {
            Debug.LogError("[ShowEventPanel] 在 " + cutScenePanel.name + " 裡找不到 'displayText' 子物件，請確認 prefab 結構。");
            return;
        }

        cutScenePanelTextUI = textTransform.GetComponent<TextMeshProUGUI>();
        if (cutScenePanelTextUI == null)
        {
            Debug.LogError("[ShowEventPanel] 'displayText' 上沒有 TextMeshProUGUI，請確認掛的是 TMP 文字。");
            return;
        }

        cutScenePanelTextUI.text = description;
        cutScenePanel.SetActive(true);
    }

    // 如果 specificPrefab == null，就用 defaultEventPanelPrefab
    private GameObject GetSafePanelPrefab(GameObject specificPrefab)
    {
        if (specificPrefab != null) return specificPrefab;
        return defaultEventPanelPrefab;
    }

    // =========================================================
    //  其他工具函式
    // =========================================================

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

    public bool HasTriggered(string eventName)
    {
        return triggeredEvents.Contains(eventName);
    }

    public List<string> GetTriggeredEvents()
    {
        return triggeredEvents;
    }
    // 目前是否有事件正在進行中
    private bool isEventActive = false;

    // 記錄現在是哪一個事件（方便按完按鈕時知道要做什麼）
    private string currentEventName = "";


    public void OnEventConfirmButtonPressed()
    {
        // 1. 根據當前事件，做最後的處理（如果需要）
        switch (currentEventName)
        {
            case "ReceiveMessage":
                // 這裡可以做「按確認後才發生的事情」
                // 例如真的把訊息加入某個 log，或是之後解鎖什麼
                break;

            case "BuyMilk":
                // 如果你想改成：
                // → 只有按下確認才算真的買到牛奶
                // 可以把 Inventory.PickupItem("Milk") 等邏輯搬到這裡
                break;

            case "ReceiveCatVideo":
                // 例如：按確認後才真正降低焦慮 / 解鎖某個 flag...
                break;

                // 其他事件...
        }

        // 2. 關掉事件面板
        if (cutScenePanel != null)
        {
            Destroy(cutScenePanel);
            cutScenePanel = null;
            cutScenePanelTextUI = null;
        }

        // 3. 如果你有 EventLogUI，可以在這裡開始淡出事件文字
        if (EventLogUI.Instance != null)
        {
            EventLogUI.Instance.StartFadeOutAll();
        }

        // 4. 清掉狀態
        isEventActive = false;
        currentEventName = "";

        // 5. 通知時間系統「事件流程結束了」
        if (newGameManager.Instance != null)
        {
            newGameManager.Instance.OnTimeManuallyUpdated();
        }

        Debug.Log("事件已結束。");
    }

}

