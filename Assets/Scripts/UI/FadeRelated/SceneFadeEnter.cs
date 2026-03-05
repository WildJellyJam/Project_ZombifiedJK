using UnityEngine;
using System.Collections;

public class SceneEnterFader : MonoBehaviour
{
    [Header("進場黑幕淡出")]
    [Tooltip("掛在全螢幕黑色 Image 上的 CanvasGroup")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("淡出時間（秒）")]
    public float fadeDuration = 1f;

    private void Awake()
    {
        // 確保一開始是黑的而且擋住點擊
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);

            if (!fadeCanvasGroup) yield break;

            // 1 -> 0
            fadeCanvasGroup.alpha = 1f - normalized;
            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.gameObject.SetActive(false); // 完全關掉黑幕
        }
    }
}
