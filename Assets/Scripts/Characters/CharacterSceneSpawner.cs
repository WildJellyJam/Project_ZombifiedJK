using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSceneSpawner : MonoBehaviour
{
    [Header("Character UI Settings")]
    [Tooltip("這裡放 UI 用的角色 Prefab（一定要是 RectTransform + Image/TMP 等 UI 元件，root 不要是 Canvas）")]
    public GameObject characterPrefab;

    [Tooltip("角色要出現的場景名字（場景名稱需與 Build Settings 裡的一致）")]
    public string[] scenesToShowCharacter;

    [Header("UI Parent")]
    [Tooltip("角色 UI 要掛在哪個 Canvas 或 Panel 底下。如果留空，會自動在場景中尋找第一個 Canvas。")]
    public RectTransform uiParent;

    private GameObject spawnedCharacter;

    private void Awake()
    {
        // 讓這個 Spawner 不會因為換場就消失
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldShow = false;

        // 檢查目前場景是否在顯示列表裡
        foreach (string sceneName in scenesToShowCharacter)
        {
            if (scene.name == sceneName)
            {
                shouldShow = true;
                break;
            }
        }

        if (shouldShow)
        {
            SpawnCharacterInScene();
        }
        else
        {
            // 這個場景不用顯示 → 把之前生成的 UI 刪掉
            if (spawnedCharacter != null)
            {
                Destroy(spawnedCharacter);
                spawnedCharacter = null;
            }
        }
    }

    private void SpawnCharacterInScene()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("❌ CharacterSceneSpawner：characterPrefab 沒有指定！（請指定 UI Prefab）");
            return;
        }

        // 先清掉舊的，避免重覆生
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
            spawnedCharacter = null;
        }

        // 如果沒手動指定 uiParent，就自動找場景中的 Canvas
        if (uiParent == null)
        {
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

        // ✅ 核心關鍵：只做這個動作，完全沿用 Prefab 的 RectTransform 設定
        spawnedCharacter = Instantiate(characterPrefab, uiParent, false);

        // ❌ 不再改 anchoredPosition / localScale / sizeDelta
        // 讓 UI 完全照 Prefab 當時的設定長出來
    }
}
