using UnityEngine;

public class newGameManager : MonoBehaviour
{
    public static newGameManager Instance { get; private set; }

    public event System.Action ResetNewWeekEvent;
    public event System.Action ShowRandomEvent;
    public event System.Action<TimePeriod> SwitchSceneEvent;
    public event System.Action<TimePeriod> LoadGameSceneEvent;
    public event System.Action LoadGameEvent;
    public event System.Action ReturnToMainMenuEvent;

    public SaveSystem saveSystem = new SaveSystem();
    public TimeSystem timeSystem = new TimeSystem();
    public SceneManage sceneManage = new SceneManage();



    public PlayerStats playerStats;
    public Inventory inventory;



    public int gameWeek = 1;

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
    }

    void Update()
    {
        
    }

    public void ReturnToMainMenu_gm()
    {
        ReturnToMainMenuEvent?.Invoke();
        sceneManage.ReturnToMainMenuScene();
        // 可選：如果主選單需要特定的初始化邏輯，可以在這裡調用
        Debug.Log("返回主頁面");
    }

// void OnEnable()
//     {
//         newGameManager.Instance.SwitchSceneEvent += SwitchSceneBasedOnTime;
//         newGameManager.Instance.LoadGameSceneEvent += SwitchSceneBasedOnTime;
//     }
//     void OnDisable()
//     {
//         newGameManager.Instance.SwitchSceneEvent -= SwitchSceneBasedOnTime;
//         newGameManager.Instance.LoadGameSceneEvent -= SwitchSceneBasedOnTime;
//     }
    public void StartNewGame()
    {
        gameWeek = 1;
        ResetForNewWeek();
        isGameStarted = true;
        sceneManage.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod); //
        
        // uiManager.ShowRandomEventOptions();
        ShowRandomEvent?.Invoke();
        // MakeUIRight();
    }

    public void LoadGame(int slotIndex)
    {
        SaveData data = saveSystem.LoadGame(slotIndex);
        if (data != null)
        {
            timeSystem.gameTime = data.gameTime;
            playerStats = data.playerStats;
            inventory = data.inventory;
            // randomEventManager = new RandomEventManager();
            sceneManage.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod); //
            // cameraManager.InitializeCamera(); //
            LoadGameSceneEvent.Invoke(timeSystem.gameTime.currentPeriod);
            LoadGameEvent.Invoke();
            // uiManager.ShowRandomEventOptions();
            ShowRandomEvent?.Invoke();
            // MakeUIRight();
        }
    }

    private void ResetForNewWeek()
    {
        timeSystem.gameTime = new GameTime { day = 1, hours = 16f, currentPeriod = TimePeriod.AtHomeBeforeSleep };
        ResetNewWeekEvent?.Invoke();
        playerStats.sanity = 100f;
        playerStats.socialEnergy = 100f;
        playerStats.popularity = 0f;
        playerStats.anxiety = 0f;
        // randomEventManager = gameObject.AddComponent<RandomEventManager>();
    }

}