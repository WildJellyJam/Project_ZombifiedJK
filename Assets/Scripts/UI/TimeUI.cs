using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public TextMeshProUGUI timeText; // 用於顯示時間的TextMeshProUGUI
    // private TimeSystem timeSystem;

    void Start()
    {
        // 初始顯示當前時間
        UpdateTimeDisplay(newGameManager.Instance.timeSystem.gameTime);
    }


    // 更新時間顯示
    public void UpdateTimeDisplay(GameTime gameTime)
    {
        if (timeText != null)
        {
            int hour = Mathf.FloorToInt(gameTime.hours);
            int minute = Mathf.FloorToInt((gameTime.hours - hour) * 60);
            timeText.text = $"Day {gameTime.day}\n{hour:00}:{minute:00}";
            Debug.Log($"已更新時間：{gameTime.hours}");
        }
    }
}