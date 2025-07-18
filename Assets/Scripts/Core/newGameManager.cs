using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class newGameManager : MonoBehaviour
{
    public static newGameManager Instance { get; private set; }

    public event System.Action ResetNewWeekEvent;
    public event System.Action ShowRandomEvent;
    public event System.Action<TimePeriod> SwitchSceneEvent;
    public event System.Action<TimePeriod> LoadGameSceneEvent;
    public event System.Action LoadGameEvent;
    public event System.Action ReturnToMainMenuEvent;

    // need to catch object in scene
    public CameraManager cameraManager;
    public PlayerMovement playerMovement;

    // static class
    // public Inventory inventory;
    // public MiniGameManager miniGame;
    public SceneManage sceneManage = new SceneManage();
    public PlayerStats playerStats = new PlayerStats();
    //public SceneManager sceneManager;
    public gameUIManager uiManager;

    public SaveSystem saveSystem;// = new SaveSystem();
    public TimeSystem timeSystem = new TimeSystem();
    public TimeUI timeUI;

    // ? 
    public NPCAffection npcAffection = new NPCAffection();
    public RandomEventManager eventManager;


    public int gameWeek = 1;
    private bool isGameStarted = false;

    /*void Start()
    {
        GameObject.Find("TimeText").GetComponent<TimeUI>();
    }*/

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 嘗試抓 TimeUI 元件（確認你物件名稱為 "TimeText"）
        GameObject timeTextObj = GameObject.Find("Canvas/TimeText");
        GameObject randomEventObj = GameObject.Find("randomEvent");
        if (timeTextObj != null)
        {
            timeUI = timeTextObj.GetComponent<TimeUI>();
            Debug.Log("TimeUI 綁定成功！");
        }
        else
        {
            timeUI = null;
            Debug.LogWarning("找不到 TimeUI，請確認 Canvas/TimeText 是否正確！");
        }
        if (randomEventObj != null)
        {
            eventManager = randomEventObj.GetComponent<RandomEventManager>();
            Debug.Log("RandomEventManager 綁定成功！");
        }
        else
        {
            eventManager = null;
            Debug.LogWarning("找不到 RandomEventManager，請確認 randomEvent 是否正確！");
        }
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
        sceneManage.SwitchScene(); //

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
            // playerStats = data.playerStats;
            // inventory = data.inventory;
            // randomEventManager = new RandomEventManager();
            sceneManage.SwitchScene(); //
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

    public void OnTimeManuallyUpdated()
    {
        sceneManage.SwitchScene();
    } 
}