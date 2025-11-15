using System.ComponentModel;
using UnityEngine;
// using UnityEditor.PackageManager;

[System.Serializable]


public enum NextActionJr
{
    none,
    goToMarket,
    goOut,
    goBackHome,
}

public enum SpecialEventJr
{
    none,
    ParentsArgue,
    MeetSido,
    LowPopularityEvent,
}


public class PlayerStatsJr : MonoBehaviour
{
    public int sanity; // 理智值
    public int socialEnergy; // 社交能量
    public int popularity; // 校園熱門度
    public int anxiety; // 焦慮指數
    public int randomEventTime = 0;

    // public string nowScene;

    public NextAction nextAction = NextAction.none;
    public SpecialEvent specialEvent = SpecialEvent.none;
    private bool stayHome = true;

    public void Interact()
    {
        socialEnergy -= 5; // 每次互動扣除社交能量
        if (socialEnergy < 0) socialEnergy = 0;
    }

    public void UpdateAnxiety(int delta)
    {
        anxiety += delta;
        if (anxiety > 120f) TriggerBadEnding(); // 焦慮>120觸發壞結局
        if (anxiety < 0) anxiety = 0;
    }

    private void TriggerBadEnding()
    {
        // 觸發壞結局
    }
    public void updateDailyState()
    {
        switch (newGameManager.Instance.playerStats.nextAction)
        {
            case NextAction.goOut:
                stayHome = false;
                break;
            case NextAction.goToMarket:
                stayHome = false;
                break;
            default:
                break;
        }
    }
    public void resetDailyState()
    {
        stayHome = true;
    }
    public bool getDailyState()
    {
        return stayHome;
    }
}