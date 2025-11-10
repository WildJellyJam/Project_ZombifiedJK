using UnityEngine;

public class HoverCursor : MonoBehaviour
{
    [Header("Cursor Sprites")]
    public Sprite defaultCursorSprite;      // normal cursor (e.g. hand)
    public Sprite objectiveCursorSprite;    // hover sprite
    public string objectiveTag = "Objective";

    [Header("Cursor Settings")]
    public Color cursorColor = Color.white;
    public float cursorScale = 0.3f;
    public float followSpeed = 20f;

    private SpriteRenderer sr;
    private Camera mainCam;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // keep cursor between scenes
    }

    void Start()
    {
        mainCam = Camera.main;
        Cursor.visible = false; // hide system cursor

        GameObject cursorVisual = new GameObject("CustomCursor");
        sr = cursorVisual.AddComponent<SpriteRenderer>();
        sr.sprite = defaultCursorSprite;
        sr.color = cursorColor;
        sr.transform.localScale = Vector3.one * cursorScale;
        sr.sortingOrder = 1000;
    }

    void Update()
    {
        if (mainCam == null) mainCam = Camera.main;

        // follow mouse
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        sr.transform.position = Vector3.Lerp(sr.transform.position, mousePos, Time.deltaTime * followSpeed);

        // detect object under cursor
        CheckHoverObject();
    }

    void CheckHoverObject()
    {
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // if cursor is over an Objective
        if (hit.collider != null && hit.collider.CompareTag(objectiveTag))
        {
            if (sr.sprite != objectiveCursorSprite)
                sr.sprite = objectiveCursorSprite;
        }
        else
        {
            // return to default sprite when not on Objective
            if (sr.sprite != defaultCursorSprite)
                sr.sprite = defaultCursorSprite;
        }
    }

    private void OnDestroy()
    {
        Cursor.visible = true; // restore system cursor
    }
}
