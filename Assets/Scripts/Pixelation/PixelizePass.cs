using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// SUPER SAFE stub so it does nothing
public class PixelizePass : ScriptableRenderPass
{
    public PixelizePass()
    {
        // If you had a constructor before, you can ignore it now
        renderPassEvent = RenderPassEvent.AfterRendering; // or whatever, doesn¡¦t matter now
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Do nothing ¡V no null access, no RTs, totally safe
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Do nothing ¡V no blit, no material, no camera
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // Do nothing
    }
}
