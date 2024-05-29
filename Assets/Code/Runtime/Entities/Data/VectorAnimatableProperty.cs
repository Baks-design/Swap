using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [System.Serializable]
    public class VectorAnimatableProperty : AnimatableProperty<Vector4>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Vector4.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetVector(id, val);
        }
    }
}