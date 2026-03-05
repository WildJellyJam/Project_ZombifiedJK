using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// EndingSequencePlayer
/// - 由 EndingManager 呼叫 Play(seq) 開始播放
/// - 支援：整體淡入淡出、圖片切換淡入淡出、打字機、台詞節點音效
/// </summary>
public class EndingSequencePlayer : MonoBehaviour
{
    [Header("UI Root")]
    [Tooltip("整個結局 UI 的 CanvasGroup，用來做全畫面淡入淡出")]
    public CanvasGroup rootGroup;

    [Header("Text")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    [Header("Image (Crossfade)")]
    [Tooltip("用兩張 Image 做交叉淡入淡出：A -> B -> A -> ...")]
    public Image imageA;
    public Image imageB;

    [Header("Audio")]
    public AudioSource sfxSource;

    [Header("Behavior")]
    public bool playOnStart = false; // 若你想進 EndingScene 自動播，可打勾
    public bool skipTypingWithClick = true;
    public bool clickToContinue = true;

    [Header("Timings")]
    public float sceneFadeInSeconds = 0.6f;
    public float sceneFadeOutSeconds = 0.6f;
    public float imageFadeSeconds = 0.35f;
    public float charsPerSecond = 40f;
    public float autoNextDelay = 0.2f; // 每句播完後的短暫停頓

    [Header("After Finish")]
    public bool returnToSceneAfterFinish = false;
    public string returnSceneName = "MainMenu";

    [Header("Runtime (set by EndingManager)")]
    public EndingSequence sequence;

    // ---- internal state ----
    Coroutine _mainRoutine;
    bool _isTyping;
    bool _skipTypingRequested;
    bool _continueRequested;
    bool _usingA = true;

    private void Awake()
    {
        // 基本保護：rootGroup 沒填就自己抓
        if (rootGroup == null) rootGroup = GetComponentInChildren<CanvasGroup>(true);

        // 預設先黑/隱藏，等 Play() 再淡入（避免一進場閃一下）
        if (rootGroup != null)
        {
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }

        // 圖層初始
        SetupImageInitialState();
    }

    private void Start()
    {
        if (playOnStart && sequence != null)
        {
            Play(sequence);
        }
    }

    private void Update()
    {
        if (!clickToContinue && !skipTypingWithClick) return;

        // 用滑鼠左鍵 / 任意點擊（你也可以改成按鍵）
        if (Input.GetMouseButtonDown(0))
        {
            if (_isTyping && skipTypingWithClick)
            {
                _skipTypingRequested = true;
            }
            else if (clickToContinue)
            {
                _continueRequested = true;
            }
        }
    }

    /// <summary>
    /// 給 EndingManager 呼叫：開始播放指定結局序列
    /// </summary>
    public void Play(EndingSequence seq)
    {
        if (seq == null)
        {
            Debug.LogError("[EndingSequencePlayer] Play() got NULL sequence!");
            return;
        }

        sequence = seq;

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // 停掉舊流程
        if (_mainRoutine != null) StopCoroutine(_mainRoutine);
        _mainRoutine = StartCoroutine(PlaySequenceRoutine(seq));
    }

    /// <summary>
    /// 可選：外部要強制停止
    /// </summary>
    public void Stop()
    {
        if (_mainRoutine != null) StopCoroutine(_mainRoutine);
        _mainRoutine = null;
    }

    private IEnumerator PlaySequenceRoutine(EndingSequence seq)
    {
        // 解鎖/接管 UI
        if (rootGroup != null)
        {
            rootGroup.interactable = true;
            rootGroup.blocksRaycasts = true;
        }

        // 進場淡入
        yield return FadeRoot(0f, 1f, sceneFadeInSeconds);

        // ---- 取得 steps ----
        // ⚠️ 這裡假設你的 EndingSequence 有 steps (List<EndingStep>) 或 lines。
        // 如果你的欄位名稱不同，請看最下方「你要改的地方」。
        IList<EndingStep> steps = GetStepsFromSequence(seq);

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning($"[EndingSequencePlayer] Sequence has no steps: {seq.endingId}");
            yield return FadeRoot(1f, 0f, sceneFadeOutSeconds);
            yield break;
        }

        // 清 UI
        SetSpeaker("");
        SetDialogue("");
        SetupImageInitialState();

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];

            // 1) speaker
            SetSpeaker(step.speaker);

            // 2) image (如有指定就換圖並淡入淡出)
            if (step.image != null)
            {
                yield return CrossFadeTo(step.image, imageFadeSeconds);
            }

            // 3) 可選：進句前音效（例如喘氣、心跳）
            if (step.sfx != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(step.sfx);
            }

            // 4) 打字機
            yield return TypeLine(step.text);

            // 5) 每句結束後：等點擊繼續 或 自動下一句
            yield return new WaitForSeconds(autoNextDelay);

            if (step.waitForClick)
            {
                yield return WaitForContinue();
            }
            else if (step.extraHoldSeconds > 0f)
            {
                yield return new WaitForSeconds(step.extraHoldSeconds);
            }
        }

        // 結束淡出
        yield return FadeRoot(1f, 0f, sceneFadeOutSeconds);

        // 回場景（可選）
        if (returnToSceneAfterFinish && !string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName);
        }
    }

    // ---------------- UI Helpers ----------------

    private void SetSpeaker(string s)
    {
        if (speakerText != null) speakerText.text = string.IsNullOrEmpty(s) ? "" : s;
    }

    private void SetDialogue(string s)
    {
        if (dialogueText != null) dialogueText.text = s ?? "";
    }

    private IEnumerator FadeRoot(float from, float to, float seconds)
    {
        if (rootGroup == null)
            yield break;

        if (seconds <= 0f)
        {
            rootGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        rootGroup.alpha = from;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            rootGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        rootGroup.alpha = to;
    }

    private void SetupImageInitialState()
    {
        _usingA = true;

        if (imageA != null)
        {
            var c = imageA.color; c.a = 1f; imageA.color = c;
            imageA.sprite = null;
            imageA.enabled = false;
        }

        if (imageB != null)
        {
            var c = imageB.color; c.a = 0f; imageB.color = c;
            imageB.sprite = null;
            imageB.enabled = false;
        }
    }

    private IEnumerator CrossFadeTo(Sprite newSprite, float seconds)
    {
        if (imageA == null || imageB == null)
        {
            // 沒有雙 Image 就直接塞
            if (imageA != null)
            {
                imageA.sprite = newSprite;
                imageA.enabled = (newSprite != null);
            }
            yield break;
        }

        Image from = _usingA ? imageA : imageB;
        Image to = _usingA ? imageB : imageA;

        // set target sprite
        to.sprite = newSprite;
        to.enabled = (newSprite != null);

        // 起始 alpha
        SetAlpha(to, 0f);
        SetAlpha(from, from.enabled ? 1f : 0f);

        if (seconds <= 0f)
        {
            SetAlpha(to, 1f);
            SetAlpha(from, 0f);
            from.enabled = false;
            _usingA = !_usingA;
            yield break;
        }

        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);

            SetAlpha(to, k);
            SetAlpha(from, 1f - k);

            yield return null;
        }

        SetAlpha(to, 1f);
        SetAlpha(from, 0f);
        from.enabled = false;

        _usingA = !_usingA;
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    private IEnumerator TypeLine(string fullText)
    {
        _isTyping = true;
        _skipTypingRequested = false;

        fullText = fullText ?? "";

        if (charsPerSecond <= 0f)
        {
            SetDialogue(fullText);
            _isTyping = false;
            yield break;
        }

        SetDialogue("");

        float secondsPerChar = 1f / charsPerSecond;
        int len = fullText.Length;

        for (int i = 0; i < len; i++)
        {
            if (_skipTypingRequested)
            {
                SetDialogue(fullText);
                break;
            }

            SetDialogue(fullText.Substring(0, i + 1));
            yield return new WaitForSecondsRealtime(secondsPerChar);
        }

        _isTyping = false;
    }

    private IEnumerator WaitForContinue()
    {
        _continueRequested = false;
        while (!_continueRequested)
            yield return null;
    }

    // ---------------- Sequence Data Adapter ----------------
    // 你很可能已經有自己的 EndingSequence/Line 結構。
    // 這裡我提供一個「最常見」的 EndingStep 結構，並用 GetStepsFromSequence 做轉接。
    // 如果你 EndingSequence 已經有 steps/lines，直接對應回來就好。

    [System.Serializable]
    public class EndingStep
    {
        public string speaker;
        [TextArea(2, 6)] public string text;
        public Sprite image;

        [Tooltip("在這句開始時播一次的音效（可留空）")]
        public AudioClip sfx;

        [Tooltip("這句播完後是否要等玩家點擊才進下一句")]
        public bool waitForClick = true;

        [Tooltip("不等點擊時，額外停留秒數（例如想讓畫面多停一下）")]
        public float extraHoldSeconds = 0f;
    }

    /// <summary>
    /// ✅ 你最需要改的地方在這裡：
    /// 讓它回傳你的 EndingSequence 裡真正的台詞列表
    /// </summary>
    private IList<EndingStep> GetStepsFromSequence(EndingSequence seq)
    {
        // ====== 情況 1：如果你 EndingSequence 本身就有 List<EndingStep> steps ======
        // 取消下面註解，並把你的欄位名稱填對
        //
        // return seq.steps;

        // ====== 情況 2：你 EndingSequence 裡叫 lines，而且每個 line 欄位類似 speaker/text/image/sfx ======
        // 你可以在這裡把它轉成 EndingStep list
        //
        // var list = new List<EndingStep>();
        // foreach (var l in seq.lines)
        // {
        //     list.Add(new EndingStep {
        //         speaker = l.speaker,
        //         text = l.content,
        //         image = l.sprite,
        //         sfx = l.sfx,
        //         waitForClick = l.waitForClick
        //     });
        // }
        // return list;

        // ====== 暫時：如果你還沒接資料，就回 null，讓你看得到 warning ======
        return null;
    }
}