using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public abstract class CameraEffectTween : MonoBehaviour
    {
        [SerializeField] protected Timer timer = new(0.5f);

        public abstract void StartTween();

        public virtual void StopTween() => timer.Update(float.MaxValue);
    }
}