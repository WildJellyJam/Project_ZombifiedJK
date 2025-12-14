using UnityEngine;

public class FocusAnwser_Drag : MinigameBase
{
    [Header("Drag Settings")]
    public Camera mainCamera;
    [Tooltip("How fast the background follows the (drunk) target position")]
    public float followSpeed = 15f;
    public bool lockZ = true;

    [Header("Anxiety Wobble Settings")]
    [Range(0f, 2f)]
    [Tooltip("0 = calm, higher = more shaky")]
    public float anxietyIntensity = 0.5f;
    [Tooltip("How fast the wobble noise moves")]
    public float anxietyNoiseSpeed = 2f;
    [Tooltip("World-space wobble distance at intensity=1")]
    public float wobbleDistance = 0.5f;

    [Header("Circle Overlap Settings")]
    [Tooltip("Invisible circle at screen center (auto-created under MainCamera)")]
    public Transform screenCircle;
    public float screenCircleRadius = 1.0f;

    [Tooltip("Goal circle somewhere on the background image")]
    public Transform targetCircle;
    public float targetCircleRadius = 1.0f;

    [Tooltip("How many seconds circles must overlap to win")]
    public float requiredOverlapTime = 2f;

    [Header("Effects Distance Mapping")]
    [Tooltip("Distance at which effects are at 'max' strength")]
    public float maxEffectDistance = 10f;

    [Header("Pixelation Effect (Optional: low → high quality)")]
    public PixelationController pixelation;  // safe if null

    [Header("Vignette Effect (Optional)")]
    [Tooltip("CanvasGroup of a fullscreen vignette image (dark corners)")]
    public CanvasGroup vignetteOverlay;
    [Tooltip("How fast vignette alpha changes")]
    public float vignetteFadeSpeed = 5f;
    [Range(0f, 1f)]
    [Tooltip("Vignette alpha when exactly on the goal")]
    public float vignetteAlphaAtGoal = 0.1f;
    [Range(0f, 1f)]
    [Tooltip("Vignette alpha when very far (panic)")]
    public float vignetteAlphaAtFar = 0.9f;

    [Header("Nausea Camera Effect (Optional)")]
    public NauseaCameraEffect nauseaEffect;  // safe if null
    [Range(0f, 1f)]
    [Tooltip("Nausea when exactly on target (0 = calm)")]
    public float nauseaAtGoal = 0.2f;
    [Range(0f, 1f)]
    [Tooltip("Nausea when very far from target")]
    public float nauseaAtFar = 0.9f;

    [Header("Drag Bounds")]
    [Tooltip("Enable to clamp the background position inside a rectangle")]
    public bool useDragBounds = true;
    [Tooltip("World-space bottom-left of allowed area")]
    public Vector2 boundsMin = new Vector2(-10f, -5f);
    [Tooltip("World-space top-right of allowed area")]
    public Vector2 boundsMax = new Vector2(10f, 5f);

    // ---- runtime state ----
    private bool isDragging = false;
    private Vector3 dragOffset;
    private bool hasSucceeded = false;

    private float overlapTimer = 0f;
    private bool wasOverlappingLastFrame = false;
    private bool isOverlappingNow = false;

    // For effects: latest distance from screenCircle to targetCircle
    private float lastDistanceToTarget = Mathf.Infinity;

    private void Awake()
    {
        // 1. Find main camera
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("[FocusAnwser_Drag] No camera assigned and no MainCamera found!");
            return;
        }

        // 2. Auto-setup screenCircle
        if (screenCircle == null)
        {
            // Try to find existing ScreenCircle under camera
            Transform found = mainCamera.transform.Find("ScreenCircle");
            if (found != null)
            {
                screenCircle = found;
            }
            else
            {
                // Create a new one at screen center on same Z as background
                GameObject go = new GameObject("ScreenCircle");
                go.transform.SetParent(mainCamera.transform);

                float bgZ = transform.position.z;
                float camZ = mainCamera.transform.position.z;
                float localZ = bgZ - camZ;

                go.transform.localPosition = new Vector3(0f, 0f, localZ);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                screenCircle = go.transform;
            }
        }

        // 3. Auto-get nausea effect if not set
        if (nauseaEffect == null)
        {
            nauseaEffect = mainCamera.GetComponent<NauseaCameraEffect>();
        }

        if (targetCircle == null)
        {
            Debug.LogWarning("[FocusAnwser_Drag] targetCircle is NULL. Assign a target circle on the background.");
        }
    }

    private void OnEnable()
    {
        // Enable nausea when this minigame is active
        if (nauseaEffect != null)
        {
            nauseaEffect.SetEnabled(true);
            nauseaEffect.SetIntensity(0f); // start calm-ish
        }
    }

    private void OnDisable()
    {
        // Turn off nausea when minigame closes / prefab disabled
        if (nauseaEffect != null)
        {
            nauseaEffect.SetEnabled(false);
        }
    }

    private void OnMouseDown()
    {
        if (hasSucceeded) return;
        if (mainCamera == null) return;

        isDragging = true;

        Vector3 mouseWorld = GetMouseWorldPos();
        dragOffset = transform.position - mouseWorld;

        Debug.Log("[FocusAnwser_Drag] OnMouseDown → start dragging background.");
    }

    private void OnMouseUp()
    {
        isDragging = false;
        Debug.Log("[FocusAnwser_Drag] OnMouseUp → stop dragging background.");
    }

    private void Update()
    {
        HandleDrag();
        HandleCirclesAndDistance();
        HandleScreenEffects();
    }

    // ----- 1. Drag + always-on wobble + bounds -----
    private void HandleDrag()
    {
        if (hasSucceeded || mainCamera == null) return;

        // Base target = current position (so it wobbles in place)
        Vector3 baseTarget = transform.position;

        // If dragging, move base target with mouse
        if (isDragging)
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            baseTarget = mouseWorld + dragOffset;
        }

        // Wobble (always, even when not dragging)
        float noiseX = (Mathf.PerlinNoise(Time.time * anxietyNoiseSpeed, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, Time.time * anxietyNoiseSpeed) - 0.5f) * 2f;
        Vector3 anxietyOffset = new Vector3(noiseX, noiseY, 0f) * anxietyIntensity * wobbleDistance;

        Vector3 targetPos = baseTarget + anxietyOffset;

        if (lockZ)
            targetPos.z = transform.position.z;

        // Clamp inside bounds
        if (useDragBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, boundsMin.x, boundsMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, boundsMin.y, boundsMax.y);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }

    // ----- 2. Circles, overlap timer, win, distance -----
    private void HandleCirclesAndDistance()
    {
        if (hasSucceeded)
        {
            // lock effects at goal
            lastDistanceToTarget = 0f;
            return;
        }

        if (screenCircle == null || targetCircle == null)
        {
            // If not set up, treat as far → max effects
            lastDistanceToTarget = maxEffectDistance;
            isOverlappingNow = false;
            return;
        }

        float dist = Vector2.Distance(screenCircle.position, targetCircle.position);
        float combinedRadius = screenCircleRadius + targetCircleRadius;

        lastDistanceToTarget = dist;
        isOverlappingNow = dist <= combinedRadius;

        if (isOverlappingNow)
        {
            if (!wasOverlappingLastFrame)
                Debug.Log($"[FocusAnwser_Drag] Circles started overlapping. Dist = {dist:F3}");

            overlapTimer += Time.deltaTime;

            if (overlapTimer >= requiredOverlapTime)
            {
                hasSucceeded = true;
                isDragging = false;

                Debug.Log($"[FocusAnwser_Drag] Overlap complete → PLAYER WINS! OverlapTime = {overlapTimer:F2}s");

                // Same as Ereaser_Hover: notify minigame system
                EndMinigame(true);
            }
        }
        else
        {
            if (wasOverlappingLastFrame)
                Debug.Log($"[FocusAnwser_Drag] Circles stopped overlapping. Reset timer. LastOverlapTime = {overlapTimer:F2}s");

            overlapTimer = 0f;
        }

        wasOverlappingLastFrame = isOverlappingNow;
    }

    // ----- 3. Pixelation + vignette + nausea from distance -----
    private void HandleScreenEffects()
    {
        // Distance → 0..1
        float d = Mathf.Clamp(lastDistanceToTarget, 0f, maxEffectDistance);
        float t = Mathf.InverseLerp(0f, maxEffectDistance, d);
        // t = 0 → exactly on goal (clear / calm)
        // t = 1 → far (max effect)

        // Pixelation (low quality far → clear near)
        if (pixelation != null)
        {
            pixelation.SetIntensity(t); // safe if its material is null
        }

        // Vignette (dark corners far → light near)
        if (vignetteOverlay != null)
        {
            float targetVignetteAlpha = Mathf.Lerp(vignetteAlphaAtGoal, vignetteAlphaAtFar, t);
            vignetteOverlay.alpha = Mathf.Lerp(
                vignetteOverlay.alpha,
                targetVignetteAlpha,
                vignetteFadeSpeed * Time.deltaTime
            );
        }

        // Nausea (strong far → calm near)
        if (nauseaEffect != null)
        {
            float nausea = Mathf.Lerp(nauseaAtGoal, nauseaAtFar, t);
            nauseaEffect.SetIntensity(nausea);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        float z = Mathf.Abs(mainCamera.WorldToScreenPoint(transform.position).z);
        mouse.z = z;
        return mainCamera.ScreenToWorldPoint(mouse);
    }

    private void OnDrawGizmosSelected()
    {
        if (screenCircle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(screenCircle.position, screenCircleRadius);
        }

        if (targetCircle != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetCircle.position, targetCircleRadius);
        }

        if (useDragBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (boundsMin.x + boundsMax.x) * 0.5f,
                (boundsMin.y + boundsMax.y) * 0.5f,
                (Application.isPlaying ? transform.position.z : 0f)
            );
            Vector3 size = new Vector3(
                Mathf.Abs(boundsMax.x - boundsMin.x),
                Mathf.Abs(boundsMax.y - boundsMin.y),
                0f
            );
#if UNITY_EDITOR
            UnityEditor.Handles.DrawWireCube(center, size);
#else
            Gizmos.DrawWireCube(center, size);
#endif
        }
    }
}
