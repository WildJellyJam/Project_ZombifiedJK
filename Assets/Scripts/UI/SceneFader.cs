using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Fade UI")]
    public CanvasGroup fadeCanvasGroup;   // assign FadePanel's CanvasGroup
    public float fadeDuration = 1f;

    private bool isFading = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start with panel hidden
        HidePanelInstant();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called every time a *new* scene finishes loading
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // After loading, fade IN from black
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Call this instead of SceneManager.LoadScene
    /// This gives you a fade OUT on exit.
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // ===================== Core Fade Coroutines =====================

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        if (fadeCanvasGroup == null)
        {
            // fallback: just load scene with no fade
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        isFading = true;

        // prepare panel
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.alpha = 0f;

        // FADE OUT (current scene ¡÷ black)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            fadeCanvasGroup.alpha = normalized;       // 0 ¡÷ 1
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        // Now fully black ¡÷ load next scene
        SceneManager.LoadScene(sceneName);
        // FadeIn will be started automatically in OnSceneLoaded
    }

    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null)
        {
            isFading = false;
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.alpha = 1f;

        // FADE IN (black ¡÷ new scene)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            fadeCanvasGroup.alpha = 1f - normalized;  // 1 ¡÷ 0
            yield return null;
        }

        HidePanelInstant();
        isFading = false;
    }

    // ===================== Helper =====================

    private void HidePanelInstant()
    {
        if (fadeCanvasGroup == null) return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false); // fully hidden
    }
}
