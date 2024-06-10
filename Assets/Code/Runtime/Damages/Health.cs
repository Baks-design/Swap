using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Damages
{
    [Serializable]
    public struct Health
    {
        [SerializeField] int maxHealth;
        [SerializeField] int currentHealth;
        List<IHealthListener> listeners;

        public readonly bool IsDepleted => currentHealth <= 0;
        public readonly int Max => maxHealth;
        public readonly int Current => currentHealth;

        public Health(int maxHealth, int currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
            listeners = new List<IHealthListener>();
        }

        public Health(int maxHealth) : this(maxHealth, maxHealth) { }

        public void Initialize() => listeners ??= new List<IHealthListener>();

        public readonly void Register(IHealthListener listener)
        {
            if (listeners.Contains(listener) is false)
                listeners.Add(listener);
        }

        public readonly bool Unregister(IHealthListener listener) => listeners.Remove(listener);

        public void IncreaseMaxHealth(int amount) => ChangeValue(ref maxHealth, amount, int.MaxValue);

        public void DecreaseMaxHealth(int amount) => ChangeValue(ref maxHealth, -amount, int.MaxValue);

        public void IncreaseCurrentHealth(int amount) => ChangeValue(ref currentHealth, amount, maxHealth);

        public void DecreaseCurrentHealth(int amount)
        {
            ChangeValue(ref currentHealth, -amount, maxHealth);
            if (IsDepleted)
                InformListenersOnHealthDepleted();
        }

        readonly void ChangeValue(ref int current, int amount, int max)
        {
            var newValue = Mathf.Clamp(current + amount, 0f, max);
            var diff = Mathf.Abs(newValue - current);
            current = (int)newValue;
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