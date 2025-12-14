using UnityEngine;

/// <summary>
/// Pushes the eraser with an impulse, spins based on velocity, and
/// shows a cursor-following UI indicator that fills while hovering.
/// Requires: Rigidbody2D + a Collider2D on this GameObject.
/// </summary>
public class Ereaser_Hover : MinigameBase
{
    [Header("Movement & Physics")]
    public float forceValue = 5f;          // initial X impulse
    public float rotateMultiplier = -5f;   // turns velocity.x into angularVelocity
    public float minBounceDotLimit = -0.2f; // optional: guard for very shallow bounces

    [Header("Hover Win")]
    public float requiredHoverTime = 3f;   // seconds to win by hovering

    [Header("UI Indicator")]
    public HoverIndicatorUI indicator;     // assign the HoverIndicatorUI in Overlay Canvas
    public float indicatorDiameterPx = 120f;

    float _hoverTime = 0f;
    bool _isHovering = false;
    float _prevProgress = 0f;

    Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_rb)
        {
            // Give an initial impulse so it moves.
            _rb.AddForce(new Vector2(forceValue, 0f), ForceMode2D.Impulse);
            // Let physics rotate it (make sure Freeze Rotation Z is NOT checked).
            Debug.Log("[Ereaser_Hover] Initial impulse added.");
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
    }

    void FixedUpdate()
    {
        if (_rb)
        {
            // Make it spin like a rolling object based on horizontal speed
            _rb.angularVelocity = _rb.velocity.x * rotateMultiplier;
        }
    }

    void Update()
    {
        // 1) Hover timing / win condition
        if (_isHovering)
        {
            _hoverTime += Time.deltaTime;
            float t = Mathf.Clamp01(_hoverTime / requiredHoverTime);

            // 2) Update indicator position + fill (cursor-following)
            UpdateIndicator(t);

            // Debug only when the circle actually grows (avoid spam)
            if (t - _prevProgress > 0.02f)
            {
                Debug.Log($"[Ereaser_Hover] Indicator growing. progress={t:0.00}");
                _prevProgress = t;
            }

            if (_hoverTime >= requiredHoverTime)
            {
                Debug.Log("[Ereaser_Hover] Hover complete → PLAYER WINS!");
                EndMinigame(true);

                _isHovering = false;
                _hoverTime = 0f;
                HideIndicator();
            }
        }
        else
        {
            // Not hovering → keep indicator hidden but follow cursor if you prefer (optional)
            HideIndicator();
            _prevProgress = 0f;
        }
    }

    // --- Indicator control ---

    void UpdateIndicator(float progress01)
    {
        if (!indicator) return;

        // show & follow mouse every frame while hovering
        indicator.SetVisible(true);
        indicator.SetTargetDiameter(indicatorDiameterPx);
        indicator.MoveToScreenPosition(Input.mousePosition);
        indicator.SetProgress(progress01);
    }

    void HideIndicator()
    {
        if (!indicator) return;
        indicator.SetVisible(false, true); // hide and reset fill
    }

    // --- Mouse events on the eraser object ---

    void OnMouseEnter()
    {
        _isHovering = true;
        _hoverTime = 0f;
        _prevProgress = 0f;
        if (indicator)
        {
            indicator.SetVisible(true, true); // show + reset
            indicator.MoveToScreenPosition(Input.mousePosition);
        }
        Debug.Log("[Ereaser_Hover] Mouse entered eraser.");
    }

    void OnMouseExit()
    {
        _isHovering = false;
        _hoverTime = 0f;
        HideIndicator();
        Debug.Log("[Ereaser_Hover] Mouse exited eraser.");
    }

    // --- Optional bounce helper if you feel it's not bouncing enough on the ground ---
    // Use this if you don't want to rely solely on a PhysicsMaterial2D.
    // Make sure your ground has a Collider2D.
    void OnCollisionEnter2D(Collision2D col)
    {
        if (_rb == null || col.contactCount == 0) return;

        // If you want strict physics bounciness, prefer assigning a PhysicsMaterial2D with Bounciness > 0.
        // This code just ensures we don't get "sticky" stops on flat ground.
        Vector2 v = _rb.velocity;
        Vector2 n = col.contacts[0].normal;

        // Only reflect if we're meaningfully hitting the surface.
        if (Vector2.Dot(v.normalized, -n) > -minBounceDotLimit)
        {
            Vector2 reflected = Vector2.Reflect(v, n);
            _rb.velocity = reflected;
        }
    }
}
