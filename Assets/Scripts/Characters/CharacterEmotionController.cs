using UnityEngine;
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
    public float shakeIntensity = 5f;       // UI uses pixels
    public float shakeDuration = 0.25f;

    [Header("Change Motion")]
    [Tooltip("角色在觸發變化時，往下移動多少（UI 單位，負值代表往下）。")]
    public float changeYOffset = -10f;      // how much lower during effect

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

    // helper: set sprite + native size
    void SetSpriteWithNativeSize(Sprite sprite)
    {
        if (characterImage == null) return;

        characterImage.sprite = sprite;

        if (sprite != null)
        {
            characterImage.SetNativeSize();
        }
    }

    // instantly set sprite from stats (used at Start)
    void ApplyEmotionInstant(int anxiety, int socialEnergy, int sanity)
    {
        SetSpriteWithNativeSize(GetEmotionSprite(anxiety, socialEnergy, sanity));
    }

    // decide which sprite to use from all 3 stats
    Sprite GetEmotionSprite(int anxiety, int socialEnergy, int sanity)
    {
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
            SetSpriteWithNativeSize(comfortSprite);
        }
        else if (!anxietyDecreased && effectSprite != null)
        {
            // neutral or worse → effect sprite
            SetSpriteWithNativeSize(effectSprite);
        }
        else
        {
            // fallback
            SetSpriteWithNativeSize(finalSprite);
        }

        if (rectTransform != null)
        {
            float timer = 0f;

            // always move lower when the effect starts
            Vector2 loweredPos = originalAnchoredPos + new Vector2(0f, changeYOffset);
            rectTransform.anchoredPosition = loweredPos;

            if (gotWorse)
            {
                // shake around the lowered position
                while (timer < shakeDuration)
                {
                    timer += Time.deltaTime;
                    rectTransform.anchoredPosition =
                        loweredPos + (Vector2)Random.insideUnitCircle * shakeIntensity;

                    yield return null;
                }
            }
            else
            {
                // stay lowered briefly, no shake
                yield return new WaitForSeconds(0.15f);
            }

            // go back to original position
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
        }

        // 3. Switch to final emotion sprite based on new stats (also native size)
        SetSpriteWithNativeSize(finalSprite);

        changeRoutine = null;
    }
}
