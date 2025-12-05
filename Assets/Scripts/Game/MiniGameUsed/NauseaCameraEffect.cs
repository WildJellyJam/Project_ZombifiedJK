using UnityEngine;

/// <summary>
/// Small wobble / nausea camera effect.
/// Add this to your Main Camera.
/// Use SetIntensity(0~1) from another script to control how strong it is.
/// </summary>
public class NauseaCameraEffect : MonoBehaviour
{
    [Header("Toggle")]
    public bool effectEnabled = true;

    [Header("Overall Intensity (0~1)")]
    [Range(0f, 1f)]
    public float intensity = 0.5f;

    [Header("Position Shake")]
    [Tooltip("Max world-space offset on X/Y when intensity = 1")]
    public float positionAmplitude = 0.2f;
    [Tooltip("How fast the position noise moves")]
    public float positionFrequency = 1.5f;

    [Header("Rotation Shake")]
    [Tooltip("Max Z rotation (degrees) when intensity = 1")]
    public float rotationAmplitude = 3f;
    [Tooltip("How fast it tilts left/right")]
    public float rotationFrequency = 1.2f;

    [Header("Subtle Zoom Warp (Optional)")]
    [Tooltip("Max extra orthographic size or FOV when intensity = 1")]
    public float zoomAmplitude = 0.3f;
    [Tooltip("How fast zoom pulses")]
    public float zoomFrequency = 0.7f;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private float originalOrthoSize;
    private float originalFOV;
    private Camera cam;

    private void Start()
    {
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            if (cam.orthographic)
                originalOrthoSize = cam.orthographicSize;
            else
                originalFOV = cam.fieldOfView;
        }
    }

    private void LateUpdate()
    {
        if (!effectEnabled || intensity <= 0f)
        {
            ResetTransform();
            return;
        }

        float t = Time.time;

        // --- Position wobble (Perlin noise) ---
        float noiseX = (Mathf.PerlinNoise(t * positionFrequency, 0f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(0f, t * positionFrequency) - 0.5f) * 2f;

        Vector3 posOffset = new Vector3(noiseX, noiseY, 0f)
                            * positionAmplitude
                            * intensity;

        // --- Rotation wobble (sin wave on Z) ---
        float rotZ = Mathf.Sin(t * rotationFrequency * Mathf.PI * 2f)
                     * rotationAmplitude
                     * intensity;

        transform.localPosition = originalLocalPos + posOffset;
        transform.localRotation = originalLocalRot * Quaternion.Euler(0f, 0f, rotZ);

        // --- Subtle zoom / ¡§breathing¡¨ warp ---
        if (cam != null && zoomAmplitude > 0f)
        {
            float zoomOffset = Mathf.Sin(t * zoomFrequency * Mathf.PI * 2f)
                               * zoomAmplitude
                               * intensity;

            if (cam.orthographic)
                cam.orthographicSize = originalOrthoSize + zoomOffset;
            else
                cam.fieldOfView = originalFOV + zoomOffset;
        }
    }

    private void ResetTransform()
    {
        transform.localPosition = originalLocalPos;
        transform.localRotation = originalLocalRot;

        if (cam != null)
        {
            if (cam.orthographic)
                cam.orthographicSize = originalOrthoSize;
            else
                cam.fieldOfView = originalFOV;
        }
    }

    /// <summary>
    /// 0 = no effect, 1 = full nausea.
    /// </summary>
    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }
}
