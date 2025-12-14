using UnityEngine;

public class AnxietyButtonController : MonoBehaviour
{
    // 焦慮 +10
    public void AddAnxiety10()
    {
        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(10f);
        }
        else
        {
            Debug.LogWarning("newGameManager 或 playerStats 沒有設定，無法增加焦慮。");
        }
    }

    // 焦慮 -10
    public void MinusAnxiety10()
    {
        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
        {
            newGameManager.Instance.playerStats.UpdateAnxiety(-10f);
        }
        else
        {
            Debug.LogWarning("newGameManager 或 playerStats 沒有設定，無法降低焦慮。");
        }
    }
}
