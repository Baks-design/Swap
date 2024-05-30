using System;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.VFX
{
    [Serializable]
    public class VectorAnimatableProperty : AnimatableProperty<Vector4>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Vector4.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetVector(id, val);
        }
    }
}