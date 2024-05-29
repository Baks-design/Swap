using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class PlayerHealthUI : ValidatedMonoBehaviour, IHealthListener
    {
        [SerializeField, Child] Image healthIndicator;
        IDamageable damageable;
        
        void OnEnable()
        {
            GameEvents.onDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void OnDisable()
        {
            GameEvents.onDamageableLoaded -= OnDamageableLoaded;
            damageable?.GetHealth().Unregister(this);
        }

        void OnDamageableLoaded(Component obj)
        {
            damageable?.GetHealth().Unregister(this);
            damageable = obj as IDamageable;
            
            var health = damageable.GetHealth();
            health.Register(this);
            UpdateUI(health.normalized);
        }

        void UpdateUI(float normalizedHealth) => healthIndicator.fillAmount = normalizedHealth;

        void IHealthListener.OnHealthChange(HealthChange healthChange) => UpdateUI(healthChange.normalized);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => UpdateUI(healthChange.normalized);
    }
}