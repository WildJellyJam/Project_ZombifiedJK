using UnityEngine;
using UnityEngine.UI;

public class IndicatorPulse : MonoBehaviour
{
    public float duration = 0.6f;
    public float startScale = 0.2f;
    public float endScale = 1.4f;
    public float startAlpha = 0.9f;
    public float endAlpha = 0f;

    private Image img;
    private float t;

    void Awake()
    {
        img = GetComponent<Image>();
        var rt = transform as RectTransform;
        rt.localScale = Vector3.one * startScale;

        var c = img.color;
        c.a = startAlpha;
        img.color = c;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);

        // scale
        float s = Mathf.Lerp(startScale, endScale, k);
        (transform as RectTransform).localScale = Vector3.one * s;

        // fade
        var c = img.color;
        c.a = Mathf.Lerp(startAlpha, endAlpha, k);
        img.color = c;

        if (k >= 1f) Destroy(gameObject);
    }
}
