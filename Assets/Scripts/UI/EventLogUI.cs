using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventLogUI : MonoBehaviour
{
    public static EventLogUI Instance;

    [Header("基本設定")]
    [SerializeField] private RectTransform container;         // 所有事件文字的父物件
    [SerializeField] private TextMeshProUGUI linePrefab;      // 每一行文字的模板（場景裡關閉的 Text TMP 也可以）

    [Header("排列 & 動畫")]
    [SerializeField] private float rowSpacing = 4f;       // 每一行之間的距離
    [SerializeField] private float enterOffsetY = 20f;    // 新行從下面出現的距離
    [SerializeField] private float moveSpeed = 15f;       // 行往目標位置移動的速度（越大越快）

    [Header("淡出設定")]
    [SerializeField] private float stayTime = 0.8f;       // 開始淡出前，至少維持顯示的時間
    [SerializeField] private float fadeTime = 0.5f;       // 淡出時間（從 1 到 0）

    // 一行文字的狀態
    private class Entry
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
        public Vector2 targetPos;

        public bool holdUntilEventEnd; // true = 等事件結束才開始淡
        public bool isFading;          // true = 正在進入 fade 流程
        public float timer;            // 計算顯示/淡出的時間
    }

    private readonly List<Entry> activeEntries = new List<Entry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 要跨場景的話可以開：
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (activeEntries.Count == 0) return;

        for (int i = activeEntries.Count - 1; i >= 0; i--)
        {
            Entry entry = activeEntries[i];
            if (entry.rect == null)
            {
                activeEntries.RemoveAt(i);
                continue;
            }

            // 位置：滑向自己的 targetPos（樓梯式往上）
            entry.rect.anchoredPosition = Vector2.Lerp(
                entry.rect.anchoredPosition,
                entry.targetPos,
                Time.deltaTime * moveSpeed
            );

            // ── 處理顯示 / 淡出 ──

            // 「事件結束前都要維持顯示」而且還沒被要求淡出
            if (entry.holdUntilEventEnd && !entry.isFading)
            {
                entry.canvasGroup.alpha = 1f;
                // 不進行 timer 累計，等開始淡出時才算
                continue;
            }

            // 自動淡出 或 已經被要求開始淡出
            entry.timer += Time.deltaTime;

            if (entry.timer <= stayTime)
            {
                entry.canvasGroup.alpha = 1f;
            }
            else
            {
                float t = Mathf.InverseLerp(stayTime, stayTime + fadeTime, entry.timer);
                entry.canvasGroup.alpha = 1f - t;
            }

            if (entry.timer >= stayTime + fadeTime)
            {
                Destroy(entry.rect.gameObject);
                activeEntries.RemoveAt(i);
                RecalculateTargets(); // 有空位後讓剩下的往下補
            }
        }
    }

    /// <summary>
    /// 顯示事件文字（預設：會一直顯示，直到你呼叫 StartFadeOutAll 才淡出）
    /// </summary>
    public void ShowEventMessage(string text)
    {
        ShowEventMessage(text, true);
    }

    /// <summary>
    /// 顯示事件文字
    /// holdUntilEventEnd = true：事件結束前不會淡出
    /// holdUntilEventEnd = false：自己待一段時間後自動淡出
    /// </summary>
    public void ShowEventMessage(string text, bool holdUntilEventEnd)
    {
        if (linePrefab == null || container == null)
        {
            Debug.LogWarning("EventLogUI：container 或 linePrefab 沒有設定");
            return;
        }

        // 生成文字
        TextMeshProUGUI tmp = Instantiate(linePrefab, container);
        tmp.gameObject.SetActive(true);
        tmp.text = text;

        RectTransform rect = tmp.rectTransform;

        // CanvasGroup 控制透明度
        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg == null) cg = rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 新的行一開始在「底下」，再滑上來
        rect.anchoredPosition = new Vector2(0f, -enterOffsetY);

        Entry entry = new Entry
        {
            rect = rect,
            canvasGroup = cg,
            targetPos = Vector2.zero, // 先給 0，等一下 RecalculateTargets 會重新排
            holdUntilEventEnd = holdUntilEventEnd,
            // 如果不需要等事件結束，就直接進入淡出流程
            isFading = !holdUntilEventEnd,
            timer = 0f
        };

        // 新的一行插到「最下面」（index 0）
        activeEntries.Insert(0, entry);

        // 重新算每一行應該在第幾排
        RecalculateTargets();
    }

    /// <summary>
    /// 重新計算每一行要待的位置：
    /// index 0 → 第一行（最下面）
    /// index 1 → 第二行...
    /// </summary>
    private void RecalculateTargets()
    {
        float lineHeight = linePrefab.rectTransform.rect.height;
        float step = lineHeight + rowSpacing;

        for (int i = 0; i < activeEntries.Count; i++)
        {
            Entry e = activeEntries[i];
            if (e.rect == null) continue;

            e.targetPos = new Vector2(0f, i * step);
        }
    }

    /// <summary>
    /// 通常在「事件結束」時呼叫：
    /// 讓目前還沒淡出的所有文字開始進入淡出流程
    /// </summary>
    public void StartFadeOutAll()
    {
        foreach (Entry e in activeEntries)
        {
            e.isFading = true;
            e.timer = 0f; // 從現在開始算 stayTime + fadeTime
        }
    }
}
