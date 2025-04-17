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
    public int gameWeek = 1;
    public CameraManager cameraManager;

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
        //npcAffection = new NPCAffection();
        randomEventManager = gameObject.AddComponent<RandomEventManager>();
        inventory = new Inventory();
        miniGameManager = gameObject.AddComponent<MiniGameManager>();
        sceneManager = gameObject.AddComponent<SceneManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("未找到CameraManager，請確保場景中有CameraManager物件！");
        }

        if (npcAffection == null)
        {
            npcAffection = new NPCAffection();
        }
    }

    void Update()
    {
        timeSystem.UpdateTime(Time.deltaTime);
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
    }

    // 修正類型為 TimeSystem.TimeUpdatedHandler
    public void SubscribeToTimeUpdated(TimeSystem.TimeUpdatedHandler callback)
    {
        timeSystem.OnTimeUpdated += callback;
    }

    public void UnsubscribeFromTimeUpdated(TimeSystem.TimeUpdatedHandler callback)
    {
        timeSystem.OnTimeUpdated -= callback;
    }

    public void StartNewGame()
    {
        gameWeek = 1;
        ResetForNewWeek();
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
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
        }
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
    public void OnSceneLoaded()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        cameraManager.SwitchCamera(0); // 默認切換到前方視角
    }
}