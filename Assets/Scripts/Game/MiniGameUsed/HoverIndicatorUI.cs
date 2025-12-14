using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cursor-following hover indicator for Screen Space Overlay (also supports Camera/World space).
/// Shows a base circle (half opacity) and a progress circle that scales up linearly (0..1).
/// </summary>
public class HoverIndicatorUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform root;           // container (this by default)
    public RectTransform baseCircle;     // the bottom, constant-size circle
    public RectTransform progressCircle; // the top, scaling circle
    public Image baseImage;              // optional; auto-grab if null
    public Image progressImage;          // optional; auto-grab if null

    [Header("Appearance")]
    [Tooltip("Final diameter (in pixels) of the base circle when fully shown.")]
    public float targetDiameterPx = 120f;
    [Range(0f, 1f)]
    public float baseAlpha = 0.5f;
    [Range(0f, 1f)]
    public float progressAlpha = 0.9f;

    // Singleton if you want to access it from gameplay easily
    public static HoverIndicatorUI Instance { get; private set; }

    Canvas _canvas;
    Camera _uiCam;
    float _progress01 = 0f;
    bool _visible = false;

    void Awake()
    {
        // Singleton (safe but simple)
        Instance = this;

        if (!root) root = transform as RectTransform;
        if (!baseImage && baseCircle) baseImage = baseCircle.GetComponent<Image>();
        if (!progressImage && progressCircle) progressImage = progressCircle.GetComponent<Image>();

        _canvas = GetComponentInParent<Canvas>();
        _uiCam = (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;

        // Ensure initial sizing & hidden state
        ApplyTargetDiameter(targetDiameterPx);
        ApplyAlphas();
        SetVisible(false);
        SetProgress(0f);
    }

    void OnEnable()
    {
        // Make sure size/alpha survive domain reloads or prefab enables
        ApplyTargetDiameter(targetDiameterPx);
        ApplyAlphas();
    }

    // --- Public API ---

    /// <summary> Show/hide indicator. </summary>
    public void SetVisible(bool on)
    {
        _visible = on;
        if (root) root.gameObject.SetActive(on);
    }

    /// <summary> Overload kept for backward compatibility: SetVisible(on, resetFill). </summary>
    public void SetVisible(bool on, bool resetFill)
    {
        SetVisible(on);
        if (resetFill) ResetFill();
    }

    /// <summary> Immediately resets progress circle to zero size. </summary>
    public void ResetFill()
    {
        SetProgress(0f);
    }

    /// <summary> Set the base circle diameter (pixels). Progress circle will grow toward this. </summary>
    public void SetTargetDiameter(float diameterPx)
    {
        targetDiameterPx = Mathf.Max(0f, diameterPx);
        ApplyTargetDiameter(targetDiameterPx);
    }

    /// <summary> Set progress 0..1. Top circle scales linearly toward base circle size. </summary>
    public void SetProgress(float t01)
    {
        _progress01 = Mathf.Clamp01(t01);
        if (!baseCircle || !progressCircle) return;

        // base circle stays at target size; progress scales from 0 to full
        Vector2 target = Vector2.one * targetDiameterPx;
        baseCircle.sizeDelta = target;
        progressCircle.sizeDelta = target * _progress01;

        // ensure images visible with desired opacity
        ApplyAlphas();
    }

    /// <summary>
    /// Position the indicator at a screen-space pixel position.
    /// Works for Overlay and Camera/World-space canvases.
    /// </summary>
    public void MoveToScreenPosition(Vector2 screenPos)
    {
        if (!_canvas || !root) return;

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // In Overlay, RectTransform.position expects screen pixels
            root.position = screenPos;
        }
        else
        {
            RectTransform canvasRect = _canvas.transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, _uiCam, out var local))
                root.localPosition = local;
        }
    }

    // --- Internal helpers ---

    void ApplyTargetDiameter(float d)
    {
        if (baseCircle) baseCircle.sizeDelta = new Vector2(d, d);
        if (progressCircle) progressCircle.sizeDelta = new Vector2(d * _progress01, d * _progress01);
    }

    void ApplyAlphas()
    {
        if (baseImage)
        {
            var c = baseImage.color; c.a = baseAlpha; baseImage.color = c;
        }
        if (progressImage)
        {
            var c = progressImage.color; c.a = progressAlpha; progressImage.color = c;
        }
    }
}
