using System;
using UnityEngine;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Observer.Example01
{
    public static class GameEvents
    {
        public static event Action<Component> onDamageableLoaded;

        public static void InvokeOnDamageableLoaded<T>(T damageable) where T : Component, IDamageable
        {
            onDamageableLoaded?.Invoke(damageable);
        }
    }
}