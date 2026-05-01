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

    // =========================================================
    //  Bad Ending 用
    // =========================================================

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

    private int currentBadEndingStageIndex = 0;

    private GameObject badEndingPanelInstance;
    private Image badEndingBackgroundImage;
    private Image badEndingPortraitImage;
    private TextMeshProUGUI badEndingDialogueText;
    private Button[] badEndingButtons;
    private TextMeshProUGUI[] badEndingButtonTexts;

    // =========================================================
    //  一般事件 UI
    // =========================================================

    [HideInInspector] public GameObject cutScenePanel;
    [HideInInspector] public TextMeshProUGUI cutScenePanelTextUI;

    // 事件結束後額外跳出的文字
    private string pendingAfterEventDescription = "";
    private bool isShowingAfterEventDescription = false;
    private GameObject lastUsedEventPanelPrefab;

    // =========================================================
    //  各區域事件
    // =========================================================

    [System.Serializable]
    public class AreaEventOption
    {
        public string eventName;

        [TextArea]
        public string description;

        [TextArea]
        public string afterEventDescription;

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

    private bool isEventActive = false;
    private string currentEventName = "";

    // =========================================================
    //  連鎖事件系統
    // =========================================================

    [System.Serializable]
    public class ChainChoiceOutcome
    {
        public string outcomeName;

        [Range(0f, 100f)]
        public float weight = 100f;

        [TextArea(2, 5)]
        public string responseText;

        [TextArea(2, 5)]
        public string afterEventDescription;

        public string immediateNextNodeId;   // 這次按下去立刻切到哪個節點
        public string nextNodeId;            // 下次再抽到 root event 時要走哪個節點

        [Header("結果效果")]
        public float anxietyDelta = 0f;
        public float timeCostHours = 0f;

        [Header("結果外觀覆蓋（可留空）")]
        public GameObject overridePanelPrefab;
        public Sprite overrideBackgroundSprite;
        public Sprite overridePortraitSprite;
    }

    [System.Serializable]
    public class ChainEventChoice
    {
        public string buttonLabel;

        [Header("如果有填 randomOutcomes，按這個選項時會先抽其中一個結果")]
        public List<ChainChoiceOutcome> randomOutcomes = new List<ChainChoiceOutcome>();

        [Header("保底單一結果（當 randomOutcomes 沒填時才使用）")]
        [TextArea(2, 5)]
        public string responseText;

        [TextArea(2, 5)]
        public string afterEventDescription;

        public string immediateNextNodeId;
        public string nextNodeId;

        [Header("選項效果")]
        public float anxietyDelta = 0f;
        public float timeCostHours = 0f;

        [Header("選項外觀覆蓋（可留空）")]
        public GameObject overridePanelPrefab;
        public Sprite overrideBackgroundSprite;
        public Sprite overridePortraitSprite;
    }

    [System.Serializable]
    public class ChainEventNode
    {
        public string nodeId;

        [TextArea(2, 6)]
        public string description;

        [TextArea(2, 6)]
        public string afterEventDescription;

        public GameObject panelPrefab;

        [Header("節點圖片（可留空，不換）")]
        public Sprite backgroundSprite;
        public Sprite portraitSprite;

        [Header("節點選項")]
        public List<ChainEventChoice> choices = new List<ChainEventChoice>();
    }

    [System.Serializable]
    public class ChainEventDefinition
    {
        public string rootEventName;      // 要對應上方 AreaEventOption.eventName
        public string startNodeId = "";   // 第一次抽到時從哪個節點開始
        public List<ChainEventNode> nodes = new List<ChainEventNode>();
    }

    [Header("連鎖事件系統")]
    public List<ChainEventDefinition> chainEventDefinitions = new List<ChainEventDefinition>();

    // 記住每個 rootEventName 下一次該走哪個 nodeId
    private readonly Dictionary<string, string> chainEventNextNodeMap = new Dictionary<string, string>();

    // 目前正在跑的連鎖事件上下文
    private ChainEventDefinition currentChainDefinition;
    private ChainEventNode currentChainNode;
    private string currentChainRootEventName = "";
    private bool currentEventUsesChain = false;
    private GameObject currentChainFallbackPanelPrefab;

    private Button[] chainChoiceButtons = new Button[4];
    private TextMeshProUGUI[] chainChoiceButtonTexts = new TextMeshProUGUI[4];

    // =========================================================
    //  Unity
    // =========================================================

    private void Start()
    {
        if (enterBadEndingMode)
        {
            StartBadEndingMode();
        }
    }

    // =========================================================
    //  Bad Ending
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

        ClearCurrentEventPanel();
        ClearPersistentEventDescriptionInStatUI();
        ResetCurrentChainContext();

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
        Transform bg = FindDeepChildByName(badEndingPanelInstance.transform, "BackgroundImage");
        Transform portrait = FindDeepChildByName(badEndingPanelInstance.transform, "CharacterPortrait");
        Transform dialogue = FindDeepChildByName(badEndingPanelInstance.transform, "displayText");

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
            Transform btn = FindDeepChildByName(badEndingPanelInstance.transform, btnName);
            if (btn != null)
            {
                badEndingButtons[i] = btn.GetComponent<Button>();
                badEndingButtonTexts[i] = btn.GetComponentInChildren<TextMeshProUGUI>(true);
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

        if (badEndingBackgroundImage != null && stage.backgroundSprite != null)
        {
            badEndingBackgroundImage.sprite = stage.backgroundSprite;
        }

        if (badEndingPortraitImage != null)
        {
            if (stage.portraitSprite != null)
            {
                badEndingPortraitImage.sprite = stage.portraitSprite;
                badEndingPortraitImage.enabled = true;
            }
            else
            {
                badEndingPortraitImage.enabled = false;
            }
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

        if (choice.hideButtonAfterClick &&
            badEndingButtons != null &&
            choiceIndex < badEndingButtons.Length &&
            badEndingButtons[choiceIndex] != null)
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

        ClearPersistentEventDescriptionInStatUI();
        ResetCurrentChainContext();

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
        if (isEventActive)
        {
            Debug.Log("[TriggerRandomEvent] 目前已有事件進行中，略過這次全域隨機事件。");
            return;
        }

        ResetCurrentChainContext();
        pendingAfterEventDescription = "";
        isShowingAfterEventDescription = false;

        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";

        if (randomCode <= 30)
        {
            eventName = "LowDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "發生了一件小小的不順心的事情。",
                GetSafePanelPrefab(null),
                ""
            );
        }
        else if (randomCode <= 60)
        {
            eventName = "MediumDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "你遇到了一件讓你有點緊張的狀況。",
                GetSafePanelPrefab(null),
                ""
            );
        }
        else if (randomCode <= 90)
        {
            eventName = "HighDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "壓力突然大幅襲來，你感到非常不安。",
                GetSafePanelPrefab(null),
                ""
            );
        }
        else
        {
            eventName = "SpecialEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "一件特別的事情發生了……",
                GetSafePanelPrefab(null),
                ""
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

        currentEventName = chosen.eventName;
        isEventActive = true;

        pendingAfterEventDescription = "";
        isShowingAfterEventDescription = false;

        Debug.Log($"[{areaNameForDebug}] 觸發事件：{chosen.eventName}（anxiety={currentAnxiety}）");

        // 先嘗試走連鎖事件
        if (TryStartChainEvent(chosen))
        {
            if (chosen.anxietyDelta != 0f)
            {
                newGameManager.Instance.playerStats.UpdateAnxiety(chosen.anxietyDelta);
            }

            if (chosen.timeCostHours != 0f)
            {
                newGameManager.Instance.timeSystem.AddEventTime(chosen.timeCostHours);
                newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
            }

            triggeredEvents.Add(chosen.eventName);
            newGameManager.Instance.OnTimeManuallyUpdated();
            return;
        }

        // 普通事件流程
        ResetCurrentChainContext();

        GameObject normalPanelPrefab = chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab;
        if (normalPanelPrefab != null)
        {
            ShowEventPanel(chosen.description, normalPanelPrefab, chosen.afterEventDescription);
        }

        if (chosen.anxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(chosen.anxietyDelta);
        }

        if (chosen.timeCostHours != 0f)
        {
            newGameManager.Instance.timeSystem.AddEventTime(chosen.timeCostHours);
            newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
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

        pendingAfterEventDescription = "";
        isShowingAfterEventDescription = false;
        ResetCurrentChainContext();

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

                ShowEventPanel(
                    "你出門去便利商店買牛奶。",
                    GetSafePanelPrefab(null),
                    ""
                );

                newGameManager.Instance.timeSystem.AddEventTime(1f);
                newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
                break;

            case "ReceiveCatVideo":
                Debug.Log("收到貓咪影片！");
                newGameManager.Instance.playerStats.UpdateAnxiety(-5f);

                ShowEventPanel(
                    "你收到了超可愛的貓咪影片，心情稍微放鬆了一點。",
                    GetSafePanelPrefab(receiveCatVideoPanelPrefab),
                    ""
                );
                break;

            case "ParentsArgue":
                Debug.Log("爸媽吵架了...");
                newGameManager.Instance.playerStats.UpdateAnxiety(10f);

                ShowEventPanel(
                    "你聽到爸媽在客廳吵架，胸口一陣發緊。",
                    GetSafePanelPrefab(parentsArguePanelPrefab),
                    ""
                );
                break;

            case "MeetSido":
                Debug.Log("探險中遇到希多！");
                newGameManager.Instance.playerStats.UpdateAnxiety(8f);

                ShowEventPanel(
                    "你在路上遇見了希多，氣氛有點微妙。",
                    GetSafePanelPrefab(null),
                    ""
                );
                break;

            case "LowPopularityEvent":
                Debug.Log("校園熱門度太低，觸發事件！");
                ShowEventPanel(
                    "你感覺自己在學校變得有點透明，心裡有點不是滋味。",
                    GetSafePanelPrefab(null),
                    ""
                );
                break;
        }

        newGameManager.Instance.OnTimeManuallyUpdated();
    }

    // =========================================================
    //  連鎖事件：節點 / 選項 / 機率
    // =========================================================

    private ChainEventDefinition GetChainDefinition(string rootEventName)
    {
        if (chainEventDefinitions == null) return null;

        foreach (var def in chainEventDefinitions)
        {
            if (def != null && def.rootEventName == rootEventName)
                return def;
        }

        return null;
    }

    private ChainEventNode GetChainNode(ChainEventDefinition def, string nodeId)
    {
        if (def == null || def.nodes == null) return null;

        foreach (var node in def.nodes)
        {
            if (node != null && node.nodeId == nodeId)
                return node;
        }

        return null;
    }

    private string GetCurrentNodeIdForRoot(ChainEventDefinition def)
    {
        if (def == null) return "";

        if (chainEventNextNodeMap.TryGetValue(def.rootEventName, out string savedNodeId))
        {
            if (!string.IsNullOrWhiteSpace(savedNodeId))
                return savedNodeId;
        }

        if (!string.IsNullOrWhiteSpace(def.startNodeId))
            return def.startNodeId;

        return def.rootEventName;
    }

    private void SetNextNodeIdForRoot(string rootEventName, string nextNodeId)
    {
        if (string.IsNullOrWhiteSpace(rootEventName)) return;
        if (string.IsNullOrWhiteSpace(nextNodeId)) return;

        chainEventNextNodeMap[rootEventName] = nextNodeId;
    }

    private void ResetCurrentChainContext()
    {
        currentChainDefinition = null;
        currentChainNode = null;
        currentChainRootEventName = "";
        currentEventUsesChain = false;
        currentChainFallbackPanelPrefab = null;

        for (int i = 0; i < chainChoiceButtons.Length; i++)
        {
            chainChoiceButtons[i] = null;
            chainChoiceButtonTexts[i] = null;
        }
    }

    private bool TryStartChainEvent(AreaEventOption chosen)
    {
        if (chosen == null) return false;

        ChainEventDefinition def = GetChainDefinition(chosen.eventName);
        if (def == null)
            return false;

        ResetCurrentChainContext();

        currentChainDefinition = def;
        currentChainRootEventName = def.rootEventName;
        currentEventUsesChain = true;
        currentChainFallbackPanelPrefab = chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab;

        string currentNodeId = GetCurrentNodeIdForRoot(def);
        ChainEventNode node = GetChainNode(def, currentNodeId);

        if (node == null)
        {
            Debug.LogWarning($"[ChainEvent] 找不到 nodeId={currentNodeId}，改用 startNodeId。");
            node = GetChainNode(def, def.startNodeId);
        }

        if (node == null)
        {
            Debug.LogError($"[ChainEvent] rootEventName={def.rootEventName} 找不到可用節點。");
            ResetCurrentChainContext();
            return false;
        }

        ShowChainNode(node, currentChainFallbackPanelPrefab, true);
        Debug.Log($"[ChainEvent] root={def.rootEventName}, 顯示節點={node.nodeId}");
        return true;
    }

    private void ShowChainNode(ChainEventNode node, GameObject fallbackPanelPrefab, bool rememberAfterText)
    {
        if (node == null) return;

        currentChainNode = node;

        GameObject panelToUse = node.panelPrefab != null
            ? node.panelPrefab
            : (fallbackPanelPrefab != null ? fallbackPanelPrefab : defaultEventPanelPrefab);

        ShowEventPanel(
            node.description,
            panelToUse,
            node.afterEventDescription,
            node.backgroundSprite,
            node.portraitSprite,
            rememberAfterText
        );

        SetupChainChoiceButtons(node);
    }

    private void SetupChainChoiceButtons(ChainEventNode node)
    {
        if (cutScenePanel == null || node == null)
            return;

        for (int i = 0; i < 4; i++)
        {
            chainChoiceButtons[i] = null;
            chainChoiceButtonTexts[i] = null;

            Transform btnTf = FindDeepChildByName(cutScenePanel.transform, $"Button_{i + 1}");
            if (btnTf != null)
            {
                chainChoiceButtons[i] = btnTf.GetComponent<Button>();
                chainChoiceButtonTexts[i] = btnTf.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (chainChoiceButtons[i] == null) continue;

            chainChoiceButtons[i].onClick.RemoveAllListeners();

            if (i < node.choices.Count)
            {
                int capturedIndex = i;
                chainChoiceButtons[i].gameObject.SetActive(true);

                if (chainChoiceButtonTexts[i] != null)
                    chainChoiceButtonTexts[i].text = node.choices[i].buttonLabel;

                chainChoiceButtons[i].onClick.AddListener(() => OnChainChoicePressed(capturedIndex));
            }
            else
            {
                chainChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideAllChainChoiceButtons()
    {
        for (int i = 0; i < chainChoiceButtons.Length; i++)
        {
            if (chainChoiceButtons[i] != null)
            {
                chainChoiceButtons[i].onClick.RemoveAllListeners();
                chainChoiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private bool AnyVisibleChainChoiceButtons()
    {
        for (int i = 0; i < chainChoiceButtons.Length; i++)
        {
            if (chainChoiceButtons[i] != null && chainChoiceButtons[i].gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private ChainChoiceOutcome PickWeightedOutcome(List<ChainChoiceOutcome> outcomes)
    {
        if (outcomes == null || outcomes.Count == 0)
            return null;

        float totalWeight = 0f;

        for (int i = 0; i < outcomes.Count; i++)
        {
            if (outcomes[i] != null && outcomes[i].weight > 0f)
                totalWeight += outcomes[i].weight;
        }

        if (totalWeight <= 0f)
        {
            int randomIndex = Random.Range(0, outcomes.Count);
            return outcomes[randomIndex];
        }

        float roll = Random.Range(0f, totalWeight);
        float current = 0f;

        for (int i = 0; i < outcomes.Count; i++)
        {
            ChainChoiceOutcome outcome = outcomes[i];
            if (outcome == null || outcome.weight <= 0f)
                continue;

            current += outcome.weight;
            if (roll <= current)
                return outcome;
        }

        return outcomes[outcomes.Count - 1];
    }

    public void OnChainChoicePressed(int choiceIndex)
    {
        if (!currentEventUsesChain || currentChainNode == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= currentChainNode.choices.Count)
            return;

        ChainEventChoice choice = currentChainNode.choices[choiceIndex];

        string finalResponseText = choice.responseText;
        string finalAfterEventDescription = choice.afterEventDescription;
        string finalImmediateNextNodeId = choice.immediateNextNodeId;
        string finalNextNodeId = choice.nextNodeId;
        float finalAnxietyDelta = choice.anxietyDelta;
        float finalTimeCostHours = choice.timeCostHours;
        GameObject finalOverridePanelPrefab = choice.overridePanelPrefab;
        Sprite finalOverrideBackground = choice.overrideBackgroundSprite;
        Sprite finalOverridePortrait = choice.overridePortraitSprite;

        // 有機率結果就先抽一個
        if (choice.randomOutcomes != null && choice.randomOutcomes.Count > 0)
        {
            ChainChoiceOutcome pickedOutcome = PickWeightedOutcome(choice.randomOutcomes);

            if (pickedOutcome != null)
            {
                finalResponseText = pickedOutcome.responseText;
                finalAfterEventDescription = pickedOutcome.afterEventDescription;
                finalImmediateNextNodeId = pickedOutcome.immediateNextNodeId;
                finalNextNodeId = pickedOutcome.nextNodeId;
                finalAnxietyDelta = pickedOutcome.anxietyDelta;
                finalTimeCostHours = pickedOutcome.timeCostHours;
                finalOverridePanelPrefab = pickedOutcome.overridePanelPrefab;
                finalOverrideBackground = pickedOutcome.overrideBackgroundSprite;
                finalOverridePortrait = pickedOutcome.overridePortraitSprite;

                Debug.Log($"[ChainEvent] 選項 '{choice.buttonLabel}' 抽到結果：{pickedOutcome.outcomeName}");
            }
        }

        // 記住下次再抽到 root event 時要走哪個節點
        if (!string.IsNullOrWhiteSpace(finalNextNodeId))
        {
            SetNextNodeIdForRoot(currentChainRootEventName, finalNextNodeId);
            Debug.Log($"[ChainEvent] root={currentChainRootEventName}, 下次改走={finalNextNodeId}");
        }

        if (finalAnxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(finalAnxietyDelta);
        }

        if (finalTimeCostHours != 0f)
        {
            newGameManager.Instance.timeSystem.AddEventTime(finalTimeCostHours);
            newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        }

        // 沒填的話，退回目前節點的 afterEventDescription
        if (string.IsNullOrWhiteSpace(finalAfterEventDescription) && currentChainNode != null)
        {
            finalAfterEventDescription = currentChainNode.afterEventDescription;
        }

        pendingAfterEventDescription = finalAfterEventDescription;
        isShowingAfterEventDescription = false;

        // 這次按下去立刻切到下一個節點（換圖 / 換文字 / 換選項）
        if (!string.IsNullOrWhiteSpace(finalImmediateNextNodeId))
        {
            ChainEventNode nextNode = GetChainNode(currentChainDefinition, finalImmediateNextNodeId);
            if (nextNode != null)
            {
                GameObject fallbackPanel = finalOverridePanelPrefab != null
                    ? finalOverridePanelPrefab
                    : currentChainFallbackPanelPrefab;

                ShowChainNode(nextNode, fallbackPanel, true);

                // 如果結果自己也有 afterEventDescription，蓋掉 nextNode 的
                if (!string.IsNullOrWhiteSpace(finalAfterEventDescription))
                {
                    pendingAfterEventDescription = finalAfterEventDescription;
                    isShowingAfterEventDescription = false;
                }

                Debug.Log($"[ChainEvent] 立刻切換到節點：{nextNode.nodeId}");
                return;
            }
            else
            {
                Debug.LogWarning($"[ChainEvent] 找不到 immediateNextNodeId = {finalImmediateNextNodeId}");
            }
        }

        HideAllChainChoiceButtons();

        // 沒有 immediateNextNodeId，就直接顯示這次結果自己的描述 / 圖
        bool hasVisualOverride = finalOverridePanelPrefab != null || finalOverrideBackground != null || finalOverridePortrait != null;

        if (!string.IsNullOrWhiteSpace(finalResponseText) || hasVisualOverride)
        {
            string textToShow = !string.IsNullOrWhiteSpace(finalResponseText)
                ? finalResponseText
                : (cutScenePanelTextUI != null ? cutScenePanelTextUI.text : "");

            GameObject responsePanel = finalOverridePanelPrefab != null
                ? finalOverridePanelPrefab
                : (lastUsedEventPanelPrefab != null ? lastUsedEventPanelPrefab : defaultEventPanelPrefab);

            ShowEventPanel(
                textToShow,
                responsePanel,
                "",
                finalOverrideBackground,
                finalOverridePortrait,
                false
            );

            return;
        }
    }

    // =========================================================
    //  顯示事件 Panel
    // =========================================================

    public void ShowEventPanel(
        string description,
        GameObject panelPrefab,
        string afterEventDescription = "",
        Sprite backgroundOverride = null,
        Sprite portraitOverride = null,
        bool rememberAfterText = true
    )
    {
        if (panelPrefab == null)
        {
            Debug.LogError("[ShowEventPanel] panelPrefab 為 null，請在 Inspector 指定 defaultEventPanelPrefab 或對應事件的 prefab。");
            return;
        }

        ClearCurrentEventPanel();

        cutScenePanel = Instantiate(panelPrefab);
        cutScenePanel.name = panelPrefab.name + "_Instance";
        lastUsedEventPanelPrefab = panelPrefab;

        Transform textTransform = FindDeepChildByName(cutScenePanel.transform, "displayText");
        if (textTransform == null)
        {
            Debug.LogError("[ShowEventPanel] 在 " + cutScenePanel.name + " 裡找不到 'displayText'（包含子階層），請確認 prefab 結構。");
            return;
        }

        cutScenePanelTextUI = textTransform.GetComponent<TextMeshProUGUI>();
        if (cutScenePanelTextUI == null)
        {
            Debug.LogError("[ShowEventPanel] 'displayText' 上沒有 TextMeshProUGUI，請確認掛的是 TMP 文字。");
            return;
        }

        cutScenePanelTextUI.text = description;
        ApplyEventVisuals(cutScenePanel.transform, backgroundOverride, portraitOverride);

        if (rememberAfterText)
        {
            pendingAfterEventDescription = afterEventDescription;
            isShowingAfterEventDescription = false;
        }

        cutScenePanel.SetActive(true);
        TryShowPersistentEventDescriptionInStatUI(description);
    }

    private void ApplyEventVisuals(Transform root, Sprite backgroundSprite, Sprite portraitSprite)
    {
        if (root == null) return;

        if (backgroundSprite != null)
        {
            Transform bgTf = FindDeepChildByName(root, "BackgroundImage");
            if (bgTf != null)
            {
                Image bgImg = bgTf.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.sprite = backgroundSprite;
                    bgImg.enabled = true;
                }
            }
        }

        if (portraitSprite != null)
        {
            Transform portraitTf = FindDeepChildByName(root, "CharacterPortrait");
            if (portraitTf != null)
            {
                Image portraitImg = portraitTf.GetComponent<Image>();
                if (portraitImg != null)
                {
                    portraitImg.sprite = portraitSprite;
                    portraitImg.enabled = true;
                }
            }
        }
    }

    private GameObject GetSafePanelPrefab(GameObject specificPrefab)
    {
        if (specificPrefab != null) return specificPrefab;
        return defaultEventPanelPrefab;
    }

    private void ClearCurrentEventPanel()
    {
        if (cutScenePanel != null)
        {
            Destroy(cutScenePanel);
            cutScenePanel = null;
            cutScenePanelTextUI = null;
        }

        for (int i = 0; i < chainChoiceButtons.Length; i++)
        {
            chainChoiceButtons[i] = null;
            chainChoiceButtonTexts[i] = null;
        }
    }

    private void TryShowPersistentEventDescriptionInStatUI(string description)
    {
        if (StatChangeUI.Instance == null) return;

        StatChangeUI.Instance.SendMessage(
            "ShowPersistentEventDescription",
            description,
            SendMessageOptions.DontRequireReceiver
        );
    }

    private void ClearPersistentEventDescriptionInStatUI()
    {
        if (StatChangeUI.Instance == null) return;

        StatChangeUI.Instance.SendMessage(
            "ClearPersistentEventDescription",
            SendMessageOptions.DontRequireReceiver
        );
    }

    private Transform FindDeepChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindDeepChildByName(child, childName);
            if (found != null)
                return found;
        }

        return null;
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
        // 連鎖事件若還在選選項，不允許直接按確認跳過
        if (currentEventUsesChain && AnyVisibleChainChoiceButtons())
        {
            Debug.Log("[ChainEvent] 這個節點還有選項，請先按選項。");
            return;
        }

        // 有後續文字就先顯示後續文字
        if (!isShowingAfterEventDescription && !string.IsNullOrWhiteSpace(pendingAfterEventDescription))
        {
            string nextText = pendingAfterEventDescription;
            pendingAfterEventDescription = "";
            isShowingAfterEventDescription = true;

            ClearCurrentEventPanel();
            ClearPersistentEventDescriptionInStatUI();

            GameObject panelToUse = lastUsedEventPanelPrefab != null ? lastUsedEventPanelPrefab : GetSafePanelPrefab(null);
            ShowEventPanel(nextText, panelToUse, "", null, null, false);

            HideAllChainChoiceButtons();

            Debug.Log("顯示事件結束後的額外文字。");
            return;
        }

        ClearCurrentEventPanel();
        ClearPersistentEventDescriptionInStatUI();
        HideAllChainChoiceButtons();

        pendingAfterEventDescription = "";
        isShowingAfterEventDescription = false;
        lastUsedEventPanelPrefab = null;

        ResetCurrentChainContext();

        isEventActive = false;
        currentEventName = "";

        if (newGameManager.Instance != null)
        {
            newGameManager.Instance.OnTimeManuallyUpdated();
        }

        Debug.Log("事件已結束。");
    }
}