using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [System.Serializable]
    public abstract class AnimatableProperty
    {
        [HideInInspector]
        public int id;
        public string propertyName;
        public AnimationCurve curve;
        public abstract void Animate(Material material, float normalizedTime);
    }

    [System.Serializable]
    public abstract class AnimatableProperty<T> : AnimatableProperty
    {
        public T start;
        public T end;
    }
}