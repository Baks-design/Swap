using KBCore.Refs;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace SwapChains.Runtime.UserInterface
{
    [RequireComponent(typeof(Image))]
    public class PlayerHealthUI : MonoBehaviour, IHealthListener //TODO: CHECK 
    {
        [SerializeField, Self] Image health;

        IDamageable damageable;
        static readonly int SegmentCount = Shader.PropertyToID("_SegmentCount");

        void OnValidate() => this.ValidateRefs();

        void OnEnable()
        {
            GameEvents.OnDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void Start() => health.enabled = true;

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
            UpdateUI(health.Current);
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => UpdateUI(healthChange.currentHealth);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => UpdateUI(healthChange.currentHealth);

        void UpdateUI(int currentHealth)  //TODO: Continue
        {
            if (health.material.HasProperty(SegmentCount))
                health.material.SetFloat(SegmentCount, -1);
        }
    }
}