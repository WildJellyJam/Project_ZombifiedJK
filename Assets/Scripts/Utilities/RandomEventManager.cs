using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // ====== Bad Ending 用 ======
    [System.Serializable]
    public class BadEndingChoice
    {
        public string buttonLabel;
        [TextArea(2, 5)] public string responseText;

        [Header("行為")]
        public bool hideButtonAfterClick = false;
        public bool goToNextStage = false;
        public bool finishBadEnding = false;
    }

    [System.Serializable]
    public class BadEndingStage
    {
        [Header("畫面")]
        public Sprite backgroundSprite;
        public Sprite portraitSprite;

        [Header("開場文字")]
        [TextArea(2, 6)] public string introText;

        [Header("按鈕")]
        public List<BadEndingChoice> choices = new List<BadEndingChoice>();
    }

    [Header("Bad Ending 設定")]
    public bool enterBadEndingMode = false;
    public GameObject badEndingPanelPrefab;
    public List<BadEndingStage> badEndingStages = new List<BadEndingStage>();
    public bool loadSceneAfterBadEnding = false;
    public string nextSceneAfterBadEnding = "";

    // Bad Ending 當前狀態
    private int currentBadEndingStageIndex = 0;

    // Bad Ending 畫面引用
    private GameObject badEndingPanelInstance;
    private Image badEndingBackgroundImage;
    private Image badEndingPortraitImage;
    private TextMeshProUGUI badEndingDialogueText;
    private Button[] badEndingButtons;
    private TextMeshProUGUI[] badEndingButtonTexts;

    // 目前畫面上那個事件 panel（如果有）
    [HideInInspector] public GameObject cutScenePanel;
    [HideInInspector] public TextMeshProUGUI cutScenePanelTextUI;

    // ====== （可選）每個區域的事件表，依 anxiety 篩選 ======
    [System.Serializable]
    public class AreaEventOption
    {
        public string eventName;
        [TextArea] public string description;
        public GameObject panelPrefab;

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

    // 目前是否有事件正在進行中
    private bool isEventActive = false;

    // 記錄現在是哪一個事件
    private string currentEventName = "";

    private void Start()
    {
        if (enterBadEndingMode)
        {
            StartBadEndingMode();
        }
    }

    // =========================================================
    //  Bad Ending 模式
    // =========================================================

    public void StartBadEndingMode()
    {
        if (badEndingPanelPrefab == null)
        {
            Debug.LogError("[BadEnding] badEndingPanelPrefab 沒有指定。");
            return;
        }

        if (badEndingStages == null || badEndingStages.Count == 0)
        {
            Debug.LogError("[BadEnding] badEndingStages 沒有設定內容。");
            return;
        }

        // 如果原本事件 panel 還在，先關掉
        if (cutScenePanel != null)
        {
            Destroy(cutScenePanel);
            cutScenePanel = null;
            cutScenePanelTextUI = null;
        }

        if (badEndingPanelInstance != null)
        {
            Destroy(badEndingPanelInstance);
        }

        badEndingPanelInstance = Instantiate(badEndingPanelPrefab);
        badEndingPanelInstance.name = badEndingPanelPrefab.name + "_BadEndingInstance";

        CacheBadEndingUIReferences();

        currentBadEndingStageIndex = 0;
        SetupBadEndingStage(currentBadEndingStageIndex);

        isEventActive = true;
        currentEventName = "BadEnding";
    }

    private void CacheBadEndingUIReferences()
    {
        Transform bg = badEndingPanelInstance.transform.Find("BackgroundImage");
        Transform portrait = badEndingPanelInstance.transform.Find("CharacterPortrait");
        Transform dialogue = badEndingPanelInstance.transform.Find("displayText");

        if (bg != null) badEndingBackgroundImage = bg.GetComponent<Image>();
        if (portrait != null) badEndingPortraitImage = portrait.GetComponent<Image>();
        if (dialogue != null) badEndingDialogueText = dialogue.GetComponent<TextMeshProUGUI>();

        if (badEndingDialogueText == null)
        {
            Debug.LogError("[BadEnding] 找不到 displayText，請確認 prefab 裡有一個叫 displayText 的 TMP 物件。");
        }

        badEndingButtons = new Button[4];
        badEndingButtonTexts = new TextMeshProUGUI[4];

        for (int i = 0; i < 4; i++)
        {
            string btnName = $"Button_{i + 1}";
            Transform btn = badEndingPanelInstance.transform.Find(btnName);
            if (btn != null)
            {
                badEndingButtons[i] = btn.GetComponent<Button>();
                badEndingButtonTexts[i] = btn.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
    }

    private void SetupBadEndingStage(int stageIndex)
    {
        if (badEndingStages == null || stageIndex < 0 || stageIndex >= badEndingStages.Count)
        {
            Debug.LogWarning("[BadEnding] stageIndex 超出範圍。");
            return;
        }

        BadEndingStage stage = badEndingStages[stageIndex];

        if (badEndingBackgroundImage != null)
        {
            badEndingBackgroundImage.sprite = stage.backgroundSprite;
        }

        if (badEndingPortraitImage != null)
        {
            badEndingPortraitImage.sprite = stage.portraitSprite;
            badEndingPortraitImage.enabled = (stage.portraitSprite != null);
        }

        if (badEndingDialogueText != null)
        {
            badEndingDialogueText.text = stage.introText;
        }

        for (int i = 0; i < badEndingButtons.Length; i++)
        {
            if (badEndingButtons[i] == null) continue;

            if (i < stage.choices.Count)
            {
                int capturedIndex = i;
                badEndingButtons[i].gameObject.SetActive(true);
                badEndingButtons[i].onClick.RemoveAllListeners();

                if (badEndingButtonTexts[i] != null)
                    badEndingButtonTexts[i].text = stage.choices[i].buttonLabel;

                badEndingButtons[i].onClick.AddListener(() => OnBadEndingChoiceClicked(capturedIndex));
            }
            else
            {
                badEndingButtons[i].gameObject.SetActive(false);
                badEndingButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    private void OnBadEndingChoiceClicked(int choiceIndex)
    {
        if (currentBadEndingStageIndex < 0 || currentBadEndingStageIndex >= badEndingStages.Count) return;

        BadEndingStage stage = badEndingStages[currentBadEndingStageIndex];
        if (choiceIndex < 0 || choiceIndex >= stage.choices.Count) return;

        BadEndingChoice choice = stage.choices[choiceIndex];

        if (badEndingDialogueText != null)
        {
            badEndingDialogueText.text = choice.responseText;
        }

        if (choice.hideButtonAfterClick && badEndingButtons != null && choiceIndex < badEndingButtons.Length && badEndingButtons[choiceIndex] != null)
        {
            badEndingButtons[choiceIndex].gameObject.SetActive(false);
        }

        if (choice.goToNextStage)
        {
            currentBadEndingStageIndex++;

            if (currentBadEndingStageIndex < badEndingStages.Count)
            {
                SetupBadEndingStage(currentBadEndingStageIndex);
                return;
            }
            else
            {
                FinishBadEnding();
                return;
            }
        }

        if (choice.finishBadEnding)
        {
            FinishBadEnding();
        }
    }

    public void FinishBadEnding()
    {
        Debug.Log("[BadEnding] 結束。");

        if (badEndingPanelInstance != null)
        {
            Destroy(badEndingPanelInstance);
            badEndingPanelInstance = null;
        }

        enterBadEndingMode = false;
        isEventActive = false;
        currentEventName = "";

        if (loadSceneAfterBadEnding && !string.IsNullOrEmpty(nextSceneAfterBadEnding))
        {
            SceneManager.LoadScene(nextSceneAfterBadEnding);
        }
    }

    // =========================================================
    //  基本隨機事件（全域用）
    // =========================================================
    public void TriggerRandomEvent()
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";

        if (randomCode <= 30)
        {
            eventName = "LowDifficultyEvent";

            ShowEventPanel(
                "發生了一件小小的不順心的事情。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 60)
        {
            eventName = "MediumDifficultyEvent";

            ShowEventPanel(
                "你遇到了一件讓你有點緊張的狀況。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 90)
        {
            eventName = "HighDifficultyEvent";

            ShowEventPanel(
                "壓力突然大幅襲來，你感到非常不安。",
                GetSafePanelPrefab(null)
            );
        }
        else
        {
            eventName = "SpecialEvent";

            ShowEventPanel(
                "一件特別的事情發生了……",
                GetSafePanelPrefab(null)
            );
        }

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");
    }

    // =========================================================
    //  各區域事件
    // =========================================================

    public void TriggerRandomEvent_home()
    {
        if (homeEvents != null && homeEvents.Count > 0)
        {
            TriggerRandomEventFromList(homeEvents, "HOME");
            return;
        }

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

        string eventName = "LowDifficultyEvent";

        newGameManager.Instance.playerStats.UpdateAnxiety(5f);
        newGameManager.Instance.timeSystem.AddEventTime(1f);

        triggeredEvents.Add(eventName);
        Debug.Log($"[OUTSIDE Fallback] 觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_sleep()
    {
        string eventName = "SleepEvent";

        newGameManager.Instance.timeSystem.AddEventTime(5f);

        triggeredEvents.Add(eventName);
        Debug.Log($"觸發事件：{eventName}");

        newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }

    public void TriggerRandomEvent_milk()
    {
        string eventName = "BuyMilk";
        TriggerEvent(eventName, true);
        Debug.Log($"觸發事件：{eventName}");
    }

    // =========================================================
    //  區域事件核心
    // =========================================================

    private void TriggerRandomEventFromList(List<AreaEventOption> options, string areaNameForDebug)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning($"[{areaNameForDebug}] 沒有設定任何事件選項！");
            return;
        }

        float currentAnxiety = newGameManager.Instance.playerStats.anxiety;

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

        int index = Random.Range(0, candidates.Count);
        AreaEventOption chosen = candidates[index];

        Debug.Log($"[{areaNameForDebug}] 觸發事件：{chosen.eventName}（anxiety={currentAnxiety}）");

        if (chosen.anxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(chosen.anxietyDelta);
        }

        if (chosen.timeCostHours != 0f)
        {
            newGameManager.Instance.timeSystem.AddEventTime(chosen.timeCostHours);
            newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        }

        GameObject prefabToUse = chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab;
        if (prefabToUse != null)
        {
            ShowEventPanel(chosen.description, prefabToUse);
        }

        triggeredEvents.Add(chosen.eventName);
        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    // =========================================================
    //  固定命名事件
    // =========================================================

    public void TriggerEvent(string eventName, bool isMandatory)
    {
        if (!isMandatory) return;

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
    //  顯示事件 Panel
    // =========================================================

    public void ShowEventPanel(string description, GameObject panelPrefab)
    {
        if (panelPrefab == null)
        {
            Debug.LogError("[ShowEventPanel] panelPrefab 為 null，請在 Inspector 指定 defaultEventPanelPrefab 或對應事件的 prefab。");
            return;
        }

        cutScenePanel = Instantiate(panelPrefab);
        cutScenePanel.name = panelPrefab.name + "_Instance";

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

    public void OnEventConfirmButtonPressed()
    {
        switch (currentEventName)
        {
            case "ReceiveMessage":
                break;

            case "BuyMilk":
                break;

            case "ReceiveCatVideo":
                break;
        }

        if (cutScenePanel != null)
        {
            Destroy(cutScenePanel);
            cutScenePanel = null;
            cutScenePanelTextUI = null;
        }

        if (EventLogUI.Instance != null)
        {
            EventLogUI.Instance.StartFadeOutAll();
        }

        isEventActive = false;
        currentEventName = "";

        if (newGameManager.Instance != null)
        {
            newGameManager.Instance.OnTimeManuallyUpdated();
        }

        Debug.Log("事件已結束。");
    }
}