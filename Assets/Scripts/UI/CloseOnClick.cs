using System.Collections;
using UnityEngine;

public class CloseOnClick : MonoBehaviour
{
    [Header("Target panel (your black screen)")]
    public GameObject panelToClose;

    [Header("Fade settings")]
    public float fadeDuration = 0.5f;

    private bool isFading = false;

    public void ClosePanel()
    {
        if (!isFading && panelToClose != null)
        {
            StartCoroutine(FadeAndClose());
        }
    }

    private IEnumerator FadeAndClose()
    {
        isFading = true;

        // Get or add CanvasGroup on the panel
        var canvasGroup = panelToClose.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelToClose.AddComponent<CanvasGroup>();
        }

        // Make sure it's visible at start
        canvasGroup.alpha = 1f;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // use unscaled if this is like a pause screen
            float lerp = t / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, lerp);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panelToClose.SetActive(false);
        isFading = false;
    }
}