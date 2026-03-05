using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIButtonHoverFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale")]
    public Vector3 normalScale = new Vector3(0.7f,0.7f,1f);
    public Vector3 hoverScale = new Vector3(0.75f, 0.75f, 1f);
    public Vector3 pressedScale = new Vector3(0.68f, 0.68f, 1f);

    [Header("Optional: also tint an Image")]
    public UnityEngine.UI.Graphic targetGraphic; // drag your Image here if you want
    public Color normalColor = Color.white;
    public Color hoverColor = Color.white;
    public Color pressedColor = Color.white;

    bool _hovering;
    bool _pressing;

    void Reset()
    {
        // Auto-fill graphic if present
        targetGraphic = GetComponent<UnityEngine.UI.Graphic>();
    }

    void Awake()
    {
        ApplyNormal(force: true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hovering) return; // prevents retrigger spam
        _hovering = true;

        if (_pressing) ApplyPressed();
        else ApplyHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_hovering) return; // prevents retrigger spam
        _hovering = false;
        _pressing = false;

        ApplyNormal();

        // Clear selection so it won't "stick" after click
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressing = true;
        ApplyPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressing = false;
        if (_hovering) ApplyHover();
        else ApplyNormal();
    }

    void OnDisable()
    {
        // If button gets hidden/disabled while hovered, exit won't fire
        _hovering = false;
        _pressing = false;
        ApplyNormal(force: true);
    }

    void ApplyNormal(bool force = false)
    {
        if (force) transform.localScale = normalScale;
        else transform.localScale = normalScale;

        if (targetGraphic) targetGraphic.color = normalColor;
    }

    void ApplyHover()
    {
        transform.localScale = hoverScale;
        if (targetGraphic) targetGraphic.color = hoverColor;
    }

    void ApplyPressed()
    {
        transform.localScale = pressedScale;
        if (targetGraphic) targetGraphic.color = pressedColor;
    }
}
