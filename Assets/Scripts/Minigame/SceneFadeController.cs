using UnityEngine;

public class SceneFadeTrigger : MonoBehaviour
{
    [Header("要切換到的場景名稱")]
    public string targetSceneName = "1_atHomeBeforeSleep";  // 這裡你可以改成任何名字，比方說 a86 對應的場景

    // 給 Button OnClick / 其他腳本呼叫
    public void FadeToTargetScene()
    {
        if (SceneFader.Instance == null)
        {
            Debug.LogError("[SceneFadeTrigger] 找不到 SceneFader.Instance，確認場景裡有放 SceneFader 物件。");
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("[SceneFadeTrigger] targetSceneName 是空的，沒有要切換的場景名稱。");
            return;
        }

        SceneFader.Instance.FadeToScene(targetSceneName);
    }
}
