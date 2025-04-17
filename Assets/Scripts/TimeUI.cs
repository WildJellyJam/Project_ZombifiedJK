using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText; // 用於顯示時間的TextMeshProUGUI
    private GameManager gameManager;

    void Start()
    {
        // 獲取GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("未找到GameManager，請確保場景中有GameManager物件！");
            return;
        }

        // 訂閱時間更新事件
        gameManager.SubscribeToTimeUpdated(UpdateTimeDisplay);

        // 初始顯示當前時間
        UpdateTimeDisplay(gameManager.timeSystem.gameTime);
    }

    void OnDestroy()
    {
        // 取消訂閱，防止記憶體洩漏
        if (gameManager != null)
        {
            gameManager.UnsubscribeFromTimeUpdated(UpdateTimeDisplay);
        }
    }

    // 更新時間顯示
    private void UpdateTimeDisplay(GameTime gameTime)
    {
        if (timeText != null)
        {
            int hour = Mathf.FloorToInt(gameTime.hours);
            int minute = Mathf.FloorToInt((gameTime.hours - hour) * 60);
            timeText.text = $"Day {gameTime.day}, {hour:00}:{minute:00} ({gameTime.currentPeriod})";
        }
    }
}