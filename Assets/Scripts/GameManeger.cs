using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 需要這個來處理按鈕

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 場景相關
    public int stage = 0;
    public int maxStages = 5;

    // 時間相關
    public float gameTime = 6.0f; // 遊戲時間（0.0 到 24.0 表示一天）
    public float timeSpeed = 0f;  // 時間流速，預設為 0，事件觸發後改變
    public float timePerDay = 24.0f; // 一天總時間（24小時制）

    // 時間段與場景對應
    [System.Serializable]
    public struct TimeSceneMapping
    {
        public string sceneName;    // 場景名稱
        public float startTime;     // 開始時間（例如 6.0 表示早晨 6 點）
    }
    public TimeSceneMapping[] timeScenes; // 時間段與場景的映射表

    // UI 或控制相關
    public GameObject StartBtn; // 開始按鈕
    public GameObject EndBtn;   // 結束按鈕

    // 遊戲狀態
    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState currentState = GameState.Menu;

    // 事件
    public static event System.Action OnLevelCompleted;
    public static event System.Action<float> OnTimeUpdated; // 時間更新事件

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 綁定開始按鈕的事件
        if (StartBtn != null)
        {
            StartBtn.GetComponent<Button>().onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogWarning("StartBtn is not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameTime();
            CheckTimeSceneTransition();
        }
    }

    // 更新遊戲時間
    private void UpdateGameTime()
    {
        gameTime += Time.deltaTime * timeSpeed;
        if (gameTime >= timePerDay)
        {
            gameTime -= timePerDay; // 重置到新的一天
        }
        OnTimeUpdated?.Invoke(gameTime); // 通知外部時間更新（例如 UI）
    }

    // 檢查是否需要切換場景
    private void CheckTimeSceneTransition()
    {
        foreach (var mapping in timeScenes)
        {
            if (gameTime >= mapping.startTime && gameTime < mapping.startTime + 6.0f) // 假設每個時間段 6 小時
            {
                if (SceneManager.GetActiveScene().name != mapping.sceneName)
                {
                    SceneManager.LoadScene(mapping.sceneName);
                }
                break;
            }
        }
    }

    // 載入當前場景
    public void LoadLevel()
    {
        if (stage >= 0 && stage < maxStages)
        {
            SceneManager.LoadScene($"Scene{stage}");
            currentState = GameState.Playing;
        }
        else
        {
            Debug.LogError($"Invalid stage number: {stage}");
        }
    }

    // 進入下一關
    public void NextLevel()
    {
        stage++;
        if (stage < maxStages)
        {
            LoadLevel();
            OnLevelCompleted?.Invoke();
        }
        else
        {
            Debug.Log("Game Completed!");
            currentState = GameState.GameOver;
        }
    }

    // 延遲重載場景
    public void DelayReset(float delay)
    {
        Invoke(nameof(LoadLevel), delay);
    }

    // 開始遊戲
    public void StartGame()
    {
        stage = 0;
        gameTime = 6.0f; // 從早晨 6 點開始
        timeSpeed = 0f;  // 預設時間不流動
        currentState = GameState.Playing;
        SceneManager.LoadScene("Morning"); // 點擊開始鍵後直接進入 Morning 場景
    }

    // 結束遊戲
    public void EndGame()
    {
        Application.Quit();
    }

    // 暫停遊戲
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    // 恢復遊戲
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    // 外部事件調用：改變時間流速
    public void SetTimeSpeed(float speed)
    {
        timeSpeed = speed;
        Debug.Log($"Time speed set to: {speed}");
    }

    // 外部事件調用：直接跳轉時間
    public void SetGameTime(float newTime)
    {
        gameTime = Mathf.Clamp(newTime, 0f, timePerDay);
        CheckTimeSceneTransition(); // 檢查是否需要立即切換場景
    }
}