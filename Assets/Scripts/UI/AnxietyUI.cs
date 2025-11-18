using UnityEngine;
using TMPro; // 如果你用 TextMeshPro 就改成 using TMPro;
using System.Collections;

public class AnxietyUI : MonoBehaviour
{
    public TextMeshProUGUI anxietyText; // TextMeshPro用 TextMeshProUGUI
    public float pulseScale = 1.2f; // 放大倍率
    public float pulseSpeed = 0.15f; // 放大縮回速度

    private Vector3 originalScale;
    private float previousAnxiety;

    void Start()
    {
        originalScale = anxietyText.transform.localScale;
        previousAnxiety = (newGameManager.Instance != null && newGameManager.Instance.playerStats != null)
            ? newGameManager.Instance.playerStats.anxiety
            : 0f;
    }

    void Update()
    {
        if (newGameManager.Instance == null || newGameManager.Instance.playerStats == null) return;

        float currentAnxiety = newGameManager.Instance.playerStats.anxiety;
        anxietyText.text = Mathf.RoundToInt(currentAnxiety).ToString();

        // 如果焦慮值變動 → 播放跳動動畫
        if (Mathf.Abs(currentAnxiety - previousAnxiety) > 0.01f)
        {
            StopAllCoroutines(); // 確保不會多重播放
            StartCoroutine(PulseEffect());
            previousAnxiety = currentAnxiety;
        }
    }

    IEnumerator PulseEffect()
    {
        // 放大
        anxietyText.transform.localScale = originalScale * pulseScale;
        yield return new WaitForSeconds(pulseSpeed);

        // 回來
        anxietyText.transform.localScale = originalScale;
    }
}
