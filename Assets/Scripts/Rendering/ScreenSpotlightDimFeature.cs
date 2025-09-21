using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpotlightDimFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material; // assign ScreenSpotlightDim.mat
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    class Pass : ScriptableRenderPass
    {
        readonly string _tag = "ScreenSpotlightDimPass";
        readonly Material _mat;

        public Pass(Material mat, RenderPassEvent evt)
        {
            _mat = mat;
            renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_mat == null) return;

            var cmd = CommandBufferPool.Get(_tag);
#if UNITY_2022_1_OR_NEWER
            var target = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, target, target, _mat, 0);
#else
            var target = renderingData.cameraData.renderer.cameraColorTarget;
            Blit(cmd, target, target, _mat, 0);
#endif
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Settings settings = new Settings();
    Pass _pass;

    // Expose the active material for convenience (lets the controller auto-wire safely)
    public static Material ActiveMaterial { get; private set; }

    public override void Create()
    {
        _pass = new Pass(settings.material, settings.passEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        ActiveMaterial = settings.material;  // public, no reflection, no internals
        renderer.EnqueuePass(_pass);
    }
}