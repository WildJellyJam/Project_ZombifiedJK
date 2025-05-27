using JetBrains.Annotations;
using UnityEngine;

public enum TimePeriod
{
    AtSchoolAfterClass,       // 放學（16:00-16:30）
    AtHomeBeforeSleep,        // 回家睡前（16:30）
    AtHomeAfterWakeUp,        // 起床後活動（21:00-次日6:00）
    AtHomeBeforeLeaving,      // 準備出門（6:00-7:00）
    AtSchool,                 // 上學（7:30-16:00）
    AtHome,                   // 不出門時的通用場景
    InCity,                   // 週末市區（15:00-18:00）
    AtSupermarket,            // 買牛奶事件
    AtHomeParentsArgue,       // 爸媽吵架
    AtAdventure               // 探險事件
}

[System.Serializable]
public class GameTime
{
    public int day;           // 第幾天（1-7）
    public float hours;       // 當前小時（0-24）
    public TimePeriod currentPeriod; // 當前時間段
}


public class TimeSystem
{
    public GameTime gameTime = new GameTime { day = 1, hours = 16f, currentPeriod = TimePeriod.AtHomeBeforeSleep }; // 初始時間：週一16:00
    // private GameManager gameManager;
    private bool hasTriggeredFixedEvent = false;
    private newGameManager gameManager => newGameManager.Instance;

    public static bool goOut = false; // 根據玩家選擇動態設定，假設第一天強制上學,為了測試，先改成false
    public static bool goToMarket = false;

    public void AddEventTime(float eventHours)
    {
        gameTime.hours += eventHours; // 增加指定小時數（例如1小時）
        if (gameTime.hours >= 24f)
        {
            gameTime.hours -= 24f;
            gameTime.day++;
            // if (gameTime.day > 7) EndGame();
        }

        // 處理睡覺過場（16:30 - 21:00）
        if (gameTime.hours >= 16.5f && gameTime.hours < 21f)
        {
            gameTime.hours = 21f; // 直接跳到21:00
            // UIManager..ShowSleepTransition();
        }

        UpdateTimePeriod();
        Debug.Log($"更新時間：{gameTime.hours}");
        // CheckCommonEvents();

        if (gameTime.hours == 4f && gameTime.day == 2)
        {
            Debug.Log("檢查凌晨4點事件");

            // 觸發固定事件，例如收到訊息
            newGameManager.Instance.randomEventManager.TriggerEvent("ReceiveMessage", true);
        }

        if (gameTime.hours == 22f && gameTime.day == 2)
        {
            Debug.Log("檢查晚上九點事件");

            // 觸發固定事件，例如收到訊息
            // SceneManage.SwitchScene(TimePeriod.AtSupermarket);
            newGameManager.Instance.timeSystem.gameTime.currentPeriod = TimePeriod.AtSupermarket;
        }
        if (goOut)
        {
            newGameManager.Instance.timeSystem.gameTime.currentPeriod = TimePeriod.AtSchool;
            goOut = false;
        }
        if (goToMarket)
        {
            newGameManager.Instance.timeSystem.gameTime.currentPeriod = TimePeriod.AtSupermarket;
            goToMarket = false;
        }

        // 通知 GameManager 更新場景
            gameManager.OnTimeManuallyUpdated();
        
    }

    private void UpdateTimePeriod()
    {
        bool isWeekend = gameTime.day >= 5;
        

        if (isWeekend)
        {
            if (goOut)
            {
                if (gameTime.hours >= 6f && gameTime.hours < 9f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                else if (gameTime.hours >= 9f && gameTime.hours < 12f) gameTime.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                else if (gameTime.hours >= 12f && gameTime.hours < 15f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                else if (gameTime.hours >= 15f && gameTime.hours < 18f) gameTime.currentPeriod = TimePeriod.InCity;
                else gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
            }
            else
            {
                if (gameTime.hours >= 6f && gameTime.hours < 9f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                else if (gameTime.hours >= 9f && gameTime.hours < 12f) gameTime.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                else if (gameTime.hours >= 12f && gameTime.hours < 15f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                else gameTime.currentPeriod = TimePeriod.AtHome;
            }
        }
        else
        {
            if (goOut)
            {
                if (gameTime.hours >= 16f && gameTime.hours < 16.5f) gameTime.currentPeriod = TimePeriod.AtSchoolAfterClass;
                else if (gameTime.hours >= 16.5f && gameTime.hours < 21f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                else if (gameTime.hours >= 21f || gameTime.hours < 6f) gameTime.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                else if (gameTime.hours >= 6f && gameTime.hours < 7f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                else if (gameTime.hours >= 7.5f && gameTime.hours < 16f) gameTime.currentPeriod = TimePeriod.AtSchool;
                else gameTime.currentPeriod = TimePeriod.AtHomeAfterWakeUp; // 通勤時間過渡
            }
            else
            {
                if (gameTime.hours >= 16f && gameTime.hours < 16.5f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                else if (gameTime.hours >= 16.5f && gameTime.hours < 21f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                else if (gameTime.hours >= 21f || gameTime.hours < 6f) gameTime.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                else if (gameTime.hours >= 6f && gameTime.hours < 7f) gameTime.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                else gameTime.currentPeriod = TimePeriod.AtHome;
            }
        }
    }
}