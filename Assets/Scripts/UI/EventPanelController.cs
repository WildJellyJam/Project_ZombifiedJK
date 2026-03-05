using UnityEngine;

public class EventPanelController : MonoBehaviour
{
    // 給事件面板上的「確認 / 關閉 / 下一步」按鈕用
    public void OnConfirmClicked()
    {
        // 叫 RandomEventManager 處理「事件結束」的流程
        if (newGameManager.Instance != null && newGameManager.Instance.eventManager != null)
        {
            newGameManager.Instance.eventManager.OnEventConfirmButtonPressed();
        }
        else
        {
            Debug.LogWarning("[EventPanelController] 找不到 newGameManager 或 eventManager，只能直接關閉自己。");
            Destroy(gameObject);
        }
    }
}
