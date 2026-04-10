using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class gameUIManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Button returnToMenuButton;

    public bool isPaused = false;
    public RectTransform panel;
    public GameObject cutScenePanel;
    public TextMeshProUGUI cutScenePanelTextUI;
    public RectTransform shopBuyMilkPanel;
    public TextMeshProUGUI messageText;
    public RectTransform messageBox;
    public float showDuration = 3f;
    public float slideSpeed = 500f;

    [Header("測試模式")]
    public bool useTestDefaultWhenManagerMissing = true;
    public TimePeriod testDefaultPeriod = TimePeriod.AtHomeAfterWakeUp;
    public bool testDailyState = false;

    private Vector2 hiddenPos;
    private Vector2 visiblePos;
    private Coroutine slideCoroutine;

    private Dictionary<TimePeriod, string> cutScenePanelMessage = new Dictionary<TimePeriod, string>
    {
        [TimePeriod.AtHomeAfterWakeUp] = "sleep...",
        [TimePeriod.AtHomeBeforeLeaving] = "prepare to school, want stay or go?",
        [TimePeriod.AtHomeBeforeSleep] = "finaly go home, i want to sleep...",
        [TimePeriod.AtSchool] = "",
        [TimePeriod.AtSchoolAfterClass] = "",
        [TimePeriod.AtSupermarket] = "mom let u got o shop buy milk",
        [TimePeriod.AtHome] = "stay at home",
    };

    void OnEnable()
    {
        if (newGameManager.Instance != null)
            newGameManager.Instance.ReturnToMainMenuEvent += ReturnToMainMenu;
    }

    void OnDisable()
    {
        if (newGameManager.Instance != null)
            newGameManager.Instance.ReturnToMainMenuEvent -= ReturnToMainMenu;
    }

    void Start()
    {
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);
        else
            Debug.LogError("continueButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'continueBtn'！");

        Debug.Log("start UIManager");

        hiddenPos = new Vector2(-150, 100);
        visiblePos = new Vector2(-150, 10);

        TimePeriod currentPeriod = GetSafeCurrentPeriod();
        bool dailyState = GetSafeDailyState();

        Debug.Log($"[gameUIManager] currentPeriod = {currentPeriod}");

        if (cutScenePanelMessage.TryGetValue(currentPeriod, out var msg))
        {
            if (currentPeriod == TimePeriod.AtHomeBeforeSleep && dailyState)
            {
                msg = "all day stay home, i want to sleep...";
            }

            if (newGameManager.Instance != null && newGameManager.Instance.eventManager != null)
            {
                newGameManager.Instance.eventManager.ShowEventPanel(
                    msg,
                    newGameManager.Instance.eventManager.defaultEventPanelPrefab
                );
            }
            else
            {
                Debug.LogWarning("[gameUIManager] eventManager 不存在，改用本地顯示測試訊息: " + msg);

                if (cutScenePanelTextUI != null)
                    cutScenePanelTextUI.text = msg;

                if (cutScenePanel != null)
                    cutScenePanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning($"TimePeriod {currentPeriod} 尚未配置 cut-scene 訊息！");
        }

        if (panel != null)
            panel.gameObject.SetActive(false);

        if (messageBox != null)
            messageBox.anchoredPosition = hiddenPos;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }
    }

    private TimePeriod GetSafeCurrentPeriod()
    {
        if (newGameManager.Instance != null &&
            newGameManager.Instance.timeSystem != null &&
            newGameManager.Instance.timeSystem.gameTime != null)
        {
            return newGameManager.Instance.timeSystem.gameTime.currentPeriod;
        }

        if (useTestDefaultWhenManagerMissing)
        {
            Debug.LogWarning($"[gameUIManager] 沒有抓到 newGameManager/timeSystem/gameTime，改用測試預設值: {testDefaultPeriod}");
            return testDefaultPeriod;
        }

        return TimePeriod.AtHomeAfterWakeUp;
    }

    private bool GetSafeDailyState()
    {
        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
            return newGameManager.Instance.playerStats.getDailyState();

        Debug.LogWarning($"[gameUIManager] playerStats 不存在，改用測試預設 dailyState: {testDailyState}");
        return testDailyState;
    }

    private void ReturnToMainMenu()
    {
        TogglePausePanel();
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void TogglePausePanel()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
            Debug.Log(isPaused ? "顯示暫停選單" : "隱藏暫停選單");
        }
    }

    private void OnReturnToMenuButtonClicked()
    {
        if (newGameManager.Instance != null)
            newGameManager.Instance.ReturnToMainMenu_gm();
        else
            Debug.LogWarning("[gameUIManager] newGameManager.Instance 不存在，無法返回主選單");
    }

    public void ShowMessage(string msg)
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        if (panel == null) return;

        panel.gameObject.SetActive(true);

        if (messageText != null)
            messageText.text = msg;

        slideCoroutine = StartCoroutine(SlideInOut());
    }

    private IEnumerator SlideInOut()
    {
        yield return StartCoroutine(SlideTo(visiblePos));
        yield return new WaitForSeconds(showDuration);
        yield return StartCoroutine(SlideTo(hiddenPos));
        panel.gameObject.SetActive(false);
    }

    private IEnumerator SlideTo(Vector2 targetPos)
    {
        if (messageBox == null) yield break;

        while (Vector2.Distance(messageBox.anchoredPosition, targetPos) > 1f)
        {
            messageBox.anchoredPosition = Vector2.MoveTowards(
                messageBox.anchoredPosition,
                targetPos,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        messageBox.anchoredPosition = targetPos;
    }

    public void ShowMilk()
    {
        Debug.Log("進來ㄌ！");

        if (shopBuyMilkPanel != null)
            shopBuyMilkPanel.gameObject.SetActive(true);

        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
            newGameManager.Instance.playerStats.nextAction = NextAction.goToMarket;
        else
            Debug.LogWarning("[gameUIManager] playerStats 不存在，無法設定 nextAction");
    }
}