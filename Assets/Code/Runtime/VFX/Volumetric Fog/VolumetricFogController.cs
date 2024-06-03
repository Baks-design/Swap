using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace SwapChains.Runtime.VFX.VolumetricFog
{
    public class VolumetricFogController : MonoBehaviour
    {
        public Material material;
        public Slider slider_raymarchSteps;
        public Slider slider_downsampleLevel;
        public Slider slider_mainLightIntensity;
        const string keyword_MAIN_LIGHT_ENABLED = "_MAIN_LIGHT_ENABLED";
        public const string materialPropertyName_raymarchSteps = "_Raymarch_Steps";
        public const string materialPropertyName_mainLightIntensity = "_Main_Light_Intensity";

        public ScriptableRendererFeature RendererFeature { get; private set; }
        public IVolumetricFog VolumetricFogCommonInterface { get; private set; }

        void Start()
        {
            var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            var rendererFeatures = property.GetValue(renderer) as List<ScriptableRendererFeature>;

            // Take first IVolumetricFog that is also active.
            RendererFeature = rendererFeatures.Where(x => x.isActive && (x as IVolumetricFog) != null).First();
            // I know this feature has IVolumetricFog because it must be as per the previous line.
            VolumetricFogCommonInterface = RendererFeature as IVolumetricFog;

            SetRaymarchSteps(slider_raymarchSteps.value);
            SetDownsampleLevel(slider_downsampleLevel.value);
            SetMainLightIntensity(slider_mainLightIntensity.value);
        }

        public void SetRaymarchSteps(float value) => material.SetInt(materialPropertyName_raymarchSteps, Mathf.RoundToInt(value));

        public void SetDownsampleLevel(float value) => VolumetricFogCommonInterface.SetDownsampleLevel(Mathf.RoundToInt(value));

        public void SetMainLightIntensity(float value)
        {
            material.SetFloat(materialPropertyName_mainLightIntensity, value);

            if (value > 0.0f)
                material.EnableKeyword(keyword_MAIN_LIGHT_ENABLED);
            else
                material.DisableKeyword(keyword_MAIN_LIGHT_ENABLED);
        }
    }
}
