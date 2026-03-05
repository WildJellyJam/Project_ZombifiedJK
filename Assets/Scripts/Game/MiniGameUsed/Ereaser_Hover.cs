using UnityEngine;

/// <summary>
/// Pushes the eraser with an impulse, spins based on velocity, and
/// shows a cursor-following UI indicator that fills while hovering.
/// Requires: Rigidbody2D + a Collider2D on this GameObject.
/// </summary>
public class Eraser_Hover : TimedMinigameBase
{
    [Header("Movement & Physics")]
    public float forceValue = 5f;            // initial X impulse
    public float rotateMultiplier = -5f;     // turns velocity.x into angularVelocity
    public float minBounceDotLimit = -0.2f;  // optional: guard for very shallow bounces

    [Header("Hover Win")]
    public float requiredHoverTime = 3f;     // seconds to win by hovering

    [Header("UI Indicator")]
    public HoverIndicatorUI indicator;       // assign the HoverIndicatorUI in Overlay Canvas
    public float indicatorDiameterPx = 120f;

    private float _hoverTime = 0f;
    private bool _isHovering = false;
    private float _prevProgress = 0f;

    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_rb)
        {
            _rb.AddForce(new Vector2(forceValue, 0f), ForceMode2D.Impulse);
            Debug.Log("[Eraser_Hover] Initial impulse added.");
        }

        // Prepare indicator
        if (!indicator && HoverIndicatorUI.Instance)
            indicator = HoverIndicatorUI.Instance;

        if (indicator)
        {
            indicator.SetTargetDiameter(indicatorDiameterPx);
            indicator.SetVisible(false, true);  // hide & reset
        }

        _prevProgress = 0f;
        _hoverTime = 0f;
        _isHovering = false;
    }

    private void FixedUpdate()
    {
        if (_rb)
        {
            _rb.angularVelocity = _rb.velocity.x * rotateMultiplier;
        }
    }

    // ✅ 關鍵：override + base.Update()，讓 timer 正常跑
    protected override void Update()
    {
        base.Update(); // 倒數計時 / sanity 倍速 / 超時輸

        // 1) Hover timing / win condition
        if (_isHovering)
        {
            _hoverTime += Time.deltaTime;
            float t = Mathf.Clamp01(_hoverTime / requiredHoverTime);

            // 2) Update indicator position + fill (cursor-following)
            UpdateIndicator(t);

            if (t - _prevProgress > 0.02f)
            {
                Debug.Log($"[Eraser_Hover] Indicator growing. progress={t:0.00}");
                _prevProgress = t;
            }

            if (_hoverTime >= requiredHoverTime)
            {
                Debug.Log("[Eraser_Hover] Hover complete → PLAYER WINS!");

                StopTimer();     // ✅ 贏了停 timer
                EndMinigame(true);

                _isHovering = false;
                _hoverTime = 0f;
                HideIndicator();
            }
        }
        else
        {
            HideIndicator();
            _prevProgress = 0f;
        }
    }

    // =========================
    // 下面這兩個函式：請保留你原本專案中的版本（不要用我猜的 API）
    // 如果你這支檔案原本就有，貼上後也會一起包含。
    // =========================

    private void UpdateIndicator(float progress01)
    {
        // ✅ 用你原本的實作（如果你原本這裡有完整內容，請把原本內容貼回來）
        if (!indicator) return;

        // TODO: 這裡請放你原本的 indicator 更新邏輯
        // 例如：indicator.SetProgress(progress01); / indicator.UpdateFill(progress01); 等等
        // 我不寫死方法名，避免再出現找不到方法的錯。
    }

    private void HideIndicator()
    {
        if (!indicator) return;
        indicator.SetVisible(false, false);
    }

    // Hover enter/exit 你原本用什麼方式觸發就留什麼（OnMouseEnter/Exit 或 Trigger）
    private void OnMouseEnter()
    {
        _isHovering = true;
        if (indicator) indicator.SetVisible(true, false);
    }

    private void OnMouseExit()
    {
        _isHovering = false;
        _hoverTime = 0f;
        HideIndicator();
    }
}