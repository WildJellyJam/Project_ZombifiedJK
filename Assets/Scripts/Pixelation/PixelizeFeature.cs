using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public int screenHeight = 144;
    }

    [SerializeField] private CustomPassSettings settings;
    private PixelizePass customPass;

    public override void Create()
    {
        // Pixelize disabled: do not create the pass anymore
        customPass = null;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // if we're in Scene view, skip (same as before)
#if UNITY_EDITOR
        if (renderingData.cameraData.isSceneViewCamera) return;
#endif
        // Pixelize disabled: do not enqueue anything
        if (customPass == null) return;
    }
}
