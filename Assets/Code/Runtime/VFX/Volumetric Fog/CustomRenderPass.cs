using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SwapChains.Runtime.VFX.VolumetricFog
{
    class CustomRenderPass : ScriptableRenderPass
    {
        readonly Settings settings;
        RenderTextureDescriptor colourTextureDescriptor;
        RenderTextureDescriptor fogTextureDescriptor;
        RenderTextureDescriptor depthTextureDescriptor;
        RTHandle colourTextureHandle;
        RTHandle fogTextureHandle;
        RTHandle depthTextureHandle;

        public CustomRenderPass(Settings settings)
        {
            if (settings == null)
                return;

            this.settings = settings;
            RenderTextureFormat renderTextureFormat;
            switch (settings.renderTextureQuality)
            {
                case RenderTextureQuality.Low:
                    {
                        // Expect banding.
                        renderTextureFormat = RenderTextureFormat.Default;
                        break;
                    }
                case RenderTextureQuality.Medium:
                    {
                        // Smooth.
                        renderTextureFormat = RenderTextureFormat.ARGB64;
                        break;
                    }
                case RenderTextureQuality.High:
                    {
                        // Smooth + HDR (works with bloom).
                        renderTextureFormat = RenderTextureFormat.ARGBFloat;
                        break;
                    }
                default:
                    {
                        throw new Exception("Unknown enum.");
                    }
            }

            colourTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, renderTextureFormat, 0);
            fogTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, renderTextureFormat, 0);
            depthTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.RFloat, 0);
        }

        #region Obsolete
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        [Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) { }

        // Called before Execute().
        [Obsolete]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var fogDownsampleLevel = settings.fogDownsampleLevel;

            colourTextureDescriptor.width = cameraTextureDescriptor.width;
            colourTextureDescriptor.height = cameraTextureDescriptor.height;

            fogTextureDescriptor.width = colourTextureDescriptor.width / fogDownsampleLevel;
            fogTextureDescriptor.height = colourTextureDescriptor.height / fogDownsampleLevel;

            depthTextureDescriptor.width = fogTextureDescriptor.width;
            depthTextureDescriptor.height = fogTextureDescriptor.height;

            // Check if the descriptor has changed, and reallocate the RTHandle if necessary.
            RenderingUtils.ReAllocateIfNeeded(ref colourTextureHandle, colourTextureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref fogTextureHandle, fogTextureDescriptor);
            RenderingUtils.ReAllocateIfNeeded(ref depthTextureHandle, depthTextureDescriptor);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Get a CommandBuffer from pool.
            var cmd = CommandBufferPool.Get();
            var cameraData = renderingData.cameraData;
            var cameraTargetHandle = cameraData.renderer.cameraColorTargetHandle;

            // Save full-res colour screen and set texture in material (for in-shader compositing).
            Blit(cmd, cameraTargetHandle, colourTextureHandle);
            settings.compositeMaterial.SetTexture(settings.compositeMaterialColourTextureName, colourTextureHandle);

            // Save depth texture and assign to material.
            Blit(cmd, cameraTargetHandle, depthTextureHandle, settings.depthMaterial);
            settings.compositeMaterial.SetTexture(settings.compositeMaterialDepthTextureName, depthTextureHandle);

            // Render fog.
            Blit(cmd, cameraTargetHandle, fogTextureHandle, settings.fogMaterial);

            // Set the fog texture.
            // You can also use Shader Graph's URP Buffer node -> BlitSource (_BlitTexture),
            // which will be whatever is passed as the blit source on the Blit command.
            settings.compositeMaterial.SetTexture(settings.compositeMaterialFogTextureName, fogTextureHandle);

            // Composite.
            Blit(cmd, fogTextureHandle, cameraTargetHandle, settings.compositeMaterial);

            // Execute the command buffer, then release it back to the pool.
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        #endregion

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd) { }

        public void Dispose()
        {
            colourTextureHandle?.Release();
            fogTextureHandle?.Release();
            depthTextureHandle?.Release();
        }
    }
}