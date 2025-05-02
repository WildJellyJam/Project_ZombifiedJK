using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TimeSystem timeSystem;
    public SaveSystem saveSystem;
    public PlayerStats playerStats;
    public NPCAffection npcAffection;
    public RandomEventManager randomEventManager;
    public Inventory inventory;
    public MiniGameManager miniGameManager;
    public SceneManager sceneManager;
    public PlayerMovement playerMovement;
    public CameraManager cameraManager;
    public int gameWeek = 1;

    private UIManager uiManager;
    private bool isGameStarted = false;

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

        timeSystem = gameObject.AddComponent<TimeSystem>();
        saveSystem = gameObject.AddComponent<SaveSystem>();
        playerStats = new PlayerStats { sanity = 100f, socialEnergy = 100f, popularity = 0f, anxiety = 0f };
        npcAffection = new NPCAffection();
        randomEventManager = gameObject.AddComponent<RandomEventManager>();
        inventory = new Inventory();
        miniGameManager = gameObject.AddComponent<MiniGameManager>();
        sceneManager = gameObject.AddComponent<SceneManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        cameraManager = FindObjectOfType<CameraManager>();

        // uiManager = FindObjectOfType<UIManager>();
        // if (uiManager == null)
        // {
        //     Debug.LogError("未找到UIManager，請確保場景中有UIManager物件！");
        // }
    }

    void Update()
    {
        /*if (isGameStarted) // 只有遊戲開始後才更新時間和場景
        {
            timeSystem.UpdateTime(Time.deltaTime);
            sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);

            if (playerStats.anxiety > 120)
            {
                uiManager.ShowEnding("Bad");
            }
        }*/
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
            Debug.LogWarning($"[GameManager] UIManager 未在場景 {scene.name} 中找到！");
        else
            Debug.Log($"[GameManager] 已找到新場景中的 UIManager：{scene.name}");
    }


    public void StartNewGame()
    {
        gameWeek = 1;
        ResetForNewWeek();
        isGameStarted = true;
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
        TriggerNextEvent();

        MakeUIRight();
    }

    public void LoadGame(int slotIndex)
    {
        SaveData data = saveSystem.LoadGame(slotIndex);
        if (data != null)
        {
            timeSystem.gameTime = data.gameTime;
            playerStats = data.playerStats;
            inventory = data.inventory;
            randomEventManager = new RandomEventManager();
            sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
            cameraManager.InitializeCamera();
            TriggerNextEvent();

            MakeUIRight();
        }
    }
    public void MakeUIRight()
    {
        if (uiManager != null)
            {
                uiManager.pausePanel.SetActive(false);
                uiManager.randomEventPanel.SetActive(false);
                uiManager.isPaused = false;
            }
    }
    public void ReturnToMainMenu_gm()
    {
        // 重置遊戲狀態
        //ResetForNewWeek();

        // 隱藏所有遊戲中UI（例如隨機事件面板、暫停選單）
        if (uiManager != null)
        {
            uiManager.pausePanel.SetActive(false);
            uiManager.randomEventPanel.SetActive(false);
            uiManager.isPaused = false;
            uiManager.statsPanel.SetActive(false);
            uiManager.choicePanel.SetActive(false);
            uiManager.endingPanel.SetActive(false);
            uiManager.exitButton.gameObject.SetActive(true);
            uiManager.continueButton.gameObject.SetActive(true);
            uiManager.newGameButton.gameObject.SetActive(true);
        }

        // 切換到主選單場景（假設主選單場景名為 "MainMenu"）
        //UnityEngine.SceneManagement.SceneManager.LoadScene("StartUpMenu");
        sceneManager.LoadScene("StartUpMenu");

        if (uiManager != null)
        {
            uiManager.RefreshMainMenuButtons();  // 加上這一行
        }

        // 可選：如果主選單需要特定的初始化邏輯，可以在這裡調用
        Debug.Log("返回主頁面");
    }

    public void OnTimeManuallyUpdated()
    {
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);

        if (playerStats.anxiety > 120)
        {
            uiManager.ShowEnding("Bad");
        }
    }

    public void TriggerChoiceEvent(string[] choices)
    {
        uiManager.ShowChoices(choices, (choiceIndex) =>
        {
            if (choiceIndex == 0)
            {
                playerStats.UpdateAnxiety(5f);
                playerStats.socialEnergy -= 10f;
            }
            else if (choiceIndex == 1)
            {
                playerStats.UpdateAnxiety(-5f);
                playerStats.socialEnergy -= 5f;
            }
            timeSystem.AddEventTime(0.5f);
        });
    }
    public void TriggerNextEvent()
    {
        // 假設由UI觸發，這裡直接調用隨機事件
        uiManager.ShowRandomEventOptions();
    }

    public void EndGameWeek()
    {
        if (playerStats.anxiety < 20)
        {
            uiManager.ShowEnding("Good");
        }
        else if (playerStats.anxiety > 100)
        {
            uiManager.ShowEnding("Bad");
        }
        else
        {
            uiManager.ShowEnding("Normal");
        }

        gameWeek++;
        ResetForNewWeek();
    }

    private void ResetForNewWeek()
    {
        timeSystem.gameTime = new GameTime { day = 1, hours = 16f, currentPeriod = TimePeriod.AtHomeBeforeSleep };
        playerStats.sanity = 100f;
        playerStats.socialEnergy = 100f;
        playerStats.popularity = 0f;
        playerStats.anxiety = 0f;
        randomEventManager = gameObject.AddComponent<RandomEventManager>();
    }

}