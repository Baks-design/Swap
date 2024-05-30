using KBCore.Refs;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    public class PlayerHealthUI : ValidatedMonoBehaviour, IHealthListener
    {
        [SerializeField, Child] Image healthIndicator;
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
            
            var health = damageable.GetHealth();
            health.Register(this);
            UpdateUI(health.Normalized);
        }

        void UpdateUI(float normalizedHealth) => healthIndicator.fillAmount = normalizedHealth;

        void IHealthListener.OnHealthChange(HealthChange healthChange) => UpdateUI(healthChange.normalized);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => UpdateUI(healthChange.normalized);
    }
}