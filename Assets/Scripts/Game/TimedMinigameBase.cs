using UnityEngine;
using UnityEngine.UI;

public abstract class TimedMinigameBase : MinigameBase
{
    [Header("Timer")]
    public float timeLimitSeconds = 15f;
    public float warningSeconds = 5f;

    [Header("UI (顯示用)")]
    public Slider timerSlider;
    public GameObject warningGroup;

    [Header("Sanity Multiplier (Inspector 調整)")]
    public bool useSanityMultiplier = true;

    [Range(0f, 100f)]
    public float fallbackSanity = 100f;

    [Min(1f)]
    public float maxSpeedMultiplier = 3f;

    public AnimationCurve sanityToSpeedCurve = new AnimationCurve(
        new Keyframe(0f, 2f),
        new Keyframe(1f, 1f)
    );

    private float _timeLeft;
    private bool _running;

    protected virtual void OnEnable()
    {
        ResetTimer(timeLimitSeconds);
        _running = true;
    }

    public void SetTimeLimit(float seconds)
    {
        timeLimitSeconds = Mathf.Max(0.1f, seconds);
        ResetTimer(timeLimitSeconds);
    }

    public void StopTimer() => _running = false;

    private void ResetTimer(float seconds)
    {
        _timeLeft = seconds;
        if (warningGroup != null) warningGroup.SetActive(false);
        UpdateUI();
    }

    // ✅ 關鍵：要讓子類可呼叫 base.Update()
    protected virtual void Update()
    {
        if (!_running) return;

        float speedMult = 1f;

        if (useSanityMultiplier && GameManager.Instance != null)
        {
            float sanity = fallbackSanity;

            var f = GameManager.Instance.GetType().GetField("sanity");
            if (f != null && f.FieldType == typeof(float))
                sanity = (float)f.GetValue(GameManager.Instance);

            float sanity01 = Mathf.Clamp01(sanity / 100f);

            speedMult = sanityToSpeedCurve != null ? sanityToSpeedCurve.Evaluate(sanity01) : 1f;
            speedMult = Mathf.Clamp(speedMult, 1f, maxSpeedMultiplier);
        }

        _timeLeft -= Time.deltaTime * speedMult;

        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            UpdateUI();
            _running = false;
            EndMinigame(false);
            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (warningGroup != null)
            warningGroup.SetActive(_timeLeft <= warningSeconds);

        if (timerSlider != null)
        {
            float t = Mathf.Clamp01(_timeLeft / Mathf.Max(0.001f, timeLimitSeconds));
            timerSlider.value = t;
        }
    }
}