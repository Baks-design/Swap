using UnityEngine;
using XIV.DesignPatterns.Common.Utils;

namespace XIV.DesignPatterns.Observer.Example01.PlayerDamageEffects.CameraEffects
{
    public abstract class CameraEffectTween : MonoBehaviour
    {
        [SerializeField] protected Timer timer = new Timer(0.5f);

        public abstract void StartTween();

        public virtual void StopTween()
        {
            timer.Update(float.MaxValue);
        }
    }
}