using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public class CameraEffectPlayer : MonoBehaviour, IHealthListener
    {
        [SerializeField] CameraEffectTween cameraEffectOnHurt;
        [SerializeField] CameraEffectTween cameraEffectOnHealthDepleted;
        IDamageable damageable;

        void OnEnable()
        {
            GameEvents.OnDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void OnDisable()
        {
            GameEvents.OnDamageableLoaded -= OnDamageableLoaded;
            damageable?.GetHealth().Unregister(this);
        }

        void OnDamageableLoaded(Component obj)
        {
            damageable?.GetHealth().Unregister(this);
            damageable = obj as IDamageable;
            damageable?.GetHealth().Register(this);
        }

        void PlayEffect(CameraEffectTween cameraEffectTween) => cameraEffectTween.StartTween();

        void IHealthListener.OnHealthChange(HealthChange healthChange) => PlayEffect(cameraEffectOnHurt);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => PlayEffect(cameraEffectOnHealthDepleted);
    }
}