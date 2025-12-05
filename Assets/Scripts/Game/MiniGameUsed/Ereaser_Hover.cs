using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ereaser_Hover : MinigameBase
{
    [Header("Movement & Physics")]
    public float forceValue = 5f;          // 推動橡皮擦的初始力（X 方向）
    public float rotateMultiplier = -5f;   // 根據速度轉動的倍率

    [Header("Hover Win Settings")]
    public float requiredHoverTime = 3f;   // 滑鼠需要停留多久才算贏

    [Header("Hover Circle FX")]
    public Transform hoverCircle;          // 指向 child 物件 HoverCircle
    public float circleMinScale = 0.3f;
    public float circleMaxScale = 1.2f;
    public float circlePulseSpeed = 2f;    // 放大速度
    public float circleFadeOutSpeed = 5f;  // 離開時淡出的速度

    private float hoverTime = 0f;
    private bool isHovering = false;
    private Rigidbody2D rb;
    private SpriteRenderer hoverCircleRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 一開始給一個 X 方向的 impulse
            rb.AddForce(new Vector2(forceValue, 0f), ForceMode2D.Impulse);
            Debug.Log("Adding force to X value");
        }

        if (hoverCircle != null)
        {
            hoverCircleRenderer = hoverCircle.GetComponent<SpriteRenderer>();

            // 一開始把圓圈縮到最小 & 完全透明
            hoverCircle.localScale = Vector3.zero;
            if (hoverCircleRenderer != null)
            {
                var c = hoverCircleRenderer.color;
                c.a = 0f;
                hoverCircleRenderer.color = c;
            }
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // 讓橡皮擦依照 X 速度自動旋轉（像滾動一樣）
            rb.angularVelocity = rb.velocity.x * rotateMultiplier;
        }
    }

    void Update()
    {
        // Hover 計時 → 勝利判定
        if (isHovering)
        {
            hoverTime += Time.deltaTime;

            if (hoverTime >= requiredHoverTime)
            {
                Debug.Log("Hover complete → PLAYER WINS!");

                EndMinigame(true);   // ✅ 通知 Minigame Manager 玩家贏了

                isHovering = false;
                hoverTime = 0f;
            }
        }

        // 處理 hover 圓圈特效
        HandleHoverCircleFX();
    }

    private void HandleHoverCircleFX()
    {
        if (hoverCircle == null || hoverCircleRenderer == null)
            return;

        Color color = hoverCircleRenderer.color;

        if (isHovering)
        {
            // 讓圓圈在 min ~ max 之間放大縮小（PingPong）
            float t = Mathf.PingPong(Time.time * circlePulseSpeed, 1f);
            float scale = Mathf.Lerp(circleMinScale, circleMaxScale, t);
            hoverCircle.localScale = new Vector3(scale, scale, 1f);

            // 讓它有一點透明度（固定值或也可以跟 t 連動）
            color.a = Mathf.Lerp(color.a, 0.8f, Time.deltaTime * 10f);
            hoverCircleRenderer.color = color;
        }
        else
        {
            // 不在 hover 時，慢慢縮小 & 淡出
            hoverCircle.localScale = Vector3.Lerp(
                hoverCircle.localScale,
                Vector3.zero,
                Time.deltaTime * circleFadeOutSpeed
            );

            color.a = Mathf.Lerp(color.a, 0f, Time.deltaTime * circleFadeOutSpeed);
            hoverCircleRenderer.color = color;
        }
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        hoverTime = 0f;
        Debug.Log("Mouse entered eraser");
    }

    private void OnMouseExit()
    {
        isHovering = false;
        hoverTime = 0f;
        Debug.Log("Mouse exited eraser");
    }

    // （可選）如果你想用程式強化彈跳，可以解開註解
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return;

        Vector2 v = rb.velocity;
        if (collision.contactCount > 0)
        {
            Vector2 n = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(v, n);
            rb.velocity = reflected;
        }
    }
    */
}
