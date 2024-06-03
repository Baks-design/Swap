using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SwapChains.Runtime.VFX.VolumetricFog
{
    public class VolumetricFogRendererFeature : ScriptableRendererFeature, IVolumetricFog
    {
        public bool renderInSceneView = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Settings settings;
        CustomRenderPass customRenderPass;

        public override void Create() => customRenderPass = new CustomRenderPass(settings) { renderPassEvent = renderPassEvent };

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var enqueuePass = renderingData.cameraData.cameraType == CameraType.Game;
            enqueuePass |= renderingData.cameraData.cameraType == CameraType.Reflection;

            if (renderInSceneView)
                enqueuePass |= renderingData.cameraData.cameraType == CameraType.SceneView;

            if (enqueuePass)
                renderer.EnqueuePass(customRenderPass);
        }

        protected override void Dispose(bool disposing) => customRenderPass.Dispose();

        public void SetDownsampleLevel(int downsampleLevel) => settings.fogDownsampleLevel = downsampleLevel;

        public int GetDownsampleLevel() => settings.fogDownsampleLevel;
    }
}