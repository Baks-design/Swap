using System.Collections.Generic;
using UnityEngine;

namespace XIV.DesignPatterns.Common.HealthSystem
{
    [System.Serializable]
    public class Health
    {
        public bool isDepleted => currentHealth < Mathf.Epsilon;
        public float normalized => currentHealth / maxHealth;
        public float max => maxHealth;
        public float current => currentHealth;

        [SerializeField] float maxHealth = 100f;
        [SerializeField] float currentHealth = 100f;
        
        List<IHealthListener> listeners;

        public Health(float maxHealth, float currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
            this.listeners = new List<IHealthListener>();
        }

        public Health(float maxHealth) : this(maxHealth, maxHealth)
        {
        }

        public void Initialize()
        {
            listeners ??= new List<IHealthListener>();
        }

        public void Register(IHealthListener listener)
        {
            if (listeners.Contains(listener) == false)
            {
                listeners.Add(listener);
            }
        }
        
        public bool Unregister(IHealthListener listener)
        {
            return listeners.Remove(listener);
        }

        public void IncreaseMaxHealth(float amount)
        {
            ChangeValue(ref maxHealth, amount, float.MaxValue);
        }

        public void DecreaseMaxHealth(float amount)
        {
            ChangeValue(ref maxHealth, -amount, float.MaxValue);
        }

        public void IncreaseCurrentHealth(float amount)
        {
            ChangeValue(ref currentHealth, amount, maxHealth);
        }

        public void DecreaseCurrentHealth(float amount)
        {
            ChangeValue(ref currentHealth, -amount, maxHealth);
            if (isDepleted)
            {
                InformListenersOnHealthDepleted();
            }
        }

        void ChangeValue(ref float current, float amount, float max)
        {
            var newValue = Mathf.Clamp(current + amount, 0f, max);
            var diff = Mathf.Abs(newValue - current);
            current = newValue;
            if (diff > 0f)
            {
                InformListenersOnHealthChange();
            }
        }

        void InformListenersOnHealthDepleted()
        {
            int count = listeners.Count;
            var healthChange = new HealthChange(maxHealth, currentHealth);
            for (int i = count - 1; i >= 0; i--)
            {
                listeners[i].OnHealthDepleted(healthChange);
            }
        }
        
        void InformListenersOnHealthChange()
        {
            int count = listeners.Count;
            var healthChange = new HealthChange(maxHealth, currentHealth);
            for (int i = count - 1; i >= 0; i--)
            {
                listeners[i].OnHealthChange(healthChange);
            }
        }
    }
}