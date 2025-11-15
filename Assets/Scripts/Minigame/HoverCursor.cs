using UnityEngine;

public class HoverCursor : MonoBehaviour
{
    public static HoverCursor Instance; // global reference

    [Header("Cursor Sprites")]
    public Sprite defaultCursorSprite;
    public Sprite objectiveCursorSprite;
    public string objectiveTag = "Objective";

    [Header("Cursor Circle Visuals")]
    public Sprite circleSprite;
    public Color circleColor = new Color(1f, 1f, 1f, 0.4f);
    public float minScale = 0.3f;
    public float maxScale = 1.2f;
    public float scaleSmooth = 10f;

    [Header("Cursor Settings")]
    public Color cursorColor = Color.white;
    public float cursorScale = 0.3f;
    public float followSpeed = 20f;

    private SpriteRenderer sr;
    private SpriteRenderer circleSR;
    private GameObject circleObj;
    private Camera mainCam;
    private float hoverProgress = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        mainCam = Camera.main;
        Cursor.visible = false;

        // create main cursor
        GameObject cursorVisual = new GameObject("CustomCursor");
        sr = cursorVisual.AddComponent<SpriteRenderer>();
        sr.sprite = defaultCursorSprite;
        sr.color = cursorColor;
        sr.transform.localScale = Vector3.one * cursorScale;
        sr.sortingOrder = 1000;

        // create hover circle as child
        if (circleSprite != null)
        {
            circleObj = new GameObject("CursorCircle");
            circleObj.transform.SetParent(cursorVisual.transform, false);
            circleSR = circleObj.AddComponent<SpriteRenderer>();
            circleSR.sprite = circleSprite;
            circleSR.color = circleColor;
            circleSR.sortingOrder = 999;
            circleObj.transform.localScale = Vector3.one * minScale;
        }
    }

    void Update()
    {
        if (mainCam == null) mainCam = Camera.main;
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        sr.transform.position = Vector3.Lerp(sr.transform.position, mousePos, Time.deltaTime * followSpeed);

        CheckHoverObject();
        UpdateCircleVisual();
    }

    void CheckHoverObject()
    {
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag(objectiveTag))
        {
            if (sr.sprite != objectiveCursorSprite)
                sr.sprite = objectiveCursorSprite;
        }
        else
        {
            if (sr.sprite != defaultCursorSprite)
                sr.sprite = defaultCursorSprite;
            hoverProgress = 0f; // reset circle when leaving objective
        }
    }

    // called by Ereaser_Hover to update hover progress (0¡V1)
    public void UpdateHoverProgress(float progress)
    {
        hoverProgress = Mathf.Clamp01(progress);
    }

    void UpdateCircleVisual()
    {
        if (!circleObj) return;

        float targetScale = Mathf.Lerp(minScale, maxScale, hoverProgress);
        circleObj.transform.localScale = Vector3.Lerp(
            circleObj.transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleSmooth);

        // optional: fade as it grows
        float alpha = Mathf.Lerp(circleColor.a, 0f, hoverProgress);
        circleSR.color = new Color(circleColor.r, circleColor.g, circleColor.b, alpha);
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }
}
