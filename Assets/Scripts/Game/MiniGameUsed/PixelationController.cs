using UnityEngine;

/// <summary>
/// Controls a fullscreen pixelation effect by setting a float on a material.
/// If pixelationMaterial is null, it does NOTHING (safe in builds).
/// You can hook this to any pixelation shader you like in the future.
/// </summary>
public class PixelationController : MonoBehaviour
{
    [Header("Pixelation Material (Optional)")]
    [Tooltip("Material used by your pixelation full-screen effect. If left null, this script does nothing.")]
    public Material pixelationMaterial;

    [Tooltip("Name of the float property controlling pixel size / resolution (e.g. _PixelSize, _BlockSize)")]
    public string pixelSizeProperty = "_PixelSize";

    [Header("Pixel Size Range")]
    [Tooltip("Pixel size when exactly ON the goal (clear). e.g. 1 or 2")]
    public float pixelSizeAtGoal = 1f;

    [Tooltip("Pixel size when VERY far away (low quality, blocky). e.g. 8 or 12")]
    public float pixelSizeAtFar = 8f;

    [Range(0f, 1f)]
    [Tooltip("Current 0~1 intensity (0 = clear, 1 = most pixelated).")]
    public float intensity = 0f;

    private void OnValidate()
    {
        ApplyCurrentIntensity();
    }

    /// <summary>
    /// 0 = clear (goal), 1 = max pixelation (far).
    /// Safe: if there's no material, it just returns.
    /// </summary>
    public void SetIntensity(float t)
    {
        intensity = Mathf.Clamp01(t);
        ApplyCurrentIntensity();
    }

    private void ApplyCurrentIntensity()
    {
        // ✅ If you never assign a material, this script does NOTHING.
        if (pixelationMaterial == null) return;

        float pixelSize = Mathf.Lerp(pixelSizeAtGoal, pixelSizeAtFar, intensity);

        if (pixelationMaterial.HasProperty(pixelSizeProperty))
        {
            pixelationMaterial.SetFloat(pixelSizeProperty, pixelSize);
        }
        else
        {
            // Optional debug for setup time only
            // Debug.LogWarning($"[PixelationController] Material does not have property {pixelSizeProperty}");
        }
    }
}
