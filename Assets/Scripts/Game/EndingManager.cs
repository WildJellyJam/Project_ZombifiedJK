using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    public enum EndingState
    {
        None,
        BadEndingMode,      // 壞結局「同場景互動模式」
        PlayingEndingUI     // 播放 ending prefab（文字+圖片）
    }

    [Header("Persistence")]
    public bool dontDestroyOnLoad = true;

    [Header("References (assign in Inspector)")]
    public EndingSequencePlayer endingPlayerPrefabOrInScene;

    [Header("Ending Sequences")]
    public EndingSequence badEnding;
    public EndingSequence normalEnding;
    public EndingSequence goodEnding;

    [Header("Rules")]
    public int badEndingAnxietyThreshold = 100;
    public int normalEndingDay = 5;

    [Header("Good route flag (temporary)")]
    public bool goodRouteUnlocked = false;

    [Header("Bad Ending Trigger Scope")]
    [Tooltip("勾起來：壞結局只會在指定場景觸發（例如 Scene2）")]
    public bool badEndingOnlyInSpecificScenes = true;
    public List<string> badEndingAllowedScenes = new List<string> { "Scene2" };

    [Header("Bad Ending Flow (No scene change)")]
    [Tooltip("勾起來：觸發壞結局時先進入 BadEndingMode（同場景互動），不直接播 ending UI")]
    public bool badEndingUseInSceneMode = true;

    [Tooltip("進入 BadEndingMode 時是否鎖住遊戲時間（一般不建議，打字機會用到時間）")]
    public bool pauseGameInBadEndingMode = false;

    [Header("State (Read Only)")]
    [SerializeField] private EndingState state = EndingState.None;
    public EndingState State => state;

    // ====== DEBUG (replace with your real GameManager values later) ======
    [Header("DEBUG (replace with GameManager values later)")]
    public int debugAnxiety = 0;
    public int debugCurrentDay = 1;

    [Header("DEBUG UI / Hotkeys")]
    public bool enableDebugTriggers = true;
    public bool debugOnlyInEditor = true;

    // =========================
    // Scene2 背景依焦慮變化（可選）
    // =========================
    [Header("Scene2 Anxiety Background (Optional)")]
    [Tooltip("Scene2 的背景 UI Image（或你想變色的 Image）")]
    public Image scene2BackgroundImage;
    [Header("Background Sprite Swap (Optional)")]
    [Tooltip("正常狀態的背景圖（低焦慮）")]
    public Sprite backgroundNormalSprite;

    [Tooltip("到達壞結局門檻（例如100）時要換成的背景圖")]
    public Sprite backgroundBadSprite;

    [Tooltip("是否在焦慮>=門檻時直接換圖")]
    public bool swapSpriteAtBadThreshold = true;

    [Tooltip("換圖門檻（通常=badEndingAnxietyThreshold）")]
    public int backgroundSwapThreshold = 100;

    private bool _bgSwapped = false;
    [Tooltip("用 Gradient 控制 0~1 的顏色變化：0=正常，1=最糟")]
    public Gradient backgroundTintByAnxiety;

    public float backgroundMinAnxiety = 0f;
    public float backgroundMaxAnxiety = 100f;

    [Tooltip("只在這些場景才更新背景（留空=所有場景都會更新）")]
    public List<string> backgroundAffectScenes = new List<string> { "Scene2" };

    // =========================
    // 按鈕限制 / 改事件（壞結局模式）
    // =========================
    public enum ButtonModeInBadEnding
    {
        Disable,        // 壞結局時禁用
        KeepNormal,     // 壞結局時仍然正常
        SwitchToBad     // 壞結局時換成 BadEnding 專用事件
    }

    [System.Serializable]
    public class ButtonRoute
    {
        public string id;
        public Button button;

        public ButtonModeInBadEnding badEndingMode = ButtonModeInBadEnding.SwitchToBad;

        [Header("Normal")]
        public UnityEvent onNormalClick;

        [Header("Bad Ending")]
        public UnityEvent onBadEndingClick;
    }

    [Header("Buttons (Optional)")]
    [Tooltip("把 Scene2 的按鈕都放進來，壞結局模式就能鎖/換事件")]
    public List<ButtonRoute> buttonRoutes = new List<ButtonRoute>();

    [Header("Bad Ending Mode Events (Optional)")]
    [Tooltip("進入 BadEndingMode 時想做的事：顯示某個 panel、播放音效、鎖玩家…")]
    public UnityEvent onEnterBadEndingMode;

    [Tooltip("離開 BadEndingMode（開始播 ending UI）前想做的事")]
    public UnityEvent onExitBadEndingMode;

    private bool _endingTriggered = false;
    private bool _routesAppliedAsBad = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        // 先套用一次「正常模式」按鈕事件
        ApplyButtonRoutesAsBad(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 切場景後，按鈕引用可能變了（Scene2 UI 重新生成）
        // 如果你每個場景都有不同 UI，你就需要在 Inspector 重新拖對，或在這裡用 Find 重抓（可自行擴充）
        // 這裡先做：根據目前 state 重新套用按鈕規則
        ApplyButtonRoutesAsBad(state == EndingState.BadEndingMode);
    }

    private void Update()
    {
        // ---- 背景依焦慮改變（可選）----
        UpdateBackgroundByAnxiety();

        // ---- Debug 快捷鍵（可選）----
        if (CanUseDebugTriggers())
        {
            if (Input.GetKeyDown(KeyCode.F7)) Debug_TriggerBad();
            if (Input.GetKeyDown(KeyCode.F8)) Debug_TriggerNormal();
            if (Input.GetKeyDown(KeyCode.F9)) Debug_TriggerGood();
        }

        if (_endingTriggered) return;

        // 1) 壞結局：只在指定場景才監控
        if (IsBadEndingAllowedInThisScene() && GetAnxiety() >= badEndingAnxietyThreshold)
        {
            if (badEndingUseInSceneMode)
                EnterBadEndingMode();
            else
                TriggerEndingUI(badEnding);

            return;
        }
    }

    /// <summary>
    /// 第五天結束時呼叫（例如 DayEnd/睡覺按鈕/回合結算）
    /// </summary>
    public void OnDayEnded(int dayNumberJustEnded)
    {
        if (_endingTriggered) return;

        // 壞結局優先（但同樣限制：只在允許場景才會觸發壞結局）
        if (IsBadEndingAllowedInThisScene() && GetAnxiety() >= badEndingAnxietyThreshold)
        {
            if (badEndingUseInSceneMode)
                EnterBadEndingMode();
            else
                TriggerEndingUI(badEnding);

            return;
        }

        // 2) 第五天結束：好/一般結局判定（不受場景限制）
        if (dayNumberJustEnded >= normalEndingDay)
        {
            if (goodRouteUnlocked && goodEnding != null)
                TriggerEndingUI(goodEnding);
            else
                TriggerEndingUI(normalEnding);

            return;
        }
    }

    public void UnlockGoodRoute() => goodRouteUnlocked = true;

    // =========================================================
    // ✅ 壞結局：同場景互動模式
    // =========================================================
    public void EnterBadEndingMode()
    {
        if (_endingTriggered) return;
        if (state != EndingState.None) return;

        state = EndingState.BadEndingMode;
        _endingTriggered = true; // 進入結局流程就鎖住，避免反覆進入
        Debug.Log("[EndingManager] Enter BadEndingMode (in-scene).");

        // 按鈕切換為壞結局規則
        ApplyButtonRoutesAsBad(true);

        // 可選：觸發 UI/音效/鎖玩家等
        onEnterBadEndingMode?.Invoke();

        if (pauseGameInBadEndingMode)
            Time.timeScale = 0f;
    }

    /// <summary>
    /// 由「壞結局模式」中的某個按鈕呼叫：正式開始播 ending prefab（文字+圖片）
    /// </summary>
    public void StartBadEndingSequence()
    {
        if (state != EndingState.BadEndingMode) return;

        if (pauseGameInBadEndingMode)
            Time.timeScale = 1f;

        onExitBadEndingMode?.Invoke();

        state = EndingState.PlayingEndingUI;
        TriggerEndingUI(badEnding, true);
    }

    // =========================================================
    // ✅ Debug Button 入口：UI Button OnClick 直接呼叫
    // =========================================================
    public void Debug_TriggerBad()
    {
        if (!CanUseDebugTriggers()) return;

        // 如果你想測試「壞結局同場景模式」就進模式；想直接播 UI 就把 badEndingUseInSceneMode 關掉
        if (badEndingUseInSceneMode && IsBadEndingAllowedInThisScene())
            EnterBadEndingMode();
        else
            TriggerEndingUI(badEnding, true);
    }

    public void Debug_TriggerNormal()
    {
        if (!CanUseDebugTriggers()) return;
        TriggerEndingUI(normalEnding, true);
    }

    public void Debug_TriggerGood()
    {
        if (!CanUseDebugTriggers()) return;
        TriggerEndingUI(goodEnding, true);
    }

    public void Debug_ResetEndingLock()
    {
        // 讓你測試時可以重複觸發
        _endingTriggered = false;
        state = EndingState.None;
        ApplyButtonRoutesAsBad(false);

        if (pauseGameInBadEndingMode)
            Time.timeScale = 1f;

        Debug.Log("[EndingManager] Debug reset ending state.");
    }

    private bool CanUseDebugTriggers()
    {
        if (!enableDebugTriggers) return false;

        if (debugOnlyInEditor)
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
        return true;
    }

    // =========================================================
    // ✅ 真正播放 ending prefab（文字+圖片）
    // =========================================================
    private void TriggerEndingUI(EndingSequence seq, bool isDebug = false)
    {
        if (seq == null)
        {
            Debug.LogError("[EndingManager] Ending sequence is NULL!");
            return;
        }

        // 進入播放 UI 狀態
        state = EndingState.PlayingEndingUI;

        EndingSequencePlayer player = endingPlayerPrefabOrInScene;
        if (player == null)
        {
            Debug.LogError("[EndingManager] No EndingSequencePlayer assigned.");
            return;
        }

        // 如果你丟的是 prefab，就 Instantiate
        if (!player.gameObject.scene.IsValid())
            player = Instantiate(player);

        if (!player.gameObject.activeInHierarchy)
            player.gameObject.SetActive(true);

        // ✅ 必須有 Play(seq)
        player.Play(seq);

        Debug.Log(isDebug
            ? $"[EndingManager] (DEBUG) Trigger Ending UI: {seq.endingId}"
            : $"[EndingManager] Trigger Ending UI: {seq.endingId}");
    }

    // =========================================================
    // ✅ 場景限制：壞結局只在特定場景才會觸發
    // =========================================================
    private bool IsBadEndingAllowedInThisScene()
    {
        if (!badEndingOnlyInSpecificScenes) return true;
        string cur = SceneManager.GetActiveScene().name;
        return badEndingAllowedScenes != null && badEndingAllowedScenes.Contains(cur);
    }

    // =========================================================
    // ✅ Scene2 背景依焦慮值變化
    // =========================================================
    private void UpdateBackgroundByAnxiety()
    {
        if (scene2BackgroundImage == null) return;

        // 限定場景（留空=所有場景）
        if (backgroundAffectScenes != null && backgroundAffectScenes.Count > 0)
        {
            string cur = SceneManager.GetActiveScene().name;
            if (!backgroundAffectScenes.Contains(cur)) return;
        }

        int anxiety = GetAnxiety();

        // ✅ 到門檻就換圖
        int threshold = (backgroundSwapThreshold > 0) ? backgroundSwapThreshold : badEndingAnxietyThreshold;

        if (swapSpriteAtBadThreshold && anxiety >= threshold)
        {
            if (!_bgSwapped)
            {
                if (backgroundBadSprite != null)
                    scene2BackgroundImage.sprite = backgroundBadSprite;

                // 換圖後通常希望不要被 tint 影響
                scene2BackgroundImage.color = Color.white;

                _bgSwapped = true;
            }
            return; // 已經是壞背景，不再做漸變
        }

        // ✅ 回到低焦慮（例如你 debug 降回去）就恢復正常圖 & 重新允許漸變
        if (_bgSwapped)
        {
            if (backgroundNormalSprite != null)
                scene2BackgroundImage.sprite = backgroundNormalSprite;

            _bgSwapped = false;
        }

        // 低焦慮區間：維持你原本的漸變 tint（可選）
        if (backgroundTintByAnxiety != null)
        {
            float t = Mathf.InverseLerp(backgroundMinAnxiety, backgroundMaxAnxiety, anxiety);
            scene2BackgroundImage.color = backgroundTintByAnxiety.Evaluate(t);
        }
        else
        {
            // 沒設 Gradient 就不要亂改色
            scene2BackgroundImage.color = Color.white;
        }
    }

    // =========================================================
    // ✅ 按鈕限制 / 改事件（壞結局模式）
    // =========================================================
    private void ApplyButtonRoutesAsBad(bool asBad)
    {
        if (buttonRoutes == null || buttonRoutes.Count == 0) return;
        if (_routesAppliedAsBad == asBad) return;

        _routesAppliedAsBad = asBad;

        foreach (var r in buttonRoutes)
        {
            if (r == null || r.button == null) continue;

            r.button.onClick.RemoveAllListeners();

            if (!asBad)
            {
                // 正常模式：一定可按（你也可以改成依自己需求）
                r.button.interactable = true;
                r.button.onClick.AddListener(() => r.onNormalClick?.Invoke());
                continue;
            }

            // 壞結局模式：依設定決定
            switch (r.badEndingMode)
            {
                case ButtonModeInBadEnding.Disable:
                    r.button.interactable = false;
                    break;

                case ButtonModeInBadEnding.KeepNormal:
                    r.button.interactable = true;
                    r.button.onClick.AddListener(() => r.onNormalClick?.Invoke());
                    break;

                case ButtonModeInBadEnding.SwitchToBad:
                    r.button.interactable = true;
                    r.button.onClick.AddListener(() => r.onBadEndingClick?.Invoke());
                    break;
            }
        }
    }

    // =========================================================
    // ✅ Replace these with your real GameManager getters
    // =========================================================
    private int GetAnxiety()
    {
        // TODO: 改成讀你的真正 anxiety 來源，例如：
        // return GameManager.Instance.anxiety;
        return debugAnxiety;
    }

    private int GetCurrentDay()
    {
        // TODO: return GameManager.Instance.day;
        return debugCurrentDay;
    }
}