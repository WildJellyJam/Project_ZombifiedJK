using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManagerJr : MonoBehaviour
{
    [Header("Endings (ScriptableObjects)")]
    public EndingSequence badEnding;
    public EndingSequence normalEnding;
    public EndingSequence goodEnding;

    [Header("Rules")]
    public float badEndingAnxietyThreshold = 100f;
    public int endingDay = 5;

    [Header("Bad ending only triggers in this scene")]
    [Tooltip("填你的『場景2』的 Scene 名稱（Build Settings 裡的名字）")]
    public string scene2Name = "Scene2";

    [Header("Good route flag (先預留)")]
    public bool goodRouteUnlocked = false;

    [Header("Debug")]
    public bool debugLog = false;

    private bool _endingTriggered = false;

    private void Update()
    {
        if (_endingTriggered) return;

        // ✅ 只在 Scene2 監控壞結局
        if (SceneManager.GetActiveScene().name != scene2Name) return;

        float anxiety = GetAnxietySafe();
        if (anxiety >= badEndingAnxietyThreshold)
        {
            _endingTriggered = true;

            if (EndingFlow.Instance == null)
            {
                Debug.LogError("[EndingManagerJr] EndingFlow not found. Please place EndingFlow in your first scene.");
                return;
            }

            if (debugLog) Debug.Log($"[EndingManagerJr] Bad ending triggered in scene2. anxiety={anxiety}");
            EndingFlow.Instance.StartBadEndingFlow(badEnding);
        }
    }

    /// <summary>
    /// 在「一天結束」那個地方呼叫：第五天結束就進一般/好結局文字場景
    /// </summary>
    public void OnDayEnded(int dayJustEnded)
    {
        if (_endingTriggered) return;

        if (dayJustEnded < endingDay) return;

        _endingTriggered = true;

        if (EndingFlow.Instance == null)
        {
            Debug.LogError("[EndingManagerJr] EndingFlow not found. Please place EndingFlow in your first scene.");
            return;
        }

        var seq = (goodRouteUnlocked && goodEnding != null) ? goodEnding : normalEnding;
        if (debugLog) Debug.Log($"[EndingManagerJr] Day {dayJustEnded} ended -> go EndingTextScene: {seq.endingId}");

        EndingFlow.Instance.StartEndingText(seq);
    }

    public void UnlockGoodRoute()
    {
        goodRouteUnlocked = true; 
        if (debugLog) Debug.Log("[EndingManagerJr] Good route unlocked.");
    }

    private float GetAnxietySafe()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.playerStats == null)
            return 0f;

        return newGameManager.Instance.playerStats.anxiety;
    }
}