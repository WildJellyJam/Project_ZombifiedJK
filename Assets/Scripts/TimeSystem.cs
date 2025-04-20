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

public class TimeSystem : MonoBehaviour
{
    public GameTime gameTime = new GameTime { day = 1, hours = 16f, currentPeriod = TimePeriod.AtSchoolAfterClass }; // 初始時間：週一16:00
    private GameManager gameManager;
    private bool hasTriggeredFixedEvent = false;

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("未找到GameManager！");
        }
    }

    public void AddEventTime(float eventHours)
    {
        gameTime.hours += eventHours; // 增加指定小時數（例如1小時）
        if (gameTime.hours >= 24f)
        {
            gameTime.hours -= 24f;
            gameTime.day++;
            if (gameTime.day > 7) EndGame();
        }

        // 處理睡覺過場（16:30 - 21:00）
        if (gameTime.day == 1 && gameTime.hours >= 16.5f && gameTime.hours < 21f)
        {
            gameTime.hours = 21f; // 直接跳到21:00
            UIManager.Instance.ShowSleepTransition();
        }

        UpdateTimePeriod();
        CheckCommonEvents();

        // 通知 GameManager 更新場景
        gameManager.OnTimeManuallyUpdated();
    }

    private void UpdateTimePeriod()
    {
        bool isWeekend = gameTime.day >= 6;
        bool goOut = true; // 根據玩家選擇動態設定，假設第一天強制上學

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
    private void CheckCommonEvents()
    {
        // 第一天16:00-16:30 收到訊息（非強制）
        if (gameTime.day == 1 && gameTime.hours >= 16f && gameTime.hours < 16.5f)
        {
            gameManager.randomEventManager.TriggerEvent("ReceiveMessage", false);
            hasTriggeredFixedEvent = true;
        }
        // 第二天21:00-22:00 買牛奶（強制）
        if (gameTime.day == 2 && gameTime.hours >= 21f && gameTime.hours < 22f)
        {
            gameManager.randomEventManager.TriggerEvent("BuyMilk", true);
            hasTriggeredFixedEvent = true;
        }
        // 第三天21:00-23:00 收到貓咪影片（非強制）
        if (gameTime.day == 3 && gameTime.hours >= 21f && gameTime.hours < 23f)
        {
            gameManager.randomEventManager.TriggerEvent("ReceiveCatVideo", false);
            hasTriggeredFixedEvent = true;
        }
        // 第三天6:00-7:00 爸媽吵架（強制）
        if (gameTime.day == 3 && gameTime.hours >= 6f && gameTime.hours < 7f)
        {
            gameManager.randomEventManager.TriggerEvent("ParentsArgue", true);
            hasTriggeredFixedEvent = true;
        }
        // 第四天2:00-4:00 探險遇到希多（觸發後強制）
        if (gameTime.day == 4 && gameTime.hours >= 2f && gameTime.hours < 4f)
        {
            bool hasTriggeredAdventure = gameManager.randomEventManager.HasTriggered("MeetSido");
            if (!hasTriggeredAdventure)
            {
                gameManager.randomEventManager.TriggerEvent("MeetSido", true);
                hasTriggeredFixedEvent = true;
            }
        }
        // 第五天4:00 校園熱門度太低觸發事件（觸發後強制）
        if (gameTime.day == 5 && gameTime.hours >= 4f && gameTime.hours < 4.1f)
        {
            if (gameManager.playerStats.popularity < 20f)
            {
                gameManager.randomEventManager.TriggerEvent("LowPopularityEvent", true);
                hasTriggeredFixedEvent = true;
            }
        }
        // 第五天21:00-22:00 買牛奶（強制）
        if (gameTime.day == 5 && gameTime.hours >= 21f && gameTime.hours < 22f)
        {
            gameManager.randomEventManager.TriggerEvent("BuyMilk", true);
            hasTriggeredFixedEvent = true;
        }
    }

    private void EndGame()
    {
        gameManager.EndGameWeek();
    }
}