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

        // 初始化模組
        timeSystem = gameObject.AddComponent<TimeSystem>();
        saveSystem = gameObject.AddComponent<SaveSystem>();
        playerStats = new PlayerStats { sanity = 100f, socialEnergy = 100f, popularity = 0f, anxiety = 0f };
        npcAffection = new NPCAffection();
        randomEventManager = gameObject.AddComponent<RandomEventManager>();
        inventory = new Inventory();
        miniGameManager = gameObject.AddComponent<MiniGameManager>();
        sceneManager = gameObject.AddComponent<SceneManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        timeSystem.UpdateTime(Time.deltaTime);
        sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
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
            randomEventManager = new RandomEventManager(); // 重新初始化
            sceneManager.SwitchSceneBasedOnTime(timeSystem.gameTime.currentPeriod);
        }
    }
}