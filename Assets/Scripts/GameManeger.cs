using UnityEngine;

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

        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("未找到UIManager，請確保場景中有UIManager物件！");
        }
    }

    void Update()
    {
        timeSystem.UpdateTime(Time.deltaTime);
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);

        if (playerStats.anxiety > 120)
        {
            uiManager.ShowEnding("Bad");
        }
    }

    // 新增：訂閱時間更新事件
    public void SubscribeToTimeUpdated(System.Action<GameTime> callback)
    {
        timeSystem.OnTimeUpdated += callback;
    }

    // 新增：取消訂閱時間更新事件（可選，防止記憶體洩漏）
    public void UnsubscribeFromTimeUpdated(System.Action<GameTime> callback)
    {
        timeSystem.OnTimeUpdated -= callback;
    }

    public void StartNewGame()
    {
        gameWeek = 1;
        ResetForNewWeek();
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
        cameraManager.SwitchCamera(0);
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
            cameraManager.SwitchCamera(0);
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
        timeSystem.gameTime = new GameTime { day = 1, hours = 6f, currentPeriod = TimePeriod.MorningHome };
        playerStats.sanity = 100f;
        playerStats.socialEnergy = 100f;
        playerStats.popularity = 0f;
        playerStats.anxiety = 0f;
        randomEventManager = gameObject.AddComponent<RandomEventManager>();
    }
}