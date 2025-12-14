using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 掛在單一一個角色身上的 cutscene 控制器。
/// DialogueManager 在每行對話時會呼叫 OnDialogueLine。
/// </summary>
public class CharacterCutsceneController : MonoBehaviour
{
    [Header("這個 Controller 控制的是哪一個角色？")]
    [Tooltip("若為 true，代表這個物件是 CharacterA；false 則是 CharacterB")]
    public bool isCharacterA = true;

    [Header("Animator 設定")]
    public Animator animator;

    [Tooltip("移動到目標點所需時間（未使用 NavMesh 時）")]
    public float moveDuration = 1.5f;

    [Header("NavMesh（可選）")]
    [Tooltip("如果使用 NavMeshAgent 做位移就勾這個")]
    public bool useNavMeshAgent = false;
    public NavMeshAgent agent;

    private Coroutine moveCoroutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (useNavMeshAgent && agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 由 DialogueManager 在每一行對話開始時呼叫。
    /// </summary>
    public void OnDialogueLine(DialogueLine line)
    {
        if (line == null) return;

        // 只處理「屬於自己」的台詞
        if (isCharacterA && line.speaker != Speaker.CharacterA) return;
        if (!isCharacterA && line.speaker != Speaker.CharacterB) return;

        // 1) 播動畫 Trigger（若有）
        if (animator != null && !string.IsNullOrEmpty(line.animatorTriggerName))
        {
            animator.SetTrigger(line.animatorTriggerName);
        }

        // 2) 移動到目標位置（若有）
        if (line.moveTarget != null)
        {
            if (useNavMeshAgent && agent != null)
            {
                MoveWithNavMesh(line.moveTarget.position);
            }
            else
            {
                MoveWithLerp(line.moveTarget.position);
            }
        }
    }

    #region 移動相關

    private void MoveWithNavMesh(Vector3 targetPos)
    {
        if (agent == null) return;

        agent.isStopped = false;
        agent.SetDestination(targetPos);
    }

    private void MoveWithLerp(Vector3 targetPos)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveOverTime(targetPos, moveDuration));
    }

    private System.Collections.IEnumerator MoveOverTime(Vector3 targetPos, float duration)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(duration, 0.01f);
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        moveCoroutine = null;
    }

    #endregion
}
