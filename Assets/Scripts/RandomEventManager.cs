using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    private List<string> triggeredEvents = new List<string>();

    public void TriggerRandomEvent(PlayerStats playerStats)
    {
        int randomCode = UnityEngine.Random.Range(0, 100);
        if (randomCode <= 30) // 低難度
        {
            playerStats.UpdateAnxiety(5f); // 增加少量焦慮
            triggeredEvents.Add("LowDifficultyEvent");
        }
        else if (randomCode <= 60) // 中難度
        {
            playerStats.UpdateAnxiety(10f);
            triggeredEvents.Add("MediumDifficultyEvent");
        }
        else if (randomCode <= 90) // 高難度
        {
            playerStats.UpdateAnxiety(20f);
            triggeredEvents.Add("HighDifficultyEvent");
        }
        else // 特殊事件
        {
            triggeredEvents.Add("SpecialEvent");
        }
    }

    public List<string> GetTriggeredEvents()
    {
        return triggeredEvents;
    }
}