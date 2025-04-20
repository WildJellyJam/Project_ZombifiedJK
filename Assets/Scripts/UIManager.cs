using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 主頁面UI元素
    [Header("主頁面UI")]
    public Button continueButton;
    public Button newGameButton;
    public Button exitButton;
    public GameObject saveListPanel;
    public Button[] saveSlotButtons;

    // 遊戲內UI元素
    [Header("遊戲內UI")]
    public GameObject statsPanel;     // 數值顯示面板
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
    public GameObject sleepTransitionPanel; // 睡覺過場面板
    public Image transitionImage; // 淡入淡出圖片

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
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("未找到GameManager，請確保場景中有GameManager物件！");
        }

        // 綁定按鈕事件
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);

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


        // 初始化UI
        if (saveListPanel != null) saveListPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (endingPanel != null) endingPanel.SetActive(false);

        sleepTransitionPanel.SetActive(false);

        // 場景載入時重新查找UI元素
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        randomEventPanel.SetActive(false);
        eventButton.onClick.AddListener(OnEventCompleted);
    }

    void OnDestroy()
    {
        // 移除場景載入事件，避免內存洩漏
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 場景載入時重新查找UI元素
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 重新查找statsPanel
        statsPanel = GameObject.Find("StatsPanel");
        if (statsPanel == null)
        {
            Debug.LogWarning("未找到StatsPanel，請確保場景中存在該GameObject！");
        }
        else
        {
            // 重新獲取子物件
            sanitySlider = statsPanel.transform.Find("SanitySlider")?.GetComponent<Slider>();
            socialEnergySlider = statsPanel.transform.Find("SocialEnergySlider")?.GetComponent<Slider>();
            popularitySlider = statsPanel.transform.Find("PopularitySlider")?.GetComponent<Slider>();
            anxietySlider = statsPanel.transform.Find("AnxietySlider")?.GetComponent<Slider>();
            affectionText = statsPanel.transform.Find("AffectionText")?.GetComponent<TextMeshProUGUI>();
        }

        // 重新查找其他UI元素（如果需要）
        saveListPanel = GameObject.Find("SaveListPanel");
        continueButton = GameObject.Find("ContinueButton")?.GetComponent<Button>();
        newGameButton = GameObject.Find("NewGameButton")?.GetComponent<Button>();
        exitButton = GameObject.Find("ExitButton")?.GetComponent<Button>();
        choicePanel = GameObject.Find("ChoicePanel");
        endingPanel = GameObject.Find("EndingPanel");

        // 重新初始化按鈕事件（如果找到按鈕）
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    void Update()
    {
        // 添加null檢查
        if (statsPanel != null && statsPanel.activeSelf)
        {
            UpdateStatsUI();
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
        transitionImage.color = new Color(0, 0, 0, 0); // 初始透明

        // 淡入（變黑）
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            transitionImage.color = new Color(0, 0, 0, elapsed);
            yield return null;
        }

        // 顯示睡覺文字（可選）
        yield return new WaitForSeconds(1f);

        // 淡出（變透明）
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
        // 假設顯示一個簡單的事件按鈕，實際遊戲中可以顯示具體事件描述
        eventButton.GetComponentInChildren<TextMeshProUGUI>().text = "完成事件";
    }
    private void OnEventCompleted()
    {
        randomEventPanel.SetActive(false);
        GameManager.Instance.randomEventManager.TriggerRandomEvent(GameManager.Instance.playerStats);
    }
}