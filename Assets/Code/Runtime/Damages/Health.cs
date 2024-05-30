using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Damages
{
    [Serializable]
    public struct Health
    {
        [SerializeField] float maxHealth;
        [SerializeField] float currentHealth;
        List<IHealthListener> listeners;

        public readonly bool IsDepleted => currentHealth < Mathf.Epsilon;
        public readonly float Normalized => currentHealth / maxHealth;
        public readonly float Max => maxHealth;
        public readonly float Current => currentHealth;

        public Health(float maxHealth, float currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
            listeners = new List<IHealthListener>();
        }

        public Health(float maxHealth) : this(maxHealth, maxHealth) { }

        public void Initialize() => listeners ??= new List<IHealthListener>();

        public readonly void Register(IHealthListener listener)
        {
            if (listeners.Contains(listener) is false)
                listeners.Add(listener);
        }

        public readonly bool Unregister(IHealthListener listener) => listeners.Remove(listener);

        public void IncreaseMaxHealth(float amount) => ChangeValue(ref maxHealth, amount, float.MaxValue);

        public void DecreaseMaxHealth(float amount) => ChangeValue(ref maxHealth, -amount, float.MaxValue);

        public void IncreaseCurrentHealth(float amount) => ChangeValue(ref currentHealth, amount, maxHealth);

        public void DecreaseCurrentHealth(float amount)
        {
            ChangeValue(ref currentHealth, -amount, maxHealth);
            if (IsDepleted)
                InformListenersOnHealthDepleted();
        }

        readonly void ChangeValue(ref float current, float amount, float max)
        {
            var newValue = Mathf.Clamp(current + amount, 0f, max);
            var diff = Mathf.Abs(newValue - current);
            current = newValue;
            if (diff > 0f)
                InformListenersOnHealthChange();
        }

        readonly void InformListenersOnHealthDepleted()
        {
            var count = listeners.Count;
            var healthChange = new HealthChange(maxHealth, currentHealth);
            for (var i = count - 1; i >= 0; i--)
                listeners[i].OnHealthDepleted(healthChange);
        }

        readonly void InformListenersOnHealthChange()
        {
            var count = listeners.Count;
            var healthChange = new HealthChange(maxHealth, currentHealth);
            for (var i = count - 1; i >= 0; i--)
                listeners[i].OnHealthChange(healthChange);
        }
    }
}