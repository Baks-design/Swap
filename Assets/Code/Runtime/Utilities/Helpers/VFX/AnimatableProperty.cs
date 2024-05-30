using System;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.VFX
{
    [Serializable]
    public abstract class AnimatableProperty
    {
        [HideInInspector]
        public int id;
        public string propertyName;
        public AnimationCurve curve;
        public abstract void Animate(Material material, float normalizedTime);
    }

    [Serializable]
    public abstract class AnimatableProperty<T> : AnimatableProperty
    {
        public T start;
        public T end;
    }
}