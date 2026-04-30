using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatChangeUI : MonoBehaviour
{
    public static StatChangeUI Instance;

    [Header("基本設定")]
    public RectTransform container;                  // 所有小字的父物件
    public TextMeshProUGUI popupTextPrefab;          // 小字模板（建議用場景/Prefab 裡關閉的 TMP 物件）

    [Header("排列 & 動畫")]
    public float rowSpacing = 6f;                    // 每一行之間的距離
    public float enterOffsetY = 24f;                 // 從底下滑上來的起始位移
    public float moveSpeed = 15f;                    // 滑動速度

    [Header("存留 & 淡出")]
    public float stayTime = 0.8f;                    // 一般數值跳字停留時間
    public float fadeTime = 0.4f;                    // 淡出時間

    [Header("多行文字 / 版面")]
    public float bottomBaseOffset = 70f;             // 整串文字離底部多高
    public float minimumLineHeight = 30f;            // 每條最小高度
    public float extraLinePadding = 12f;             // 每條文字額外補的空間
    public float extraPerWrappedLine = 10f;          // 每多一行再額外補多少空間

    // 固定事件描述
    private RectTransform persistentEventRect;
    private CanvasGroup persistentEventLine;
    private TextMeshProUGUI persistentEventText;
    private Vector2 persistentEventTargetPos;

    // 一般數值跳字
    private readonly List<PopupEntry> activePopups = new List<PopupEntry>();

    private class PopupEntry
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
        public Vector2 targetPos;
        public float timer;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 固定事件描述也做滑入 / 平滑移動
        if (persistentEventRect != null)
        {
            persistentEventRect.anchoredPosition = Vector2.Lerp(
                persistentEventRect.anchoredPosition,
                persistentEventTargetPos,
                Time.deltaTime * moveSpeed
            );
        }

        if (activePopups.Count == 0)
            return;

        for (int i = activePopups.Count - 1; i >= 0; i--)
        {
            PopupEntry entry = activePopups[i];
            if (entry == null || entry.rect == null)
            {
                activePopups.RemoveAt(i);
                continue;
            }

            entry.timer += Time.deltaTime;

            // 平滑移動到目標位置
            entry.rect.anchoredPosition = Vector2.Lerp(
                entry.rect.anchoredPosition,
                entry.targetPos,
                Time.deltaTime * moveSpeed
            );

            // 淡出控制
            if (entry.timer <= stayTime)
            {
                entry.canvasGroup.alpha = 1f;
            }
            else
            {
                float t = Mathf.InverseLerp(stayTime, stayTime + fadeTime, entry.timer);
                entry.canvasGroup.alpha = 1f - t;
            }

            // 時間到就刪掉
            if (entry.timer >= stayTime + fadeTime)
            {
                Destroy(entry.rect.gameObject);
                activePopups.RemoveAt(i);
                RecalculateTargets();
            }
        }
    }

    /// <summary>
    /// 顯示某個屬性改變的小字，例如：焦慮 +10
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

    /// <summary>
    /// 顯示固定事件描述。
    /// 這條會固定待在最下面，直到 RandomEventManager 呼叫 ClearPersistentEventDescription()
    /// </summary>
    public void ShowPersistentEventDescription(string message)
    {
        if (popupTextPrefab == null || container == null)
        {
            Debug.LogWarning("StatChangeUI：container 或 popupTextPrefab 沒有設定");
            return;
        }

        bool isNewLine = false;

        if (persistentEventLine == null || persistentEventRect == null || persistentEventText == null)
        {
            TextMeshProUGUI tmp = Instantiate(popupTextPrefab, container);
            tmp.gameObject.SetActive(true);

            persistentEventRect = tmp.rectTransform;
            persistentEventText = tmp;

            persistentEventLine = tmp.GetComponent<CanvasGroup>();
            if (persistentEventLine == null)
                persistentEventLine = tmp.gameObject.AddComponent<CanvasGroup>();

            persistentEventLine.alpha = 1f;

            // 新建立時先放在下面，之後 Update 會把它滑到目標位置
            persistentEventRect.anchoredPosition = new Vector2(0f, -enterOffsetY);
            isNewLine = true;
        }

        persistentEventText.text = message;
        ApplyAutoWrappedHeight(persistentEventText);

        persistentEventLine.alpha = 1f;

        if (isNewLine)
        {
            persistentEventRect.anchoredPosition = new Vector2(0f, -enterOffsetY);
        }

        RecalculateTargets();
    }

    /// <summary>
    /// 事件結束時清掉固定事件描述。
    /// </summary>
    public void ClearPersistentEventDescription()
    {
        if (persistentEventRect != null)
        {
            Destroy(persistentEventRect.gameObject);
        }

        persistentEventRect = null;
        persistentEventLine = null;
        persistentEventText = null;
        persistentEventTargetPos = Vector2.zero;

        RecalculateTargets();
    }

    private void CreatePopup(string text)
    {
        if (popupTextPrefab == null || container == null)
        {
            Debug.LogWarning("StatChangeUI：container 或 popupTextPrefab 沒有設定");
            return;
        }

        TextMeshProUGUI tmp = Instantiate(popupTextPrefab, container);
        tmp.gameObject.SetActive(true);
        tmp.text = text;

        ApplyAutoWrappedHeight(tmp);

        RectTransform rect = tmp.rectTransform;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = rect.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 1f;

        // 一開始先從底下滑出來
        rect.anchoredPosition = new Vector2(0f, -enterOffsetY);

        PopupEntry entry = new PopupEntry
        {
            rect = rect,
            canvasGroup = cg,
            targetPos = Vector2.zero,
            timer = 0f
        };

        // 新的數值變動永遠插在最下面那個數值位置
        // 如果有固定事件描述，它就會自然待在事件描述上面
        activePopups.Insert(0, entry);

        RecalculateTargets();
    }

    /// <summary>
    /// 自動換行 + 依內容撐高文字框
    /// </summary>
    private void ApplyAutoWrappedHeight(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;

        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.ForceMeshUpdate();

        RectTransform rt = tmp.rectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        float preferredHeight = GetDisplayHeight(tmp);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    /// <summary>
    /// 依 TMP 實際內容算出這條文字應該佔多少高度
    /// </summary>
    private float GetDisplayHeight(TextMeshProUGUI tmp)
    {
        if (tmp == null)
            return minimumLineHeight;

        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.ForceMeshUpdate();

        float h = tmp.preferredHeight;
        if (h < minimumLineHeight)
            h = minimumLineHeight;

        int lineCount = Mathf.Max(1, tmp.textInfo.lineCount);

        h += extraLinePadding;

        if (lineCount > 1)
        {
            h += (lineCount - 1) * extraPerWrappedLine;
        }

        return h;
    }

    /// <summary>
    /// 重新計算所有行的位置：
    /// description 在最下面，
    /// 之後的數值跳字依 description 高度往上排。
    /// </summary>
    private void RecalculateTargets()
    {
        float currentY = bottomBaseOffset;

        // 固定事件描述放最下面
        if (persistentEventRect != null && persistentEventText != null)
        {
            float eventHeight = GetDisplayHeight(persistentEventText);
            persistentEventRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, eventHeight);

            persistentEventTargetPos = new Vector2(0f, currentY);
            currentY += eventHeight + rowSpacing;
        }

        // 其他數值跳字從 description 上面開始往上堆
        for (int i = 0; i < activePopups.Count; i++)
        {
            PopupEntry e = activePopups[i];
            if (e == null || e.rect == null) continue;

            TextMeshProUGUI tmp = e.rect.GetComponent<TextMeshProUGUI>();
            float thisHeight = GetDisplayHeight(tmp);
            e.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, thisHeight);

            e.targetPos = new Vector2(0f, currentY);
            currentY += thisHeight + rowSpacing;
        }
    }
}