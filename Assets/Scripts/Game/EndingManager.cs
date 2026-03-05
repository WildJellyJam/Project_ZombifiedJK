using UnityEngine;

public class EndingManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    public EndingSequencePlayer endingPlayerPrefabOrInScene;

    [Header("Ending Sequences")]
    public EndingSequence badEnding;
    public EndingSequence normalEnding;
    public EndingSequence goodEnding;

    [Header("Rules")]
    public int badEndingAnxietyThreshold = 100;
    public int normalEndingDay = 5;

    [Header("Good route flag (temporary)")]
    public bool goodRouteUnlocked = false;

    private bool _endingTriggered = false;

    [Header("DEBUG (replace with GameManagerJR values later)")]
    public int debugAnxiety = 0;
    public int debugCurrentDay = 1;

    void Update()
    {
        if (_endingTriggered) return;

        // 壞結局隨時監控
        if (GetAnxiety() >= badEndingAnxietyThreshold)
        {
            TriggerEnding(badEnding);
        }
    }

    public void OnDayEnded(int dayNumberJustEnded)
    {
        if (_endingTriggered) return;

        if (GetAnxiety() >= badEndingAnxietyThreshold)
        {
            TriggerEnding(badEnding);
            return;
        }

        if (dayNumberJustEnded >= normalEndingDay)
        {
            if (goodRouteUnlocked && goodEnding != null)
                TriggerEnding(goodEnding);
            else
                TriggerEnding(normalEnding);
        }
    }

    public void UnlockGoodRoute() => goodRouteUnlocked = true;

    // ======== 測試按鈕入口：直接給 UI OnClick 用 ========
    public void DebugTriggerBad() => TriggerEnding(badEnding);
    public void DebugTriggerNormal() => TriggerEnding(normalEnding);
    public void DebugTriggerGood() => TriggerEnding(goodEnding);

    private void TriggerEnding(EndingSequence seq)
    {
        if (_endingTriggered) return;

        if (seq == null)
        {
            Debug.LogError("[EndingManager] Ending sequence is NULL!");
            return;
        }

        var player = endingPlayerPrefabOrInScene;
        if (player == null)
        {
            Debug.LogError("[EndingManager] No EndingSequencePlayer assigned.");
            return;
        }

        // 如果你丟的是 prefab 才 instantiate（可留著，但我更推薦放場景內）
        if (!player.gameObject.scene.IsValid())
        {
            player = Instantiate(player);
        }

        // ✅ 最重要：要確定會開始播
        // 推薦 EndingSequencePlayer 實作 Play(seq)
        if (!player.gameObject.activeInHierarchy)
            player.gameObject.SetActive(true);

        player.Play(seq); // ← 你需要在 EndingSequencePlayer 做這個方法

        _endingTriggered = true;
        Debug.Log($"[EndingManager] Trigger Ending: {seq.endingId}");
    }

    private int GetAnxiety() => debugAnxiety;
    private int GetCurrentDay() => debugCurrentDay;
}