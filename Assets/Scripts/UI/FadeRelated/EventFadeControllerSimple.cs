using UnityEngine;

public class EventFadeControllerSimple : MonoBehaviour
{
    [Header("事件黑幕淡入（先測試用）")]
    public CanvasGroup blackFadeCanvasGroup;

    private void Start()
    {
        if (blackFadeCanvasGroup == null)
        {
            Debug.LogError("[EventFadeTest] blackFadeCanvasGroup 沒有指定！");
            return;
        }

        // 一進場先藏起來
        blackFadeCanvasGroup.alpha = 0f;
        blackFadeCanvasGroup.blocksRaycasts = false;
        blackFadeCanvasGroup.gameObject.SetActive(false);

        Debug.Log("[EventFadeTest] Start 完成，黑幕已隱藏。");
    }

    // 🔴 Button OnClick 就叫這個
    public void OnConfirmClicked()
    {
        Debug.Log("[EventFadeTest] OnConfirmClicked 被呼叫。");

        if (blackFadeCanvasGroup == null)
        {
            Debug.LogError("[EventFadeTest] OnConfirmClicked 時 blackFadeCanvasGroup 是空的！");
            return;
        }

        // 直接把黑幕打開 + alpha 設成 1，看它會不會變黑
        blackFadeCanvasGroup.gameObject.SetActive(true);
        blackFadeCanvasGroup.blocksRaycasts = true;
        blackFadeCanvasGroup.alpha = 1f;

        Debug.Log($"[EventFadeTest] 直接把 alpha 設成 1，目前 alpha = {blackFadeCanvasGroup.alpha}");
    }
}
