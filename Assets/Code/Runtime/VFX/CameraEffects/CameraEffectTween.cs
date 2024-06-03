using SwapChains.Runtime.Utilities.Timers;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public abstract class CameraEffectTween : MonoBehaviour
    {
        [SerializeField] protected CountdownTimer timer = new(0.5f);

        public abstract void StartTween();

        public virtual void StopTween() => timer.Tick();
    }
}