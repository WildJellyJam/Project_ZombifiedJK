using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EventLogUI : MonoBehaviour
{
    public static EventLogUI Instance { get; private set; }

    [Header("Layout")]
    [Tooltip("裝所有事件文字的父物件（放在 Canvas 底下的空物件 / Panel）")]
    public RectTransform container;

    [Tooltip("一行事件文字用的 Prefab（裡面要有 TextMeshProUGUI）")]
    public GameObject linePrefab;

    [Header("行為設定")]
    [Tooltip("每一行預設存在多久才開始淡出（秒）")]
    public float defaultLifetime = 2.5f;

    [Tooltip("淡出需要多久（秒）")]
    public float fadeDuration = 0.5f;

    [Tooltip("預設是否啟用自動淡出（第二個參數 = true 時會忽略）")]
    public bool enableDefaultAutoFade = true;

    [Tooltip("畫面上最多顯示幾行（超過會刪掉最舊那一行）")]
    public int maxLinesOnScreen = 5;

    // 目前在畫面上的所有行（用 CanvasGroup 控制透明度）
    private readonly List<CanvasGroup> activeLines = new List<CanvasGroup>();

    private void Awake()
    {
        // Singleton，方便別的腳本用 EventLogUI.Instance 呼叫
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 顯示一行事件文字
    /// keepUntilManuallyCleared = true  -> 不會自動淡出，要你自己在事件結束時 Clear
    /// keepUntilManuallyCleared = false -> 會依 defaultLifetime + fadeDuration 自動淡出
    /// </summary>
    public void ShowEventMessage(string message, bool keepUntilManuallyCleared)
    {
        if (container == null || linePrefab == null)
        {
            Debug.LogWarning("EventLogUI：container 或 linePrefab 沒有設定", this);
            return;
        }

        // 實例化一行
        GameObject lineGO = Instantiate(linePrefab, container);
        lineGO.transform.SetAsFirstSibling();



        // 設定文字
        TextMeshProUGUI text = lineGO.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = message;
        }

        // 用 CanvasGroup 控制透明度
        CanvasGroup cg = lineGO.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = lineGO.AddComponent<CanvasGroup>();
        }
        cg.alpha = 1f;

        activeLines.Add(cg);

        // 限制最大行數：超過就刪掉最舊的那一行
        if (activeLines.Count > maxLinesOnScreen)
        {
            CanvasGroup oldest = activeLines[0];
            activeLines.RemoveAt(0);
            if (oldest != null)
            {
                Destroy(oldest.gameObject);
            }
        }

        // 決定要不要自動淡出
        bool shouldAutoFade = enableDefaultAutoFade && !keepUntilManuallyCleared;
        if (shouldAutoFade)
        {
            StartCoroutine(FadeAndRemoveLine(cg, defaultLifetime, fadeDuration));
        }
    }

    private IEnumerator FadeAndRemoveLine(CanvasGroup cg, float lifetime, float fadeTime)
    {
        // 等待存活時間
        yield return new WaitForSeconds(lifetime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (cg == null) yield break;

            float normalized = t / fadeTime;
            cg.alpha = Mathf.Lerp(1f, 0f, normalized);

            yield return null;
        }

        if (cg != null)
        {
            activeLines.Remove(cg);
            Destroy(cg.gameObject);
        }
    }

    /// <summary>
    /// 事件結束時，如果你希望所有 log 一次消掉，可以呼叫這個。
    /// （例如在 RandomEventManager.OnEventConfirmButtonPressed 裡用）
    /// </summary>
    public void ClearAllMessages()
    {
        foreach (var cg in activeLines)
        {
            if (cg != null)
            {
                Destroy(cg.gameObject);
            }
        }
        activeLines.Clear();
    }
    public void StartFadeOutAll()
    {
        // 做一份目前行的副本，避免一邊淡出一邊改到原本的 List
        var snapshot = new List<CanvasGroup>(activeLines);

        foreach (var cg in snapshot)
        {
            if (cg != null)
            {
                // lifetime 設 0：立刻開始往 0 alpha 淡
                StartCoroutine(FadeAndRemoveLine(cg, 0f, fadeDuration));
            }
        }
    }

}
