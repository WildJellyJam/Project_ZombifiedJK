using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    public Text timeText; // UI Text 組件，用於顯示時間

    void Start()
    {
        // 訂閱時間更新事件，使用 TimeSystem.TimeUpdatedHandler
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubscribeToTimeUpdated(OnTimeUpdated);
            UpdateTimeDisplay(GameManager.Instance.timeSystem.gameTime); // 初始顯示
        }
    }

    void OnDestroy()
    {
        // 取消訂閱
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnsubscribeFromTimeUpdated(OnTimeUpdated);
        }
    }

    private void OnTimeUpdated(GameTime newTime)
    {
        UpdateTimeDisplay(newTime);
    }

    private void UpdateTimeDisplay(GameTime time)
    {
        if (timeText != null)
        {
            timeText.text = $"第 {time.day} 天, {time.hours:F1} 點\n時間段: {time.currentPeriod}";
        }
    }
}