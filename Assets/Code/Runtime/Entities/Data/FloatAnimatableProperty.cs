using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [System.Serializable]
    public class FloatAnimatableProperty : AnimatableProperty<float>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Mathf.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetFloat(id, val);
        }
    }
}