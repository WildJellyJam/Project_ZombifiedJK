using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// 1. 基礎對話資料
[System.Serializable]
public class GameChatData
{
    public string speaker;
    [TextArea(2, 5)]
    public string content;
}

// 2. 繼承 MinigameBase，讓框架能識別它
public class ChatMinigame : MinigameBase
{
    [Header("UI 組件連結")]
    public Transform chatContent;
    public GameObject chatBubblePrefab;
    public TMP_InputField playerInputField;
    public TextMeshProUGUI countText;

    [Header("遊戲設定")]
    public List<GameChatData> chatList;
    public float chatSpeed = 2.0f;
    public int goalCharCount = 50;

    private bool _isPaused = false;
    private bool _isFinished = false;
    private int _currentIndex = 0;

    private void Start()
    {
        // 初始 UI
        UpdateCountUI(0);

        // 監聽輸入框狀態
        playerInputField.onSelect.AddListener(x => _isPaused = true);
        playerInputField.onDeselect.AddListener(x => _isPaused = false);

        // 監聽文字變動
        playerInputField.onValueChanged.AddListener(CheckCharacterCount);

        // 啟動自動對話
        StartCoroutine(ChatRoutine());
    }

    IEnumerator ChatRoutine()
    {
        // 只要還沒結束，就持續循環顯示對話
        while (!_isFinished)
        {
            // 如果清單播完了，就重新從頭開始 (或者是停在最後一句)
            if (_currentIndex >= chatList.Count) _currentIndex = 0;

            yield return new WaitUntil(() => !_isPaused);

            CreateBubble(chatList[_currentIndex]);
            _currentIndex++;

            yield return new WaitForSeconds(chatSpeed);
        }
    }

    void CreateBubble(GameChatData data)
    {
        if (chatContent == null || chatBubblePrefab == null) return;

        GameObject bubble = Instantiate(chatBubblePrefab, chatContent);
        var textComp = bubble.GetComponentInChildren<TextMeshProUGUI>();

        if (textComp != null)
        {
            textComp.text = $"<color=#00FF00>{data.speaker}:</color> {data.content}";
        }

        // 自動捲動到底部
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = chatContent.parent.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
    }

    void CheckCharacterCount(string input)
    {
        if (_isFinished) return;

        int currentCount = input.Length;
        UpdateCountUI(currentCount);

        if (currentCount >= goalCharCount)
        {
            FinishGame(true); // 觸發勝利
        }
    }

    void UpdateCountUI(int current)
    {
        if (countText != null)
            countText.text = $"輸入進度: {current} / {goalCharCount}";
    }

    // 關鍵：與框架對接
    public void FinishGame(bool success)
    {
        if (_isFinished) return;
        _isFinished = true;

        playerInputField.interactable = false;

        // 3. 呼叫框架要求的事件，通知 MinigameManagerJr 小遊戲結束了
        // 傳入 true 會播放 winVideo，false 會播放 loseVideo
        EndMinigame(success);

        Debug.Log(success ? "小遊戲勝利！" : "小遊戲失敗！");
    }
}