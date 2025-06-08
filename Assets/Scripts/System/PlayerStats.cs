using System.ComponentModel;
using UnityEditor.PackageManager;

[System.Serializable]


public enum NextAction
{
    none,
    goToMarket,
    goOut,
    goBackHome,
}

public enum SpecialEvent
{
    none,
    ParentsArgue,
    MeetSido, 
    LowPopularityEvent, 
}


public class PlayerStats
{
    public float sanity; // 理智值
    public float socialEnergy; // 社交能量
    public float popularity; // 校園熱門度
    public float anxiety; // 焦慮指數

    // public string nowScene;

    public NextAction nextAction = NextAction.none;
    public SpecialEvent specialEvent = SpecialEvent.none;
    private bool stayHome = true;

    public void Interact()
    {
        socialEnergy -= 5f; // 每次互動扣除社交能量
        if (socialEnergy < 0) socialEnergy = 0;
    }

    public void UpdateAnxiety(float delta)
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