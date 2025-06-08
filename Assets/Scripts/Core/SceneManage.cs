using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneManage
{
    // private bool isLoading = false;
    public GameObject cutScenePanel;
    public TextMeshProUGUI cutScenePanelTextUI;

    private Dictionary<TimePeriod, string> cutScenePanelMessage = new Dictionary<TimePeriod, string>
    {
        [TimePeriod.AtHomeAfterWakeUp]   = "sleep...",
        [TimePeriod.AtHomeBeforeLeaving] = "prepare to school, want stay or go?",
        [TimePeriod.AtHomeBeforeSleep]   = "finaly go home, i want to sleep...",
        [TimePeriod.AtSchool]            = "",
        [TimePeriod.AtSchoolAfterClass]  = "", 
        [TimePeriod.AtSupermarket]       = "mom let u got o shop buy milk",
        [TimePeriod.AtHome]              = "stay at home",
    };

    private void UpdateTimePeriod()
    {
        var time = newGameManager.Instance.timeSystem.gameTime;
        bool isWeekend = time.day >= 5;
        if (isWeekend)
        {
            switch (newGameManager.Instance.playerStats.nextAction)
            {
                case NextAction.goOut:
                    if (time.hours >= 6f && time.hours < 9f) time.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                    else if (time.hours >= 9f && time.hours < 12f) time.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                    else if (time.hours >= 12f && time.hours < 15f) time.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                    else if (time.hours >= 15f && time.hours < 18f) time.currentPeriod = TimePeriod.InCity;
                    else time.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                    break;
                case NextAction.goToMarket:
                    time.currentPeriod = TimePeriod.AtSupermarket;
                    break;
            }
        }
        else
        {
            switch (newGameManager.Instance.playerStats.nextAction)
            {
                case NextAction.goOut:
                    if (time.hours >= 16f && time.hours < 16.5f) time.currentPeriod = TimePeriod.AtSchoolAfterClass;
                    else if (time.hours >= 16.5f && time.hours < 21f) time.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                    else if (time.hours >= 21f || time.hours < 6f) time.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                    else if (time.hours >= 6f && time.hours < 7f) time.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                    else if (time.hours >= 7.5f && time.hours < 16f) time.currentPeriod = TimePeriod.AtSchool;
                    else time.currentPeriod = TimePeriod.AtHomeAfterWakeUp; // 通勤時間過渡
                    break;
                case NextAction.goToMarket:
                    time.currentPeriod = TimePeriod.AtSupermarket;
                    break;
                case NextAction.goBackHome:
                    time.currentPeriod = TimePeriod.AtHome;
                    break;
                default:
                    if (time.hours >= 16f && time.hours < 16.5f) time.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                    else if (time.hours >= 16.5f && time.hours < 21f) time.currentPeriod = TimePeriod.AtHomeBeforeSleep;
                    else if (time.hours >= 21f || time.hours < 6f) time.currentPeriod = TimePeriod.AtHomeAfterWakeUp;
                    else if (time.hours > 6f && time.hours <= 7f) time.currentPeriod = TimePeriod.AtHomeBeforeLeaving;
                    else time.currentPeriod = TimePeriod.AtHome;
                    break;
            }
        }
    }

    public void SwitchScene()
    {
        var period = newGameManager.Instance.timeSystem.gameTime.currentPeriod;
        switch (newGameManager.Instance.playerStats.specialEvent)
        {
            case SpecialEvent.ParentsArgue:
                period = TimePeriod.AtAdventure;
                break;
            case SpecialEvent.MeetSido:
                period = TimePeriod.AtAdventure;
                break;
            case SpecialEvent.LowPopularityEvent:
                period = TimePeriod.AtSchoolAfterClass;
                break;
            default:
                UpdateTimePeriod();
                break;
        }
        period = newGameManager.Instance.timeSystem.gameTime.currentPeriod;
        string sceneName = period switch
        {
            TimePeriod.AtSchoolAfterClass => "0_atSchoolAfterClass",
            TimePeriod.AtHomeBeforeSleep => "1_atHomeBeforeSleep",
            TimePeriod.AtHomeAfterWakeUp => "2_atHomeAfterWakeUp",
            TimePeriod.AtHomeBeforeLeaving => "3_atHomeBeforeLeaving",
            TimePeriod.AtSchool => "4_atSchool",
            TimePeriod.AtHome => "5_atHome",
            TimePeriod.InCity => "6_inCity",
            TimePeriod.AtSupermarket => "7_atSupermarket",
            TimePeriod.AtHomeParentsArgue => "8_atHomeParentsArgue",
            TimePeriod.AtAdventure => "9_atAdventure",
            _ => "1_atHomeBeforeSleep"
        };
        // 檢查當前場景是否與目標場景相同，避免重複加載
        Debug.Log("in switch scene");
        Debug.Log(sceneName);
        Debug.Log(period);
        if (sceneName == "4_atSchool")
        {
            newGameManager.Instance.playerStats.updateDailyState();
        }
        else if (sceneName == "2_atHomeAfterWakeUp")
        {
            newGameManager.Instance.playerStats.resetDailyState();
        }
        if (SceneManager.GetActiveScene().name != sceneName) // && !isLoading)
        {
            SceneManager.LoadScene(sceneName);

        }
    }

    public void ReturnToMainMenuScene()
    {
        SceneManager.LoadSceneAsync("StartUpMenu");
    }
    
}
