using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class NearestOnlyFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask layerMask = ~0;
        public bool useR16Float = false;
    }

    public Settings settings = new Settings();

    class NearestOnlyPass : ScriptableRenderPass
    {
        readonly LayerMask _layerMask;
        readonly ShaderTagId _tagDepth = new ShaderTagId("NearestOnlyDepth");
        readonly ProfilingSampler _profWrite   = new ProfilingSampler("NearestOnly: Write");
        readonly ProfilingSampler _profPublish = new ProfilingSampler("NearestOnly: Publish");
        readonly GraphicsFormat _fmt;

        public NearestOnlyPass(LayerMask mask, bool useR16)
        {
            _layerMask = mask;
            _fmt = useR16 ? GraphicsFormat.R16_SFloat : GraphicsFormat.R32_SFloat;
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        class WriteData
        {
            public TextureHandle output;
            public RendererListHandle rendererList;
        }

        class PublishData
        {
            public TextureHandle nearestTex;
        }

        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            var urpRendering = frameData.Get<UniversalRenderingData>();
            var urpCamera    = frameData.Get<UniversalCameraData>();

            TextureHandle nearestTex;
            {
                using var builder = rg.AddRasterRenderPass<WriteData>(
                    "NearestOnly: Write", out var pass, _profWrite);

                var desc = new TextureDesc(Vector2.one, true, true)
                {
                    name        = "_NearestOnlyTex",
                    colorFormat = _fmt,
                    clearBuffer = true,
                    clearColor  = new Color(1e9f, 0, 0, 0), // start “infinite”
                    msaaSamples = MSAASamples.None,
                    filterMode  = FilterMode.Point,
                    wrapMode    = TextureWrapMode.Clamp
                };

                pass.output = rg.CreateTexture(desc);
                builder.SetRenderAttachment(pass.output, 0);

                var tags = new ShaderTagId[] { _tagDepth };
                var rlDesc = new RendererListDesc(tags, urpRendering.cullResults, urpCamera.camera)
                {
                    sortingCriteria  = SortingCriteria.CommonTransparent,
                    renderQueueRange = RenderQueueRange.transparent,
                    layerMask        = _layerMask
                };
                pass.rendererList = rg.CreateRendererList(rlDesc);
                builder.UseRendererList(pass.rendererList);

                builder.SetRenderFunc((WriteData d, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(d.rendererList);
                });

                nearestTex = pass.output;
            }

            {
                using var builder = rg.AddComputePass<PublishData>(
                    "NearestOnly: Publish", out var pass, _profPublish);

                pass.nearestTex = nearestTex;
                builder.UseTexture(pass.nearestTex);

                builder.SetRenderFunc((PublishData d, ComputeGraphContext ctx) =>
                {
                    ctx.cmd.SetGlobalTexture("_NearestOnlyTex", d.nearestTex);
                });

                builder.AllowGlobalStateModification(true);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }

    NearestOnlyPass _pass;

    public override void Create()
    {
        _pass = new NearestOnlyPass(settings.layerMask, settings.useR16Float);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}
