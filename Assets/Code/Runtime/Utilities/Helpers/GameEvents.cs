using System;
using SwapChains.Runtime.Entities.Damages;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.Helpers
{
    public static class GameEvents
    {
        public static event Action<Component> OnDamageableLoaded;

        public static void InvokeOnDamageableLoaded<T>(T damageable) where T : Component, IDamageable
        => OnDamageableLoaded?.Invoke(damageable);
    }
}