using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XIV.DesignPatterns.Common.Utils;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
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
            ScriptableRendererData scriptableRendererData = GetRendererData(renderPipelineAsset);
            if (supportedRenderers.Contains(scriptableRendererData) == false) return null;
            
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
                timer = new Timer(duration),
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
            int count = animatableProperties.Count;
            for (int i = 0; i < count; i++)
            {
                var p = animatableProperties[i];
                p.id = Shader.PropertyToID(p.propertyName);
            }
        }

        ScriptableRendererFeature GetFeature(ScriptableRendererData scriptableRendererData)
        {
            int count = scriptableRendererData.rendererFeatures.Count;
            for (int i = 0; i < count; i++)
            {
                ScriptableRendererFeature item = scriptableRendererData.rendererFeatures[i];
                if (item.name == rendererFeatureName)
                {
                    return item;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            bool Count(ScriptableRendererData scriptableRendererData)
            {
                int count = 0;
                for (int i = 0; i < supportedRenderers.Count && count < 2; i++)
                {
                    if (supportedRenderers[i] == scriptableRendererData)
                    {
                        count++;
                    }
                }

                return count > 1;
            }
            // Remove duplicate entries
            supportedRenderers.RemoveAll(Count);
        }
#endif
    }
}