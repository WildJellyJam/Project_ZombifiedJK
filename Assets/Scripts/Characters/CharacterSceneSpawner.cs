using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSceneSpawner : MonoBehaviour
{
    public static CharacterSceneSpawner Instance;

    [Header("Character UI Settings")]
    [Tooltip("這裡放 UI 用的角色 Prefab（建議：Prefab root 是一個含有 RectTransform + Image/TMP 的 UI 物件）")]
    public GameObject characterPrefab;

    [Header("Scene Visibility Settings")]
    [Tooltip("角色【不想出現】的場景名稱。沒有列在這裡的場景，一律會顯示角色。")]
    public string[] scenesToHideCharacter;

    [Header("UI Parent")]
    [Tooltip("角色 UI 要掛在哪個 Canvas 或 Panel 底下。\n建議：這個物件本身也標成 DontDestroyOnLoad，當作 Global UI。")]
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

        // 確保有 UI Parent
        if (uiParent == null)
        {
            // 自動找場景中的第一個 Canvas 當父物件（RectTransform）
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                uiParent = canvas.transform as RectTransform;
            }
            else
            {
                Debug.LogError("❌ CharacterSceneSpawner：場景裡找不到 Canvas，也沒有指定 uiParent。");
                return;
            }
        }

        // 🔹 建議：這個 uiParent 最好是你專門做的一個 Global UI Panel
        //        並在它身上也呼叫 DontDestroyOnLoad，這樣角色 UI 才能跟著跨場景存在。
        //        如果你確定 uiParent 是專屬 Global UI，可以在這裡加上：
        //        DontDestroyOnLoad(uiParent.gameObject);

        // ✅ 只 Instantiate 一次，之後都只 SetActive 開關，不再 Destroy/重生
        spawnedCharacter = Instantiate(characterPrefab, uiParent, false);
        // 如果你也希望角色本身在 DontDestroyOnLoad 裡：
        // DontDestroyOnLoad(spawnedCharacter);
    }
}
