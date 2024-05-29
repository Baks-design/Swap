using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [System.Serializable]
    public class ColorAnimatableProperty : AnimatableProperty<Color>
    {
        public override void Animate(Material material, float normalizedTime)
        {
            var val = Color.Lerp(start, end, curve.Evaluate(normalizedTime));
            material.SetVector(id, val);
        }
    }
}