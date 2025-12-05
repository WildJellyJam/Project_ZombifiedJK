using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatChangeUI : MonoBehaviour
{
    public static StatChangeUI Instance;

    [Header("基本設定")]
    [SerializeField] private RectTransform container;         // 所有小字的父物件
    [SerializeField] private TextMeshProUGUI popupTextPrefab; // 小字模板（可以是場景裡關閉的 Text TMP）

    [Header("排列 & 動畫")]
    [SerializeField] private float rowSpacing = 4f;       // 每一行之間的距離
    [SerializeField] private float enterOffsetY = 20f;    // 從底下出現的距離（越大越下面）
    [SerializeField] private float moveSpeed = 15f;       // 行往目標位置移動的速度（越大越快）

    [Header("存留 & 淡出")]
    [SerializeField] private float stayTime = 0.8f;       // 保持不淡出的時間
    [SerializeField] private float fadeTime = 0.4f;       // 淡出時間

    // 一行的狀態
    private class PopupEntry
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
        public Vector2 targetPos;
        public float timer;    // 用來算停留 + 淡出
    }

    private readonly List<PopupEntry> activePopups = new List<PopupEntry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 如果要跨場景沿用可以開這行
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (activePopups.Count == 0) return;

        // 更新每一行的位置 & 淡出
        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            PopupEntry entry = activePopups[i];
            if (entry.rect == null)
            {
                activePopups.RemoveAt(i);
                continue;
            }

            entry.timer += Time.deltaTime;

            // 位置：往 targetPos 插值靠近（做出順滑上移效果）
            entry.rect.anchoredPosition = Vector2.Lerp(
                entry.rect.anchoredPosition,
                entry.targetPos,
                Time.deltaTime * moveSpeed
            );

            // 透明度控制
            if (entry.timer <= stayTime)
            {
                // 完全不淡出
                entry.canvasGroup.alpha = 1f;
            }
            else
            {
                float t = Mathf.InverseLerp(stayTime, stayTime + fadeTime, entry.timer);
                entry.canvasGroup.alpha = 1f - t;
            }

            // 時間到了 → 移除
            if (entry.timer >= stayTime + fadeTime)
            {
                Destroy(entry.rect.gameObject);
                activePopups.RemoveAt(i);
                // 有人被刪掉後，重新計算剩下行的目標位置，讓空位補上來
                RecalculateTargets();
            }
        }
    }

    /// <summary>
    /// 顯示某個屬性改變的小字，例如：焦慮 +1
    /// </summary>
    public void ShowStatChange(string statName, float delta)
    {
        if (Mathf.Approximately(delta, 0f))
            return;

        string sign = delta > 0 ? "+" : "";

        string valueText;
        if (Mathf.Approximately(delta % 1f, 0f))
            valueText = ((int)delta).ToString();
        else
            valueText = delta.ToString("0.0");

        string text = $"{statName} {sign}{valueText}";

        CreatePopup(text);
    }

    private void CreatePopup(string text)
    {
        if (popupTextPrefab == null || container == null)
        {
            Debug.LogWarning("StatChangeUI：container 或 popupTextPrefab 沒有設定");
            return;
        }

        // 生成新的 TMP Text
        TextMeshProUGUI tmp = Instantiate(popupTextPrefab, container);
        tmp.gameObject.SetActive(true);
        tmp.text = text;

        RectTransform rect = tmp.rectTransform;

        // CanvasGroup
        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg == null) cg = rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        // 新的一行：一開始在「比最底下一行再下面一點」
        float lineHeight = rect.rect.height;
        Vector2 startPos = new Vector2(0f, -enterOffsetY);
        rect.anchoredPosition = startPos;

        PopupEntry entry = new PopupEntry
        {
            rect = rect,
            canvasGroup = cg,
            targetPos = Vector2.zero, // 先暫時給 0，下面 RecalculateTargets 會更新
            timer = 0f
        };

        // 新的一行插入最下面（index 0）
        activePopups.Insert(0, entry);

        // 有新的一行加入，重新算每一行應該在第幾排
        RecalculateTargets();
    }

    /// <summary>
    /// 重新計算每一行要待的位置：
    /// index 0 → 第一行（最下面）
    /// index 1 → 第二行
    /// index 2 → 第三行 ...
    /// </summary>
    private void RecalculateTargets()
    {
        float lineHeight = popupTextPrefab.rectTransform.rect.height;
        float step = lineHeight + rowSpacing;

        for (int i = 0; i < activePopups.Count; i++)
        {
            PopupEntry e = activePopups[i];
            if (e.rect == null) continue;

            // 從 container 的 anchoredPosition(0,0) 往上堆
            e.targetPos = new Vector2(0f, i * step);
        }
    }
}
