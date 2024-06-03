using System.Collections.Generic;
using System.Reflection;
using SwapChains.Runtime.Utilities.Timers;
using SwapChains.Runtime.Utilities.VFX;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

namespace SwapChains.Runtime.VFX
{
    [CreateAssetMenu(menuName = "SO/" + nameof(EffectDataSO))]
    public class EffectDataSO : ScriptableObject
    {
        [SerializeField] Material material;
        [SerializeField] string rendererFeatureName;
        [SerializeField] FloatAnimatableProperty[] floatAnimatableProperties;
        [SerializeField] ColorAnimatableProperty[] colorAnimatableProperties;
        [SerializeField] VectorAnimatableProperty[] vectorAnimatableProperties;
        [SerializeField] bool deactivateOnComplete;
        
        [SerializeField] float duration;
        [Tooltip("DO NOT ADD USING PLUS ICON. Since it will duplicate the last added item and that will trigger remove duplicates function." + 
                 " Instead drag and drop the items that you want to add.")]
        [SerializeField] List<ScriptableRendererData> supportedRenderers;

        public EffectData GetEffectData(UniversalRenderPipelineAsset renderPipelineAsset)
        {
            // TODO : EffectDataSO => GetEffectData - Optimization. Do not use reflection
            var scriptableRendererData = GetRendererData(renderPipelineAsset);
            //if (supportedRenderers.Contains(scriptableRendererData) is false) return true;
            
            var list = ListPool<AnimatableProperty>.Get();
            list.AddRange(floatAnimatableProperties);
            list.AddRange(colorAnimatableProperties);
            list.AddRange(vectorAnimatableProperties);
            
            InitializeAnimatableProperties(list);
            
            var arr = list.ToArray();
            ListPool<AnimatableProperty>.Release(list);
            return new EffectData
            {
                deactivateOnComplete = deactivateOnComplete,
                material = material,
                animatableProperties = arr,
                feature = GetFeature(scriptableRendererData),
                timer = new CountdownTimer(duration),
            };
        }

        ScriptableRendererData GetRendererData(UniversalRenderPipelineAsset urpAsset)
        {
            var type = typeof(UniversalRenderPipelineAsset);
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var propertyInfo = type.GetProperty("scriptableRendererData", FLAGS);
            var val = (ScriptableRendererData)propertyInfo.GetValue(urpAsset);
            return val;
        }

        void InitializeAnimatableProperties(List<AnimatableProperty> animatableProperties)
        {
            var count = animatableProperties.Count;
            for (var i = 0; i < count; i++)
            {
                var p = animatableProperties[i];
                p.id = Shader.PropertyToID(p.propertyName);
            }
        }

        ScriptableRendererFeature GetFeature(ScriptableRendererData scriptableRendererData)
        {
            var count = scriptableRendererData.rendererFeatures.Count;
            for (var i = 0; i < count; i++)
            {
                var item = scriptableRendererData.rendererFeatures[i];
                if (item.name == rendererFeatureName)
                    return item;
            }

            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            bool Count(ScriptableRendererData scriptableRendererData)
            {
                var count = 0;
                for (var i = 0; i < supportedRenderers.Count && count < 2; i++)
                    if (supportedRenderers[i] == scriptableRendererData)
                        count++;
               
                return count > 1;
            }
            // Remove duplicate entries
            supportedRenderers.RemoveAll(Count);
        }
#endif
    }
}