using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MinigameOption
{
    public float anxietyThreshold;         // 這個焦慮值以上會選到這個遊戲
    public GameObject minigamePrefab;      // 要實例化的 minigame prefab
}

public class MinigameManagerJr : MonoBehaviour
{
    [Header("Minigame Settings")]
    public int minigamesToPlay = 3;       // 要連續玩幾個小遊戲
    private int currentMinigameCount = 0;

    [Header("Animation")]
    public Animator transitionAnimator;   // Intro / Win / Lose 動畫

    [Header("Minigame Options")]
    public List<MinigameOption> minigameOptions = new List<MinigameOption>();

    private GameObject activeMinigame;

    // ✅ 外部呼叫用 (Trigger、劇情等等)
    public void StartMinigameSequence()
    {
        currentMinigameCount = 0;
        StartCoroutine(PlayMinigameFlow());
    }

    // ✅ 如果你之前用 StartMinigameFlow，不想改其他地方，可以留著：
    public void StartMinigameFlow()
    {
        StartMinigameSequence();
    }

    // --- 主要流程：動畫 → minigame → 結果 → 下一個 ---
    private IEnumerator PlayMinigameFlow()
    {
        while (currentMinigameCount < minigamesToPlay)
        {
            // 1. 播放開場動畫
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("StartMinigame");
                yield return new WaitForSeconds(1f);
            }

            // 2. 根據焦慮值選 minigame prefab
            GameObject selectedMinigame = SelectMinigameBasedOnAnxiety();
            if (selectedMinigame == null)
            {
                Debug.LogWarning("⚠ 沒有符合焦慮值的 minigame！");
                yield break;
            }

            // 3. 實例化小遊戲
            activeMinigame = Instantiate(selectedMinigame);
            MinigameBase minigame = activeMinigame.GetComponent<MinigameBase>();

            bool? result = null; // true=win, false=lose
            minigame.OnMinigameEnd += (bool won) => { result = won; };

            // 等 minigame 呼叫 EndMinigame()
            yield return new WaitUntil(() => result.HasValue);

            // 4. 播放勝利 or 失敗動畫
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger(result.Value ? "Win" : "Lose");
                yield return new WaitForSeconds(1f);
            }

            // 5. 清除小遊戲
            Destroy(activeMinigame);
            activeMinigame = null;
            currentMinigameCount++;
        }

        // ✅ 全部 minigame 結束後才統一處理焦慮值 & 換場景
        FinishAllMinigames();
    }

    // --- 全部 minigame 結束後要做什麼 ---
    private void FinishAllMinigames()
    {
        Debug.Log($"✅ {minigamesToPlay} 個小遊戲結束！");

        // 👉 這裡你可以決定加或減焦慮值
        GameManager.Instance.AddAnxiety(-10);  // 贏得多 = 放鬆
        // GameManager.Instance.AddAnxiety(+10); // 或者失敗多 = 更焦慮

        // 👉 然後載入下一個場景或返回劇情
        // SceneManager.LoadScene("NextSceneName");
    }

    // --- 根據焦慮值決定 spawn 哪個 minigame ---
    private GameObject SelectMinigameBasedOnAnxiety()
    {
        float anxiety = GameManager.Instance.anxiety;
        GameObject chosen = null;
        float bestMatch = -1;

        foreach (var option in minigameOptions)
        {
            if (anxiety >= option.anxietyThreshold && option.anxietyThreshold > bestMatch)
            {
                bestMatch = option.anxietyThreshold;
                chosen = option.minigamePrefab;
            }
        }
        return chosen;
    }
}
