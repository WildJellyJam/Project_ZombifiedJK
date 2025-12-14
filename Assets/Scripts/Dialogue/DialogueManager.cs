using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 元件")]
    [Tooltip("顯示台詞文字的 TextMeshProUGUI")]
    public TextMeshProUGUI dialogueText;

    [Tooltip("顯示說話者名字的 TextMeshProUGUI")]
    public TextMeshProUGUI speakerNameText;

    [Tooltip("左邊角色的 UI Image（角色 A，整個角色/立繪都是 UI）")]
    public Image leftPortrait;   // UI Image

    [Tooltip("右邊角色的 UI Image（角色 B）")]
    public Image rightPortrait;  // UI Image


    [Header("對話內容（在 Inspector 填台詞）")]
    public DialogueLine[] lines;

    [Header("角色 A 資料")]
    public string characterAName = "主角";

    [Tooltip("角色 A：預設中立頭像/立繪")]
    public Sprite characterAPortrait;

    [Tooltip("角色 A：開心")]
    public Sprite characterAHappy;

    [Tooltip("角色 A：難過")]
    public Sprite characterASad;

    [Tooltip("角色 A：生氣")]
    public Sprite characterAAngry;

    [Tooltip("角色 A：驚訝")]
    public Sprite characterASurprised;

    [Header("角色 B 資料")]
    public string characterBName = "？？？";

    [Tooltip("角色 B：預設中立頭像/立繪")]
    public Sprite characterBPortrait;

    [Tooltip("角色 B：開心")]
    public Sprite characterBHappy;

    [Tooltip("角色 B：難過")]
    public Sprite characterBSad;

    [Tooltip("角色 B：生氣")]
    public Sprite characterBAngry;

    [Tooltip("角色 B：驚訝")]
    public Sprite characterBSurprised;


    [Header("Portrait 狀態設定（誰在講話的視覺強度）")]
    [Tooltip("正在講話的頭像縮放")]
    public float activeScale = 1f;

    [Tooltip("沒有講話的頭像縮放")]
    public float inactiveScale = 0.8f;

    [Tooltip("正在講話的頭像顏色（通常白色、不透明）")]
    public Color activeColor = Color.white;

    [Tooltip("沒有講話的頭像顏色（可比較灰、比較透明）")]
    public Color inactiveColor = new Color(0.7f, 0.7f, 0.7f, 0.4f);

    [Tooltip("頭像亮/暗、大小切換的動畫時間（秒）")]
    public float portraitTransitionDuration = 0.25f;


    [Header("一般設定")]
    [Tooltip("場景一開始就自動播放對話")]
    public bool autoStartOnAwake = true;

    [Tooltip("允許用滑鼠左鍵 / 空白鍵 控制下一句")]
    public bool allowClickToNext = true;


    [Header("打字機設定")]
    [Tooltip("是否使用打字機效果")]
    public bool useTypewriter = true;

    [Tooltip("每秒顯示幾個字（越高越快）")]
    public float charsPerSecond = 30f;


    [Header("（可選）角色演出控制")]
    [Tooltip("如果要讓 3D/2D 角色隨對話動作，丟一個 Controller 進來；不需要可以留空")]
    public CharacterCutsceneController characterController;

    [HideInInspector]
    public Action onDialogueFinished; // 對話結束時通知外部（cutscene 結束、切場景用）

    private int currentIndex = -1;
    private bool isPlaying = false;

    // 打字機狀態
    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;

    // portrait 動畫用
    private Coroutine leftPortraitAnimCoroutine;
    private Coroutine rightPortraitAnimCoroutine;

    #region Unity 生命週期

    private void Awake()
    {
        if (autoStartOnAwake)
            StartDialogue();
    }

    private void Update()
    {
        if (!isPlaying || !allowClickToNext)
            return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (useTypewriter && isTyping)
            {
                // 打字中 → 先顯示完整台詞
                SkipTypewriter();
            }
            else
            {
                // 已經打完 → 播下一句
                NextLine();
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// 從第一句開始播放整段對話
    /// </summary>
    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueManager：lines 為空，沒有台詞可播放");
            return;
        }

        isPlaying = true;
        currentIndex = -1;

        // 一開始先給左右一張「中立表情」當底圖
        if (leftPortrait != null && characterAPortrait != null)
        {
            leftPortrait.sprite = characterAPortrait;
            leftPortrait.enabled = true;
        }
        if (rightPortrait != null && characterBPortrait != null)
        {
            rightPortrait.sprite = characterBPortrait;
            rightPortrait.enabled = true;
        }

        // 起始時都當成「沒在講話」狀態（直接設初始值，不需要動畫）
        if (leftPortrait != null)
        {
            leftPortrait.color = inactiveColor;
            leftPortrait.rectTransform.localScale = Vector3.one * inactiveScale;
        }

        if (rightPortrait != null)
        {
            rightPortrait.color = inactiveColor;
            rightPortrait.rectTransform.localScale = Vector3.one * inactiveScale;
        }

        NextLine();
    }

    /// <summary>
    /// 播下一句（可接 Button OnClick）
    /// </summary>
    public void NextLine()
    {
        if (!isPlaying)
            return;

        currentIndex++;

        if (currentIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = lines[currentIndex];

        // 先更新頭像 / 名字（誰在講話、用什麼表情）
        UpdateSpeakerUI(line);

        // 如果有角色演出控制，丟這行資訊給它（處理 moveTarget / animatorTriggerName）
        if (characterController != null)
            characterController.OnDialogueLine(line);

        // 觸發這一行對話的事件（開門、切鏡頭…）
        line.onLineStart?.Invoke();

        // 顯示文字（打字機 or 一次顯示）
        ShowLineText(line.text);
    }

    #endregion

    #region 打字機

    private void ShowLineText(string text)
    {
        if (dialogueText == null)
            return;

        // 停掉上一個打字 coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentFullText = text;

        if (!useTypewriter || string.IsNullOrEmpty(text))
        {
            dialogueText.text = text;
            isTyping = false;
            return;
        }

        typingCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }

    private System.Collections.IEnumerator TypeTextCoroutine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        float t = 0f;
        int charIndex = 0;
        int length = text.Length;

        while (charIndex < length)
        {
            t += Time.deltaTime * charsPerSecond;
            int newIndex = Mathf.Clamp(Mathf.FloorToInt(t), 0, length);

            if (newIndex != charIndex)
            {
                charIndex = newIndex;
                dialogueText.text = text.Substring(0, charIndex);
            }

            yield return null;
        }

        // 確保最後會顯示完整句子
        dialogueText.text = text;
        isTyping = false;
        typingCoroutine = null;
    }

    private void SkipTypewriter()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
            dialogueText.text = currentFullText;

        isTyping = false;
    }

    #endregion

    #region Portrait / 名字

    private void EndDialogue()
    {
        isPlaying = false;
        Debug.Log("DialogueManager：對話播放結束");
        onDialogueFinished?.Invoke();
    }

    private void UpdateSpeakerUI(DialogueLine line)
    {
        // 設定名字
        switch (line.speaker)
        {
            case Speaker.CharacterA:
                if (speakerNameText != null)
                    speakerNameText.text = characterAName;
                break;

            case Speaker.CharacterB:
                if (speakerNameText != null)
                    speakerNameText.text = characterBName;
                break;

            case Speaker.None:
            default:
                if (speakerNameText != null)
                    speakerNameText.text = "";
                break;
        }

        // 根據這一行的情緒選擇要顯示什麼 sprite
        if (line.speaker == Speaker.CharacterA && leftPortrait != null)
        {
            leftPortrait.sprite = GetPortraitSpriteFor(line, isCharacterA: true);
            leftPortrait.enabled = (leftPortrait.sprite != null);
        }
        else if (line.speaker == Speaker.CharacterB && rightPortrait != null)
        {
            rightPortrait.sprite = GetPortraitSpriteFor(line, isCharacterA: false);
            rightPortrait.enabled = (rightPortrait.sprite != null);
        }

        // 沒有講話那一邊，如果還沒有圖，就補一張 Default
        if (leftPortrait != null && leftPortrait.sprite == null && characterAPortrait != null)
        {
            leftPortrait.sprite = characterAPortrait;
            leftPortrait.enabled = true;
        }
        if (rightPortrait != null && rightPortrait.sprite == null && characterBPortrait != null)
        {
            rightPortrait.sprite = characterBPortrait;
            rightPortrait.enabled = true;
        }

        // 根據誰在講話來決定「目標狀態」，實際過渡由 SetPortraitState 動畫完成
        switch (line.speaker)
        {
            case Speaker.CharacterA:
                SetPortraitState(leftPortrait, true);   // A：亮 + 大（有動畫）
                SetPortraitState(rightPortrait, false);  // B：暗 + 小（有動畫）
                break;

            case Speaker.CharacterB:
                SetPortraitState(leftPortrait, false);
                SetPortraitState(rightPortrait, true);
                break;

            case Speaker.None:
            default:
                SetPortraitState(leftPortrait, false);
                SetPortraitState(rightPortrait, false);
                break;
        }
    }

    /// <summary>
    /// 根據這一行的 mood 決定要用哪一張 Sprite（最後是塞進 UI Image 裡）
    /// </summary>
    private Sprite GetPortraitSpriteFor(DialogueLine line, bool isCharacterA)
    {
        Sprite defaultSprite = isCharacterA ? characterAPortrait : characterBPortrait;

        if (line.mood == PortraitMood.CustomSprite && line.customPortraitSprite != null)
        {
            return line.customPortraitSprite;
        }

        switch (line.mood)
        {
            case PortraitMood.Happy:
                return isCharacterA
                    ? (characterAHappy ?? defaultSprite)
                    : (characterBHappy ?? defaultSprite);

            case PortraitMood.Sad:
                return isCharacterA
                    ? (characterASad ?? defaultSprite)
                    : (characterBSad ?? defaultSprite);

            case PortraitMood.Angry:
                return isCharacterA
                    ? (characterAAngry ?? defaultSprite)
                    : (characterBAngry ?? defaultSprite);

            case PortraitMood.Surprised:
                return isCharacterA
                    ? (characterASurprised ?? defaultSprite)
                    : (characterBSurprised ?? defaultSprite);

            case PortraitMood.Default:
            default:
                return defaultSprite;
        }
    }

    /// <summary>
    /// 控制 UI Image 的顏色 + 縮放，使用動畫過渡，而不是瞬間跳
    /// </summary>
    private void SetPortraitState(Image img, bool isActiveSpeaker)
    {
        if (img == null)
            return;

        Color targetColor = isActiveSpeaker ? activeColor : inactiveColor;
        float targetScale = isActiveSpeaker ? activeScale : inactiveScale;

        // 決定要用哪個 coroutine 變數
        if (img == leftPortrait)
        {
            if (leftPortraitAnimCoroutine != null)
                StopCoroutine(leftPortraitAnimCoroutine);

            leftPortraitAnimCoroutine = StartCoroutine(
                AnimatePortrait(img, targetColor, targetScale)
            );
        }
        else if (img == rightPortrait)
        {
            if (rightPortraitAnimCoroutine != null)
                StopCoroutine(rightPortraitAnimCoroutine);

            rightPortraitAnimCoroutine = StartCoroutine(
                AnimatePortrait(img, targetColor, targetScale)
            );
        }
        else
        {
            // 其他 Image（其實目前不會用到，但以防萬一）
            StartCoroutine(AnimatePortrait(img, targetColor, targetScale));
        }
    }

    /// <summary>
    /// 逐幀 Lerp 顏色與縮放，做出平滑過場效果
    /// </summary>
    private System.Collections.IEnumerator AnimatePortrait(Image img, Color targetColor, float targetScale)
    {
        if (img == null)
            yield break;

        Color startColor = img.color;
        Vector3 startScale = img.rectTransform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

        float duration = Mathf.Max(portraitTransitionDuration, 0.01f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float lerp = Mathf.Clamp01(t);

            img.color = Color.Lerp(startColor, targetColor, lerp);
            img.rectTransform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            yield return null;
        }

        // 最後確保完全到達目標值
        img.color = targetColor;
        img.rectTransform.localScale = endScale;
    }

    #endregion
}
