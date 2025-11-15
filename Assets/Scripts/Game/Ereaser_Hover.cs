using UnityEngine;

public class Ereaser_Hover : MinigameBase
{
    [Header("Movement Settings")]
    public float forceValue = 5f;
    public float rotateMultiplier = -5f;

    [Header("Hover Win Settings")]
    public float requiredHoverTime = 3f;
    public float decaySpeed = 1.5f; // time loss when not hovering

    private float hoverTime = 0f;
    private bool isHovering = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
            rb.AddForce(new Vector2(forceValue, forceValue * 2), ForceMode2D.Impulse);
    }

    void FixedUpdate()
    {
        if (rb)
            rb.angularVelocity = rb.velocity.x * rotateMultiplier;
    }

    void Update()
    {
        if (isHovering)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime >= requiredHoverTime)
            {
                Debug.Log("Hover complete → PLAYER WINS!");
                EndMinigame(true);
                hoverTime = 0f;
                isHovering = false;
            }
        }
        else
        {
            if (hoverTime > 0f)
            {
                hoverTime -= Time.deltaTime * decaySpeed;
                hoverTime = Mathf.Max(0f, hoverTime);
            }
        }

        // ✅ tell the cursor how much progress there is (for visual feedback)
        HoverCursor.Instance?.UpdateHoverProgress(hoverTime / requiredHoverTime);
    }

    private void OnMouseEnter()
    {
        isHovering = true;
    }

    private void OnMouseExit()
    {
        isHovering = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
    }
}
