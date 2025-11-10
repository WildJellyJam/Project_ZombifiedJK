using UnityEngine;

public class Ereaser_Hover : MinigameBase
{
    [Header("Movement Settings")]
    public float forceValue = 5f;          // Initial push force
    public float rotateMultiplier = -5f;   // Rotation relative to movement

    [Header("Hover Win Settings")]
    public float requiredHoverTime = 3f;   // How long to hover to win
    private float hoverTime = 0f;
    private bool isHovering = false;

    private Rigidbody2D rb;

    [Header("Hover Circle Visuals")]
    public Sprite circleSprite;                    // 指定一個白色圓形 Sprite（在 Inspector 指定）
    public Color baseCircleColor = new Color(1f, 1f, 1f, 0.2f); // 大圈顏色（較透明）
    public float baseCircleScale = 2.0f;           // 大圈固定尺度（相對於物件）
    public Color pulseCircleColor = new Color(1f, 1f, 1f, 0.6f); // 脈衝圈起始顏色（較不透明）
    public float pulseStartScale = 1.0f;           // 脈衝圈從這個倍數開始
    public float pulseMaxScale = 3.0f;             // 脈衝圈到達最大倍數時重製
    public float pulseDuration = 1.0f;             // 一次脈衝時間（秒）
    public float transitionSpeed = 8f;             // 圖示跟著物件位置/縮放平滑度（通常不用動）

    private GameObject baseCircle;
    private GameObject pulseCircle;
    private SpriteRenderer baseSR;
    private SpriteRenderer pulseSR;
    private Vector3 originalScale;
    private float pulseTimer = 0f;
    private int baseSortingOrder = 0;

    void Start()        
    {
        rb = GetComponent<Rigidbody2D>();

        // Push object to start moving
        if (rb != null)
        {
            rb.AddForce(new Vector2(forceValue, forceValue * 2), ForceMode2D.Impulse);
        }

        originalScale = transform.localScale;

        // 嘗試取得此物件的 SpriteRenderer 排序順序，讓圓圈置於下方或上方可微調
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseSortingOrder = sr.sortingOrder - 1;
        else baseSortingOrder = 0;

        CreateHoverCircles();
        SetHoverVisualsActive(false);
    }

    void FixedUpdate()
    {
        // Rotate based on how fast it's moving
        if (rb != null)
        {
            rb.angularVelocity = rb.velocity.x * rotateMultiplier;
        }
    }

    void Update()
    {
        // 平滑處理：維持圓圈在物件位置（因為是子物件，已跟著）
        // 脈衝邏輯
        if (pulseCircle != null && pulseCircle.activeSelf)
        {
            pulseTimer += Time.deltaTime;
            float t = pulseTimer / Mathf.Max(0.0001f, pulseDuration);
            // 由 startScale 緩動到 maxScale
            float scale = Mathf.Lerp(pulseStartScale, pulseMaxScale, t);
            pulseCircle.transform.localScale = Vector3.Lerp(pulseCircle.transform.localScale, originalScale * scale, Time.deltaTime * transitionSpeed);

            // 透明度由起始到 0（越大越透明）
            Color c = pulseSR.color;
            float alpha = Mathf.Lerp(pulseCircleColor.a, 0f, t);
            pulseSR.color = new Color(pulseCircleColor.r, pulseCircleColor.g, pulseCircleColor.b, alpha);

            if (t >= 1f)
            {
                // 重新開始下一次脈衝
                pulseTimer = 0f;
                // 立即重置脈衝圈回起始大小與透明度
                pulseCircle.transform.localScale = originalScale * pulseStartScale;
                pulseSR.color = pulseCircleColor;
            }
        }

        // Hover → Count time → Win
        if (isHovering)
        {
            hoverTime += Time.deltaTime;

            if (hoverTime >= requiredHoverTime)
            {
                Debug.Log("Hover complete → PLAYER WINS!");
                EndMinigame(true);     // Tell MinigameManager game is won
                isHovering = false;
                hoverTime = 0f;
                SetHoverVisualsActive(false);
            }
        }
    }

    // Mouse hover detection
    private void OnMouseEnter()
    {
        isHovering = true;
        hoverTime = 0f;
        pulseTimer = 0f;
        // 啟動視覺
        SetHoverVisualsActive(true);
        // 立即設定脈衝初始大小/顏色
        if (pulseCircle != null) pulseCircle.transform.localScale = originalScale * pulseStartScale;
        if (pulseSR != null) pulseSR.color = pulseCircleColor;
    }

    private void OnMouseExit()
    {
        isHovering = false;
        hoverTime = 0f;
        pulseTimer = 0f;
        SetHoverVisualsActive(false);
    }

    // Collision bounce
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
        }
    }

    // 建立兩個圓形子物件（大底圈與脈衝圈）
    private void CreateHoverCircles()
    {
        if (circleSprite == null)
        {
            Debug.LogWarning($"[{nameof(Ereaser_Hover)}] circleSprite 未設定，無法顯示圓形視覺。請在 Inspector 指定一個白色圓形 Sprite。");
            return;
        }

        // base circle - 固定大圈（較透明）
        baseCircle = new GameObject("HoverBaseCircle");
        baseCircle.transform.SetParent(transform, false);
        baseCircle.transform.localPosition = Vector3.zero;
        baseSR = baseCircle.AddComponent<SpriteRenderer>();
        baseSR.sprite = circleSprite;
        baseSR.color = baseCircleColor;
        baseCircle.transform.localScale = originalScale * baseCircleScale;
        baseSR.sortingOrder = baseSortingOrder;

        // pulse circle - 會放大並淡出
        pulseCircle = new GameObject("HoverPulseCircle");
        pulseCircle.transform.SetParent(transform, false);
        pulseCircle.transform.localPosition = Vector3.zero;
        pulseSR = pulseCircle.AddComponent<SpriteRenderer>();
        pulseSR.sprite = circleSprite;
        pulseSR.color = pulseCircleColor;
        pulseCircle.transform.localScale = originalScale * pulseStartScale;
        pulseSR.sortingOrder = baseSortingOrder + 1;
    }

    private void SetHoverVisualsActive(bool active)
    {
        if (baseCircle != null) baseCircle.SetActive(active);
        if (pulseCircle != null) pulseCircle.SetActive(active);
    }
}
