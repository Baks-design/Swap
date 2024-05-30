using System;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.VFX
{
    [Serializable]
    public class FloatAnimatableProperty : AnimatableProperty<float>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Mathf.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetFloat(id, val);
        }
    }
}