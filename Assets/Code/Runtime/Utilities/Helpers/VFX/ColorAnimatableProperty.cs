using System;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.VFX
{
    [Serializable]
    public class ColorAnimatableProperty : AnimatableProperty<Color>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Color.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetVector(id, val);
        }
    }
}