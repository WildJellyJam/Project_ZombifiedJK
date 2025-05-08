using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class MainMenuUIManager : MonoBehaviour
{
    // public static UIManager Instance { get; private set; }

    [Header("主頁面按鈕容器")]
    // 主頁面UI元素
    [Header("主頁面UI")]
    public Button continueButton;
    public Button newGameButton;
    public Button exitButton;
    public GameObject saveListPanel;
    public Button[] saveSlotButtons;
    // public Button backToMenuButtons;

    // 遊戲內UI元素
    // [Header("遊戲內UI")]
    public GameObject statsPanel;
    // public Slider sanitySlider;
    // public Slider socialEnergySlider;
    // public Slider popularitySlider;
    // public Slider anxietySlider;
    // public TextMeshProUGUI affectionText;

    // // 劇情選擇UI
    // [Header("劇情選擇UI")]
    // public GameObject choicePanel;
    // public Button[] choiceButtons;

    // [Header("隨機事件UI")]
    // public GameObject randomEventPanel;
    // public Button eventButton;

    // [Header("過場動畫")]
    // public GameObject sleepTransitionPanel;
    // public Image transitionImage;

    // [Header("暫停選單UI")]
    // public GameObject pausePanel;
    // public Button returnToMainMenuButton;

    // public bool isPaused = false;

    // // 結局UI
    // [Header("結局UI")]
    // public GameObject endingPanel;
    // public TextMeshProUGUI endingText;
    // public Button replayButton;
    // public Button returnToMenuButton;
    private SaveSystem saveSystem;

    void Awake()
    {
        // if (Instance == null)
        // {
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
        Debug.Log("awake scene");
        InitializeMainMenuButtons();
        CheckTheListener();
        // gameManager.StartNewGame();
    }

    void Start()
    {
        Debug.Log("在start喔");

        // 初始化主頁面按鈕（初始場景）
        InitializeMainMenuButtons();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

    }


    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        newGameManager.Instance.ReturnToMainMenuEvent += displayButton;
    }
    void OnDisable()
    {
        newGameManager.Instance.ReturnToMainMenuEvent -= displayButton;
    }


    void displayButton()
    {
        exitButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        newGameButton.gameObject.SetActive(true);
    }
    // 初始化主頁面按鈕
    private void InitializeMainMenuButtons()
    {
        Debug.Log("在初始化按鈕喔");


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
    }
    public void CheckTheListener()
    {
        Debug.Log("into CheckTheListtener function");
        continueButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        // backToMenuButtons.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        // backToMenuButtons.onClick.AddListener(BackToMenuButtonPressed);
    }
    public void RefreshMainMenuButtons()
    {
        InitializeMainMenuButtons();
    }
    private void OnContinueButtonClicked()
    {
        if (saveListPanel != null)
        {
            saveListPanel.SetActive(true);
            UpdateSaveListUI();
        }
    }

    public void OnNewGameButtonClicked()
    {
        Debug.Log("new game button be click");
        newGameManager.Instance.StartNewGame();
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
        newGameManager.Instance.LoadGame(slotIndex);
        if (saveListPanel != null) saveListPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);
    }

    private void UpdateSaveListUI()
    {
        var saveSlots = newGameManager.Instance.saveSystem.GetSaveSlots();
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
}