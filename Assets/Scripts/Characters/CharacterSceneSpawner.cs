using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSceneSpawner : MonoBehaviour
{
    public static CharacterSceneSpawner Instance;

    private const string AUTO_CANVAS_NAME = "AutoCanvas_CharacterUI";

    [Header("Character UI Settings")]
    [Tooltip("這裡放 UI 用的角色 Prefab（建議：Prefab root 是一個含有 RectTransform + Image/TMP 的 UI 物件）")]
    public GameObject characterPrefab;

    [Header("Scene Visibility Settings")]
    [Tooltip("角色【不想出現】的場景名稱。沒有列在這裡的場景，一律會顯示角色。")]
    public string[] scenesToHideCharacter;

    [Header("UI Parent")]
    [Tooltip("不用填也可以：會永遠自動使用/生成名為 AutoCanvas_CharacterUI 的 Canvas（sortingOrder=5）")]
    public RectTransform uiParent;

    private GameObject spawnedCharacter;

    private void Awake()
    {
        // ✅ Singleton 防止出現兩個 Spawner
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 一開始的場景也要套用顯示邏輯
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyForScene(scene.name);
    }

    private void ApplyForScene(string sceneName)
    {
        // 第一次用到前，先確保有生出角色
        EnsureCharacterSpawned();

        if (spawnedCharacter == null)
            return;

        bool shouldShow = true;

        // 如果在「隱藏清單」裡，就不要顯示
        if (scenesToHideCharacter != null)
        {
            foreach (string hideScene in scenesToHideCharacter)
            {
                if (!string.IsNullOrEmpty(hideScene) && sceneName == hideScene)
                {
                    shouldShow = false;
                    break;
                }
            }
        }

        spawnedCharacter.SetActive(shouldShow);
    }

    /// <summary>
    /// 確保角色至少被生成一次（只會 Instantiate 一次）
    /// </summary>
    private void EnsureCharacterSpawned()
    {
        if (spawnedCharacter != null)
            return;

        if (characterPrefab == null)
        {
            Debug.LogError("❌ CharacterSceneSpawner：characterPrefab 沒有指定！（請指定 UI Prefab）");
            return;
        }

        // ✅ 永遠只用你自己的 AutoCanvas（找不到就生成）
        EnsureAutoCanvasParent();

        if (uiParent == null)
        {
            Debug.LogError("❌ CharacterSceneSpawner：AutoCanvas 生成/取得失敗，uiParent 仍為 null。");
            return;
        }

        // ✅ 只 Instantiate 一次，之後都只 SetActive 開關，不再 Destroy/重生
        spawnedCharacter = Instantiate(characterPrefab, uiParent, false);

        // 如果你希望角色本身也跨場景存在，可以打開：
        // DontDestroyOnLoad(spawnedCharacter);
    }

    /// <summary>
    /// 只使用/生成你自己的 Canvas：AutoCanvas_CharacterUI，並強制 sortingOrder=5
    /// </summary>
    private void EnsureAutoCanvasParent()
    {
        GameObject go = GameObject.Find(AUTO_CANVAS_NAME);
        Canvas canvas = null;

        if (go != null)
            canvas = go.GetComponent<Canvas>();

        // 沒有就生成
        if (canvas == null)
        {
            go = new GameObject(AUTO_CANVAS_NAME);
            canvas = go.AddComponent<Canvas>();
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // sortingOrder 要生效必須 overrideSorting
            canvas.overrideSorting = true;
            canvas.sortingOrder = 24;

            // ✅ 讓這個 Canvas 跨場景存在（你說你要「都用我生成的」）
            DontDestroyOnLoad(go);
        }
        else
        {
            // ✅ 就算已存在，也強制保持你要的設定
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5;
        }

        uiParent = canvas.transform as RectTransform;
    }
}
