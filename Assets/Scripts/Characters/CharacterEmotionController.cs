using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CharacterEmotionController : MonoBehaviour
{
    // --------- Singleton ---------
    public static CharacterEmotionController Instance;

    [Header("Character Appearance (UI Image)")]
    public Image characterImage;            // your character uses UI Image
    public Sprite normalSprite;
    public Sprite sadSprite;
    public Sprite anxiousSprite;

    [Header("Change Effect")]
    public Sprite effectSprite;             // sprite used when things get worse

    [Header("Comfort Effect")]
    public Sprite comfortSprite;            // sprite used when anxiety goes down

    [Header("Shake Settings")]
    public float shakeIntensity = 5f;       // UI uses pixels, so a bit bigger value
    public float shakeDuration = 0.25f;

    [Header("Scene Settings")]
    [Tooltip("Character will be visible in ALL scenes EXCEPT these.")]
    public string[] scenesToHideIn;         // only hide in these scenes

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;

    // previous values to detect changes
    private int previousAnxiety;
    private int previousSocialEnergy;
    private int previousSanity;

    // to prevent overlapping effects
    private Coroutine changeRoutine;

    void Awake()
    {
        // ---------- Singleton guard ----------
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalAnchoredPos = rectTransform.anchoredPosition;
        }

        // subscribe to sceneLoaded once for this persistent instance
        SceneManager.sceneLoaded += OnSceneLoaded;

        // also apply visibility for the current scene (when starting the game)
        ApplySceneVisibility(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // unsubscribe when this instance is really destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
        {
            previousAnxiety = Mathf.RoundToInt(newGameManager.Instance.playerStats.anxiety);
            previousSocialEnergy = Mathf.RoundToInt(newGameManager.Instance.playerStats.socialEnergy);
            previousSanity = Mathf.RoundToInt(newGameManager.Instance.playerStats.sanity);
        }
        else
        {
            previousAnxiety = 0;
            previousSocialEnergy = 0;
            previousSanity = 0;
        }

        // set initial emotion
        ApplyEmotionInstant(previousAnxiety, previousSocialEnergy, previousSanity);
    }

    void Update()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.playerStats == null)
            return;

        int anxiety = Mathf.RoundToInt(newGameManager.Instance.playerStats.anxiety);
        int socialEnergy = Mathf.RoundToInt(newGameManager.Instance.playerStats.socialEnergy);
        int sanity = Mathf.RoundToInt(newGameManager.Instance.playerStats.sanity);

        bool changed =
            anxiety != previousAnxiety ||
            socialEnergy != previousSocialEnergy ||
            sanity != previousSanity;

        if (changed)
        {
            bool anxietyIncreased = anxiety > previousAnxiety;
            bool anxietyDecreased = anxiety < previousAnxiety;

            bool gotWorse =
                anxietyIncreased ||
                socialEnergy < previousSocialEnergy ||
                sanity < previousSanity;

            previousAnxiety = anxiety;
            previousSocialEnergy = socialEnergy;
            previousSanity = sanity;

            if (changeRoutine != null)
            {
                StopCoroutine(changeRoutine);
            }

            changeRoutine = StartCoroutine(
                PlayChangeAnimation(anxiety, socialEnergy, sanity, gotWorse, anxietyDecreased)
            );
        }
    }

    // instantly set sprite from stats (used at Start)
    void ApplyEmotionInstant(int anxiety, int socialEnergy, int sanity)
    {
        if (characterImage == null) return;
        characterImage.sprite = GetEmotionSprite(anxiety, socialEnergy, sanity);
    }

    // decide which sprite to use from all 3 stats
    Sprite GetEmotionSprite(int anxiety, int socialEnergy, int sanity)
    {
        // tweak thresholds however you like

        // Very bad mental state: high anxiety or very low sanity
        if (anxiety >= 70 || sanity <= 30)
        {
            return anxiousSprite;
        }

        // Mid state: some anxiety or low social energy or mid sanity
        if (anxiety >= 30 || socialEnergy <= 40 || sanity <= 60)
        {
            return sadSprite;
        }

        // Otherwise: normal
        return normalSprite;
    }

    IEnumerator PlayChangeAnimation(
        int anxiety,
        int socialEnergy,
        int sanity,
        bool gotWorse,
        bool anxietyDecreased
    )
    {
        if (characterImage == null)
            yield break;

        Sprite finalSprite = GetEmotionSprite(anxiety, socialEnergy, sanity);

        // 1. Choose temporary sprite based on change type
        if (anxietyDecreased && comfortSprite != null)
        {
            // feeling better → comfort animation
            characterImage.sprite = comfortSprite;
        }
        else if (!anxietyDecreased && effectSprite != null)
        {
            // neutral or worse → effect sprite
            characterImage.sprite = effectSprite;
        }
        else
        {
            // fallback
            characterImage.sprite = finalSprite;
        }

        float timer = 0f;

        if (rectTransform != null)
        {
            if (gotWorse)
            {
                // 2a. If things got worse → do shake
                while (timer < shakeDuration)
                {
                    timer += Time.deltaTime;
                    rectTransform.anchoredPosition =
                        originalAnchoredPos + (Vector2)Random.insideUnitCircle * shakeIntensity;

                    yield return null;
                }
            }
            else
            {
                // 2b. If neutral or better → just hold the comfort/effect sprite briefly
                yield return new WaitForSeconds(0.15f);
            }

            // reset position
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
        }

        // 3. Switch to final emotion sprite based on new stats
        characterImage.sprite = finalSprite;

        changeRoutine = null;
    }

    // ---- Scene visibility logic ----
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneVisibility(scene.name);
    }

    private void ApplySceneVisibility(string sceneName)
    {
        // visible by default, only hide if listed in scenesToHideIn
        bool shouldBeVisible = true;

        if (scenesToHideIn != null)
        {
            foreach (string hideScene in scenesToHideIn)
            {
                if (!string.IsNullOrEmpty(hideScene) && sceneName == hideScene)
                {
                    shouldBeVisible = false;
                    break;
                }
            }
        }

        gameObject.SetActive(shouldBeVisible);
    }
}
