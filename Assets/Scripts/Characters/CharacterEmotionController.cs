using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterEmotionController : MonoBehaviour
{
    [Header("Character Appearance")]
    public SpriteRenderer characterRenderer;      // ← your character uses SpriteRenderer
    public Sprite normalSprite;
    public Sprite sadSprite;
    public Sprite anxiousSprite;

    [Header("Shake Settings")]
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 0.25f;

    [Header("Scene Settings")]
    public string[] scenesToAppearIn;            // list of scenes this character should show up in

    private Vector3 originalPos;
    private int previousAnxiety;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        originalPos = transform.localPosition;
    }

    void Start()
    {
        previousAnxiety = GameManager.Instance.anxiety;
        UpdateEmotion(GameManager.Instance.anxiety);
    }

    void Update()
    {
        int anxiety = GameManager.Instance.anxiety;

        // If anxiety changed
        if (anxiety != previousAnxiety)
        {
            UpdateEmotion(anxiety);

            // If anxiety increased → shake character
            if (anxiety > previousAnxiety)
                StartCoroutine(Shake());

            previousAnxiety = anxiety;
        }
    }

    void UpdateEmotion(int anxiety)
    {
        if (anxiety < 30)
        {
            characterRenderer.sprite = normalSprite;
        }
        else if (anxiety < 70)
        {
            characterRenderer.sprite = sadSprite;
        }
        else
        {
            characterRenderer.sprite = anxiousSprite;
        }
    }

    IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;
            characterRenderer.transform.localPosition =
                originalPos + (Vector3)Random.insideUnitCircle * shakeIntensity;

            yield return null;
        }

        characterRenderer.transform.localPosition = originalPos;
    }

    // ---- Scene logic ----
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool shouldAppear = false;

        foreach (string sceneName in scenesToAppearIn)
        {
            if (scene.name == sceneName)
            {
                shouldAppear = true;
                break;
            }
        }

        gameObject.SetActive(shouldAppear);
    }
}
