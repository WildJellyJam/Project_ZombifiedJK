using UnityEngine;

[System.Serializable]
public enum NextAction
{
    none,
    goToMarket,
    goOut,
    goBackHome,
}

[System.Serializable]
public enum SpecialEvent
{
    none,
    ParentsArgue,
    MeetSido,
    LowPopularityEvent,
}

[System.Serializable]
public class PlayerStats
{
    public float sanity;        // 理智值
    public float socialEnergy;  // 社交能量
    public float popularity;    // 校園熱門度
    public float anxiety;       // 焦慮指數
    public float randomEventTime = 0f;

    public NextAction nextAction = NextAction.none;
    public SpecialEvent specialEvent = SpecialEvent.none;
    private bool stayHome = true;

    public void Interact()
    {
        // 原本的互動：扣社交能量
        socialEnergy -= 5f;
        if (socialEnergy < 0) socialEnergy = 0;

        // 如果之後也想在互動時跳出字條，可以這樣：
        // if (StatChangeUI.Instance != null)
        // {
        //     StatChangeUI.Instance.ShowStatChange("社交能量", -5f);
        // }
    }

    public void UpdateAnxiety(float delta)
    {
        anxiety += delta;

        // ✅ 這一行會讓畫面出現「焦慮 +x / -x」的小字
        if (StatChangeUI.Instance != null)
        {
            StatChangeUI.Instance.ShowStatChange("焦慮", delta);
        }

        if (anxiety > 120f) TriggerBadEnding(); // 焦慮>120觸發壞結局
        if (anxiety < 0) anxiety = 0;
    }

    private void TriggerBadEnding()
    {
        // 觸發壞結局的處理
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
