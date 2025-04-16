using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TimePeriod
{
    MorningHome, MorningSchool, NoonSchool, AfternoonSchool, EveningHome,
    WeekendMorningHome, WeekendDayOutside, WeekendEveningHome, WeekendDayHome
}

[System.Serializable]
public class GameTime
{
    public int day; // 第幾天（1-7）
    public float hours; // 當前小時（0-24）
    public TimePeriod currentPeriod; // 當前時間段
}

public class TimeSystem : MonoBehaviour
{
    public GameTime gameTime = new GameTime { day = 1, hours = 6f, currentPeriod = TimePeriod.MorningHome };
    public float timeSpeed = 1f; // 時間流逝速度

    public void UpdateTime(float deltaTime)
    {
        gameTime.hours += deltaTime * timeSpeed;
        if (gameTime.hours >= 24f)
        {
            gameTime.hours -= 24f;
            gameTime.day++;
            if (gameTime.day > 7) EndGame(); // 第7天結束
        }
        UpdateTimePeriod();
    }

    public void AddEventTime(float eventHours)
    {
        gameTime.hours += eventHours;
        if (gameTime.hours >= 24f)
        {
            gameTime.hours -= 24f;
            gameTime.day++;
            if (gameTime.day > 7) EndGame();
        }
        UpdateTimePeriod();
    }

    private void UpdateTimePeriod()
    {
        bool isWeekend = gameTime.day >= 6;
        bool goOut = true; // 假設玩家選擇出門，需根據玩家選擇動態設定
        if (isWeekend)
        {
            if (goOut)
            {
                if (gameTime.hours >= 6f && gameTime.hours < 9f) gameTime.currentPeriod = TimePeriod.WeekendMorningHome;
                else if (gameTime.hours >= 9f && gameTime.hours < 18f) gameTime.currentPeriod = TimePeriod.WeekendDayOutside;
                else gameTime.currentPeriod = TimePeriod.WeekendEveningHome;
            }
            else
            {
                gameTime.currentPeriod = TimePeriod.WeekendDayHome;
            }
        }
        else
        {
            if (goOut)
            {
                if (gameTime.hours >= 6f && gameTime.hours < 7f) gameTime.currentPeriod = TimePeriod.MorningHome;
                else if (gameTime.hours >= 8f && gameTime.hours < 12f) gameTime.currentPeriod = TimePeriod.MorningSchool;
                else if (gameTime.hours >= 12f && gameTime.hours < 13f) gameTime.currentPeriod = TimePeriod.NoonSchool;
                else if (gameTime.hours >= 13f && gameTime.hours < 18f) gameTime.currentPeriod = TimePeriod.AfternoonSchool;
                else gameTime.currentPeriod = TimePeriod.EveningHome;
            }
            else
            {
                gameTime.currentPeriod = TimePeriod.MorningHome; // 整天在家
            }
        }
    }

    private void EndGame()
    {
        // 觸發結局，檢查壓力值
    }
}