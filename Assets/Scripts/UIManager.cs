using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("主頁面按鈕容器")]
    public GameObject mainMenuButtonsContainer;
    // 主頁面UI元素
    [Header("主頁面UI")]
    public Button continueButton;
    public Button newGameButton;
    public Button exitButton;
    public GameObject saveListPanel;
    public Button[] saveSlotButtons;
    public Button backToMenuButtons;

    // 遊戲內UI元素
    [Header("遊戲內UI")]
    public GameObject statsPanel;
    public Slider sanitySlider;
    public Slider socialEnergySlider;
    public Slider popularitySlider;
    public Slider anxietySlider;
    public TextMeshProUGUI affectionText;

    // 劇情選擇UI
    [Header("劇情選擇UI")]
    public GameObject choicePanel;
    public Button[] choiceButtons;

    [Header("隨機事件UI")]
    public GameObject randomEventPanel;
    public Button eventButton;

    [Header("過場動畫")]
    public GameObject sleepTransitionPanel;
    public Image transitionImage;

    [Header("暫停選單UI")]
    public GameObject pausePanel;
    public Button returnToMainMenuButton;

    public bool isPaused = false;

    // 結局UI
    [Header("結局UI")]
    public GameObject endingPanel;
    public TextMeshProUGUI endingText;
    public Button replayButton;
    public Button returnToMenuButton;

    private GameManager gameManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("在start喔");
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("未找到GameManager，請確保場景中有GameManager物件！");
        }

        // 初始化主頁面按鈕（初始場景）
        InitializeMainMenuButtons();

        // 初始化其他 UI
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            if (saveSlotButtons[i] != null)
                saveSlotButtons[i].onClick.AddListener(() => OnSaveSlotClicked(slotIndex));
        }

        if (replayButton != null)
            replayButton.onClick.AddListener(OnReplayButtonClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuButtonClicked);

        if (backToMenuButtons != null)
            backToMenuButtons.onClick.AddListener(BackToMenuButtonPressed);

        if (saveListPanel != null) saveListPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);
        if (pausePanel != null)
        {
            
            if (returnToMainMenuButton != null)
            {
                returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("未設置PausePanel，請在Unity編輯器中分配！");
        }

        sleepTransitionPanel.SetActive(false);
        randomEventPanel.SetActive(false);
        if (eventButton != null)
            eventButton.onClick.AddListener(OnEventCompleted);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 初始化主頁面按鈕
    private void InitializeMainMenuButtons()
    {
        Debug.Log("在初始化按鈕喔");
        if (mainMenuButtonsContainer == null)
        {
            Debug.LogError("MainMenuButtonsContainer 未設置，請在 Unity 編輯器中分配！");
            return;
        }
        /*continueButton = GameObject.Find("continueBtn")?.GetComponent<Button>();
        newGameButton = GameObject.Find("startBtn")?.GetComponent<Button>();
        exitButton = GameObject.Find("endBtn")?.GetComponent<Button>();
        backToMenuButtons = GameObject.Find("backBtn")?.GetComponent<Button>();
        saveListPanel = GameObject.Find("savePanel");*/
        if (backToMenuButtons != null)
        {
            backToMenuButtons.onClick.RemoveAllListeners();
            backToMenuButtons.onClick.AddListener(BackToMenuButtonPressed);
            Debug.Log("返回主頁面按鈕已綁定");
        }
        else
        {
            Debug.LogError("continueButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'continueBtn'！");
        }


        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            Debug.Log("繼續遊戲按鈕已綁定");
        }
        else
        {
            Debug.LogError("continueButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'continueBtn'！");
        }

        if (newGameButton != null)
        {
            continueButton.gameObject.SetActive(true);
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnNewGameButtonClicked);
            Debug.Log("開始新遊戲按鈕已綁定");
        }
        else
        {
            Debug.LogError("newGameButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'startBtn'！");
        }

        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitButtonClicked);
            Debug.Log("退出遊戲按鈕已綁定");
        }
        else
        {
            Debug.LogError("exitButton 未找到，請檢查 MainMenuButtonsContainer 中是否存在 'endBtn'！");
        }
    }

    // 場景載入時重新查找UI元素
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("111}");
        // 延遲查找 UI 元素，確保場景完全載入
        StartCoroutine(DelayedInitializeUI(scene));
    }

    private System.Collections.IEnumerator DelayedInitializeUI(Scene scene)
    {
        // 等待一幀，確保場景中的 UI 物件已生成
        yield return null;

        Debug.Log($"場景已載入：{scene.name}");

        // 根據場景類型初始化 UI
        if (scene.name == "StartUpMenu")
        {
            // 重新查找主頁面按鈕
            InitializeMainMenuButtons();

            // 檢查按鈕是否啟用
            if (continueButton != null)
                Debug.Log($"continueBtn 啟用狀態：{continueButton.gameObject.activeInHierarchy}");
            if (newGameButton != null)
                Debug.Log($"startBtn 啟用狀態：{newGameButton.gameObject.activeInHierarchy}");
            if (exitButton != null)
                Debug.Log($"endBtn 啟用狀態：{exitButton.gameObject.activeInHierarchy}");
        }
        else
        {
            // 遊戲場景：查找其他 UI 元素
            statsPanel = GameObject.Find("StatsPanel");
            if (statsPanel != null)
            {
                sanitySlider = statsPanel.transform.Find("SanitySlider")?.GetComponent<Slider>();
                socialEnergySlider = statsPanel.transform.Find("SocialEnergySlider")?.GetComponent<Slider>();
                popularitySlider = statsPanel.transform.Find("PopularitySlider")?.GetComponent<Slider>();
                anxietySlider = statsPanel.transform.Find("AnxietySlider")?.GetComponent<Slider>();
                affectionText = statsPanel.transform.Find("AffectionText")?.GetComponent<TextMeshProUGUI>();
                Debug.Log("StatsPanel 已找到");
            }
            else
            {
                Debug.LogWarning("未找到StatsPanel，請確保場景中存在該GameObject！");
            }

            choicePanel = GameObject.Find("ChoicePanel");
            endingPanel = GameObject.Find("EndPanel");
        }
    }
    public void CheckTheListener()
    {
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        backToMenuButtons.onClick.AddListener(BackToMenuButtonPressed);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausePanel();
        }

        if (statsPanel != null && statsPanel.activeSelf)
        {
            UpdateStatsUI();
        }
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

    private void ReturnToMainMenu()
    {
        isPaused = false;
        TogglePausePanel();
        if (gameManager != null)
        {
            gameManager.ReturnToMainMenu_gm();
        }
        else
        {
            Debug.LogError("GameManager 未初始化，無法返回主頁面！");
        }
    }

    private void OnContinueButtonClicked()
    {
        if (saveListPanel != null)
        {
            saveListPanel.SetActive(true);
            UpdateSaveListUI();
        }
    }

    private void OnNewGameButtonClicked()
    {
        gameManager.StartNewGame();
        exitButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        newGameButton.gameObject.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);
    }

    private void OnExitButtonClicked()
    {
        Application.Quit();
    }

    private void OnSaveSlotClicked(int slotIndex)
    {
        gameManager.LoadGame(slotIndex);
        if (saveListPanel != null) saveListPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);
    }

    private void UpdateSaveListUI()
    {
        var saveSlots = gameManager.saveSystem.GetSaveSlots();
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            if (saveSlotButtons[i] == null) continue;
            if (i < saveSlots.Count && saveSlots[i] != null)
            {
                saveSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                    $"存檔 {i + 1}: {saveSlots[i].timestamp}, Day {saveSlots[i].gameTime.day}";
            }
            else
            {
                saveSlotButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"存檔 {i + 1}: 空";
            }
        }
    }

    private void UpdateStatsUI()
    {
        if (sanitySlider != null) sanitySlider.value = gameManager.playerStats.sanity / 100f;
        if (socialEnergySlider != null) socialEnergySlider.value = gameManager.playerStats.socialEnergy / 100f;
        if (popularitySlider != null) popularitySlider.value = gameManager.playerStats.popularity / 100f;
        if (anxietySlider != null) anxietySlider.value = gameManager.playerStats.anxiety / 120f;

        if (affectionText != null)
        {
            string affectionDisplay = "";
            foreach (var npc in gameManager.npcAffection.affection)
            {
                affectionDisplay += $"{npc.Key}: {npc.Value}\n";
            }
            affectionText.text = affectionDisplay;
        }
    }

    public void ShowChoices(string[] choices, System.Action<int> onChoiceSelected)
    {
        if (choicePanel == null) return;
        choicePanel.SetActive(true);
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;
            choiceButtons[i].gameObject.SetActive(i < choices.Length);
            if (i < choices.Length)
            {
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = choices[i];
                int choiceIndex = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() =>
                {
                    onChoiceSelected(choiceIndex);
                    choicePanel.SetActive(false);
                });
            }
        }
    }

    public void ShowEnding(string endingType)
    {
        if (endingPanel == null) return;
        endingPanel.SetActive(true);
        if (endingText != null)
        {
            endingText.text = endingType switch
            {
                "Good" => "好結局：壓力低於20，恭喜你成功度過一周！",
                "Normal" => "普通結局：壓力適中，生活還算平穩。",
                "Bad" => "壞結局：壓力過高，你崩潰了...",
                _ => "未知結局"
            };
        }
    }

    private void OnReplayButtonClicked()
    {
        gameManager.StartNewGame();
        if (endingPanel != null) endingPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);
    }

    private void OnReturnToMenuButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartUpMenu");
        if (endingPanel != null) endingPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    public void ShowSleepTransition()
    {
        StartCoroutine(SleepTransitionCoroutine());
    }

    private System.Collections.IEnumerator SleepTransitionCoroutine()
    {
        sleepTransitionPanel.SetActive(true);
        transitionImage.color = new Color(0, 0, 0, 0);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transitionImage.color = new Color(0, 0, 0, elapsed);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transitionImage.color = new Color(0, 0, 0, 1f - elapsed);
            yield return null;
        }

        sleepTransitionPanel.SetActive(false);
    }

    public void ShowRandomEventOptions()
    {
        randomEventPanel.SetActive(true);
    }

    private void OnEventCompleted()
    {
        randomEventPanel.SetActive(false);
        GameManager.Instance.randomEventManager.TriggerRandomEvent(GameManager.Instance.playerStats);
    }

    public void BackToMenuButtonPressed()
    {
        saveListPanel.SetActive(false);
    }
}