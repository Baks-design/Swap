using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public class RendererEffectPlayer : MonoBehaviour, IHealthListener
    {
        [SerializeField] RendererEffectManager rendererEffectManager;
        [SerializeField] EffectDataSO rendererEffectOnHurt;
        [SerializeField] EffectDataSO rendererEffectOnHealthDepleted;
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

        void PlayEffect(EffectDataSO effectDataSO)
        {
            if (rendererEffectManager.IsActive(effectDataSO)) return;
            rendererEffectManager.PlayEffect(effectDataSO);
        }

        void StopEffect(EffectDataSO effectDataSO) => rendererEffectManager.StopEffect(effectDataSO);

        void IHealthListener.OnHealthChange(HealthChange healthChange) => PlayEffect(rendererEffectOnHurt);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange)
        {
            StopEffect(rendererEffectOnHurt);
            PlayEffect(rendererEffectOnHealthDepleted);
        }
    }
}