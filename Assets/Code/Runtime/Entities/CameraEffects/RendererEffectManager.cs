using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using XIV.DesignPatterns.Observer.Example01.ScriptableObjects;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class RendererEffectManager : MonoBehaviour
    {
        List<EffectData> activeEffectDatas;
        List<EffectDataSO> activeEffects;
        // These effects will be deactivated on OnDestroy call
        List<EffectDataSO> effectsToDeactivate;

        void Awake()
        {
            activeEffectDatas = new List<EffectData>();
            activeEffects = new List<EffectDataSO>();
            effectsToDeactivate = new List<EffectDataSO>();
        }

        void Update()
        {
            int count = activeEffectDatas.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var effectData = activeEffectDatas[i];
                effectData.timer.Update(Time.deltaTime);
                AnimateMaterial(effectData);

                if (effectData.timer.IsDone())
                {
                    DeactivateIfPossible(effectData, i);
                    activeEffectDatas.RemoveAt(i);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        void OnDestroy()
        {
            activeEffectDatas.ForEach(Deactivate);
            effectsToDeactivate.ForEach(p => Deactivate(p.GetEffectData(GetPipelineAsset())));
        }

        public void PlayEffect(EffectDataSO effectDataSO)
        {
            EffectData effectData = effectDataSO.GetEffectData(GetPipelineAsset());
            if (effectData == null) return;
            Activate(effectData);
            activeEffects.Add(effectDataSO);
            activeEffectDatas.Add(effectData);
        }

        public void PlayEffects(EffectDataSO[] arr)
        {
            int length = arr.Length;
            for (int i = 0; i < length; i++)
            {
                if (IsActive(arr[i])) continue;

                PlayEffect(arr[i]);
            }
        }

        public void StopEffect(EffectDataSO effectDataSO, bool complete = false)
        {
            int index = activeEffects.IndexOf(effectDataSO);
            if (index == -1) return;

            var effectData = activeEffectDatas[index];
            if (complete)
            {
                effectData.timer.Update(float.MaxValue);
                AnimateMaterial(effectData);
            }
            // We consider that the effect has been completed because we are done with it
            DeactivateIfPossible(effectData, index);
            activeEffectDatas.RemoveAt(index);
            activeEffects.RemoveAt(index);
        }

        void DeactivateIfPossible(EffectData effectData, int index)
        {
            if (effectData.deactivateOnComplete) Deactivate(effectData);
            else effectsToDeactivate.Add(activeEffects[index]);
        }

        public void StopEffects(EffectDataSO[] arr, bool complete = false)
        {
            int length = arr.Length;
            for (int i = 0; i < length; i++)
            {
                StopEffect(arr[i], complete);
            }
        }

        public bool IsActive(EffectDataSO effectDataSO)
        {
            return activeEffects.Contains(effectDataSO);
        }

        void Activate(EffectData effectData)
        {
            effectData.feature.SetActive(true);
        }

        void Deactivate(EffectData effectData)
        {
            effectData.feature.SetActive(false);
        }

        void AnimateMaterial(EffectData effectData)
        {
            Material material = effectData.material;
            AnimatableProperty[] animatableProperties = effectData.animatableProperties;
            int length = animatableProperties.Length;
            for (int i = 0; i < length; i++)
            {
                animatableProperties[i].Animate(material, effectData.timer.normalizedTime);
            }
        }

        UniversalRenderPipelineAsset GetPipelineAsset()
        {
            return (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        }
    }
}