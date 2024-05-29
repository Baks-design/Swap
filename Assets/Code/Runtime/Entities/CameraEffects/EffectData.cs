using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using XIV.DesignPatterns.Common.Utils;
using XIV.DesignPatterns.Observer.Example01.ScriptableObjects;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class EffectData
    {
        public bool deactivateOnComplete;
        public Material material;
        public AnimatableProperty[] animatableProperties;
        public ScriptableRendererFeature feature;
        public Timer timer;
    }
}