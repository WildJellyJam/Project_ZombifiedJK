using UnityEngine;
using UnityEngine.UI;

public class SceneFadeButtonTrigger : MonoBehaviour
{
    [Header("Target Scene")]
    public string targetSceneName;

    [Header("Message")]
    [TextArea]
    public string message = "……";

    [Header("Timing")]
    public float fadeDuration = 0.8f;
    public float holdTime = 1.5f;
    public float textFadeDuration = 0.25f;

    [Header("Optional")]
    public bool useCurrentScenePreset = false;
    public SceneFadePreset currentScenePreset;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponent<Button>();

        if (cachedButton == null)
        {
            Debug.LogError(
                $"[SceneFadeButtonTrigger] 找不到 Button 元件！物件：{GetFullPath(gameObject)}，Scene：{gameObject.scene.name}");
        }
    }

    public void TriggerFadeToScene()
    {
        if (SceneFader.Instance == null)
        {
            Debug.LogError(
                $"[SceneFadeButtonTrigger] 找不到 SceneFader.Instance！物件：{GetFullPath(gameObject)}，Scene：{gameObject.scene.name}");
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError(
                $"[SceneFadeButtonTrigger] targetSceneName 沒填！物件：{GetFullPath(gameObject)}，Scene：{gameObject.scene.name}");
            return;
        }

        float finalFadeDuration = fadeDuration;
        float finalHoldTime = holdTime;
        float finalTextFadeDuration = textFadeDuration;

        if (useCurrentScenePreset && currentScenePreset != null)
        {
            finalFadeDuration = currentScenePreset.fadeDuration;
            finalHoldTime = currentScenePreset.holdTime;
            finalTextFadeDuration = currentScenePreset.textFadeDuration;
        }

        SceneFader.Instance.FadeToSceneWithCustomData(
            targetSceneName,
            message,
            finalFadeDuration,
            finalHoldTime,
            finalTextFadeDuration
        );
    }

    private string GetFullPath(GameObject obj)
    {
        if (obj == null) return "(null)";

        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}