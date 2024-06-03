using SwapChains.Runtime.Utilities.Timers;
using SwapChains.Runtime.Utilities.VFX;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SwapChains.Runtime.VFX
{
    public struct EffectData
    {
        public bool deactivateOnComplete;
        public Material material;
        public AnimatableProperty[] animatableProperties;
        public ScriptableRendererFeature feature;
        public CountdownTimer timer;
    }
}