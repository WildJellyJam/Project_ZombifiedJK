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
    // 額外的後續文字（事件結束後可再跳一次）
    private string pendingAfterEventDescription = "";
    private bool isShowingAfterEventDescription = false;
    private GameObject lastUsedEventPanelPrefab;

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
   
    [System.Serializable]
    public class ChainEventChoice
    {
        public string buttonLabel;

        [TextArea(2, 5)]
        public string responseText;   // 按下選項後立刻顯示的文字（可留空）

        [TextArea(2, 5)]
        public string afterEventDescription;   // 事件結束後額外跳出的文字（依選項而不同）

        public string nextNodeId;     // 下次再抽到同一個 rootEventName 時，要走哪個節點

        [Header("選項效果")]
        public float anxietyDelta = 0f;
        public float timeCostHours = 0f;
    }
    [Header("連鎖事件系統")]
public List<ChainEventDefinition> chainEventDefinitions = new List<ChainEventDefinition>();

// 記住每個 rootEventName 下一次要走哪個 nodeId
private readonly Dictionary<string, string> chainEventNextNodeMap = new Dictionary<string, string>();

// 目前正在顯示的連鎖事件上下文
private ChainEventDefinition currentChainDefinition;
private ChainEventNode currentChainNode;
private string currentChainRootEventName = "";
private bool currentEventUsesChain = false;

// 連鎖事件的按鈕快取
private Button[] chainChoiceButtons = new Button[4];
private TextMeshProUGUI[] chainChoiceButtonTexts = new TextMeshProUGUI[4];
    [System.Serializable]
    public class ChainEventNode
    {
        public string nodeId;         // 例如：B、B-2-1、B-2-2

        [TextArea(2, 6)]
        public string description;

        [TextArea(2, 6)]
        public string afterEventDescription;   // 事件結束後額外再跳一段（可留空）

        public GameObject panelPrefab;         // 這個節點自己要用的 panel，可留空就用外部 chosen.panelPrefab

        [Header("選項")]
        public List<ChainEventChoice> choices = new List<ChainEventChoice>();
    }

    [System.Serializable]
    public class ChainEventDefinition
    {
        public string rootEventName;      // 例如：B
        public string startNodeId = "B";  // 第一次抽到 B 時要顯示哪個節點
        public List<ChainEventNode> nodes = new List<ChainEventNode>();
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

        ClearCurrentEventPanel();
        ClearPersistentEventDescriptionInStatUI();

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
    private void ApplyEventDescriptionLayout(TextMeshProUGUI tmp, float extraTopOffset = 18f, float extraPadding = 12f, float minHeight = 40f)
    {
        if (tmp == null) return;

        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.ForceMeshUpdate();

        RectTransform rt = tmp.rectTransform;

        float preferredHeight = tmp.preferredHeight + extraPadding;
        if (preferredHeight < minHeight)
            preferredHeight = minHeight;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        Vector2 pos = rt.anchoredPosition;
        pos.y += extraTopOffset;
        rt.anchoredPosition = pos;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
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

            FinishBadEnding();
            return;
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

        int randomCode = UnityEngine.Random.Range(0, 100);
        string eventName = "";

        if (randomCode <= 30)
        {
            eventName = "LowDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "發生了一件小小的不順心的事情。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 60)
        {
            eventName = "MediumDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "你遇到了一件讓你有點緊張的狀況。",
                GetSafePanelPrefab(null)
            );
        }
        else if (randomCode <= 90)
        {
            eventName = "HighDifficultyEvent";
            currentEventName = eventName;
            isEventActive = true;

            ShowEventPanel(
                "壓力突然大幅襲來，你感到非常不安。",
                GetSafePanelPrefab(null)
            );
        }
        else
        {
            eventName = "SpecialEvent";
            currentEventName = eventName;
            isEventActive = true;

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

        currentEventName = chosen.eventName;
        isEventActive = true;

        Debug.Log($"[{areaNameForDebug}] 觸發事件：{chosen.eventName}（anxiety={currentAnxiety}）");

        // 先嘗試走連鎖事件
        if (TryStartChainEvent(chosen))
        {
            // root 事件本身的效果仍然可以照常套用
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

        // 如果不是連鎖事件，才走你原本的普通事件流程
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

        GameObject prefabToUse = chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab;
        if (prefabToUse != null)
        {
            ShowEventPanel(chosen.description, prefabToUse, chosen.afterEventDescription);
        }

        // 再做數值變化
        if (chosen.anxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(chosen.anxietyDelta);
        }

        // 最後再加時間
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

    public void ShowEventPanel(string description, GameObject panelPrefab, string afterEventDescription = "")
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
        cutScenePanel.SetActive(true);

        // 只有第一段事件文字才把後續文字記起來
        if (!isShowingAfterEventDescription)
        {
            pendingAfterEventDescription = afterEventDescription;
        }

        TryShowPersistentEventDescriptionInStatUI(description);
    }
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

    private bool TryStartChainEvent(AreaEventOption chosen)
    {
        if (chosen == null) return false;

        ChainEventDefinition def = GetChainDefinition(chosen.eventName);
        if (def == null)
            return false;

        string currentNodeId = GetCurrentNodeIdForRoot(def);
        ChainEventNode node = GetChainNode(def, currentNodeId);

        if (node == null)
        {
            Debug.LogWarning($"[ChainEvent] 找不到 nodeId={currentNodeId}，改用 startNodeId。");

            node = GetChainNode(def, def.startNodeId);
            if (node == null)
            {
                Debug.LogError($"[ChainEvent] rootEventName={def.rootEventName} 連 startNodeId={def.startNodeId} 都找不到。");
                return false;
            }
        }

        currentChainDefinition = def;
        currentChainNode = node;
        currentChainRootEventName = def.rootEventName;
        currentEventUsesChain = true;

        GameObject panelToUse = node.panelPrefab != null
            ? node.panelPrefab
            : (chosen.panelPrefab != null ? chosen.panelPrefab : defaultEventPanelPrefab);

        ShowEventPanel(node.description, panelToUse, node.afterEventDescription);
        SetupChainChoiceButtons(node);

        Debug.Log($"[ChainEvent] root={def.rootEventName}, 顯示節點={node.nodeId}");

        return true;
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

        if (node.choices.Count > 0 && chainChoiceButtons[0] == null)
        {
            Debug.LogWarning("[ChainEvent] 這個 panel 沒找到 Button_1 ~ Button_4，所以無法顯示連鎖事件選項。");
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

    public void OnChainChoicePressed(int choiceIndex)
    {
        if (!currentEventUsesChain || currentChainNode == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= currentChainNode.choices.Count)
            return;

        ChainEventChoice choice = currentChainNode.choices[choiceIndex];

        // 記住下次抽到這個 rootEventName 時，要走哪個節點
        if (!string.IsNullOrWhiteSpace(choice.nextNodeId))
        {
            SetNextNodeIdForRoot(currentChainRootEventName, choice.nextNodeId);
            Debug.Log($"[ChainEvent] root={currentChainRootEventName}, 下次改走={choice.nextNodeId}");
        }

        if (choice.anxietyDelta != 0f)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(choice.anxietyDelta);
        }

        if (choice.timeCostHours != 0f)
        {
            newGameManager.Instance.timeSystem.AddEventTime(choice.timeCostHours);
            newGameManager.Instance.timeUI.UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
        }
        string choiceAfterText = choice.afterEventDescription;

// 如果選項自己沒寫，就退回節點本身的 afterEventDescription
if (string.IsNullOrWhiteSpace(choiceAfterText) && currentChainNode != null)
{
    choiceAfterText = currentChainNode.afterEventDescription;
}

pendingAfterEventDescription = choiceAfterText;
isShowingAfterEventDescription = false;
        HideAllChainChoiceButtons();

        // 有 responseText 就先顯示當下回應
        if (!string.IsNullOrWhiteSpace(choice.responseText))
        {
            if (cutScenePanelTextUI != null)
            {
                cutScenePanelTextUI.text = choice.responseText;
            }

            TryShowPersistentEventDescriptionInStatUI(choice.responseText);
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
        // 如果還有後續文字，而且現在還沒顯示過，就先顯示後續文字
        if (!isShowingAfterEventDescription && !string.IsNullOrWhiteSpace(pendingAfterEventDescription))
        {
            string nextText = pendingAfterEventDescription;
            pendingAfterEventDescription = "";
            isShowingAfterEventDescription = true;

            ClearCurrentEventPanel();
            ClearPersistentEventDescriptionInStatUI();

            GameObject panelToUse = lastUsedEventPanelPrefab != null ? lastUsedEventPanelPrefab : GetSafePanelPrefab(null);
            ShowEventPanel(nextText, panelToUse, "");

            Debug.Log("顯示事件結束後的額外文字。");
            return;
        }

        switch (currentEventName)
        {
            case "ReceiveMessage":
                break;

            case "BuyMilk":
                break;

            case "ReceiveCatVideo":
                break;
        }

        ClearCurrentEventPanel();
        ClearPersistentEventDescriptionInStatUI();
        HideAllChainChoiceButtons();

        currentChainDefinition = null;
        currentChainNode = null;
        currentChainRootEventName = "";
        currentEventUsesChain = false;

        isEventActive = false;
        currentEventName = "";
        pendingAfterEventDescription = "";
        isShowingAfterEventDescription = false;
        lastUsedEventPanelPrefab = null;

        if (newGameManager.Instance != null)
        {
            newGameManager.Instance.OnTimeManuallyUpdated();
        }

        Debug.Log("事件已結束。");

    }
}