using UnityEngine;

public class newGsameManager : MonoBehaviour
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
    public Inventory inventory;
    public MiniGameManager miniGame;
    public SceneManage sceneManage;
    public PlayerStats playerStats;

    public SaveSystem saveSystem = new SaveSystem();
    public TimeSystem timeSystem = new TimeSystem();
    public TimeUI timeUI = new TimeUI();

    // ? 
    public NPCAffection npcAffection = new NPCAffection();
    public RandomEventManager randomEventManager = new RandomEventManager();
    

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

    // public void OnTimeManuallyUpdated()
    // {
    //     sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);

    //     if (playerStats.anxiety > 120)
    //     {
    //         uiManager.ShowEnding("Bad");
    //     }
    // }

    // public void TriggerChoiceEvent(string[] choices)
    // {
    //     uiManager.ShowChoices(choices, (choiceIndex) =>
    //     {
    //         if (choiceIndex == 0)
    //         {
    //             playerStats.UpdateAnxiety(5f);
    //             playerStats.socialEnergy -= 10f;
    //         }
    //         else if (choiceIndex == 1)
    //         {
    //             playerStats.UpdateAnxiety(-5f);
    //             playerStats.socialEnergy -= 5f;
    //         }
    //         timeSystem.AddEventTime(0.5f);
    //     });
    // }
    // public void TriggerNextEvent()
    // {
    //     // 假設由UI觸發，這裡直接調用隨機事件
    //     uiManager.ShowRandomEventOptions();
    // }

    // public void EndGameWeek()
    // {
    //     if (playerStats.anxiety < 20)
    //     {
    //         uiManager.ShowEnding("Good");
    //     }
    //     else if (playerStats.anxiety > 100)
    //     {
    //         uiManager.ShowEnding("Bad");
    //     }
    //     else
    //     {
    //         uiManager.ShowEnding("Normal");
    //     }

    //     gameWeek++;
    //     ResetForNewWeek();
    // 
}