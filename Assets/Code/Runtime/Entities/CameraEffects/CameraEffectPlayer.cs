using System;
using UnityEngine;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Observer.Example01.PlayerDamageEffects.CameraEffects
{
    public class CameraEffectPlayer : MonoBehaviour, IHealthListener
    {
        [SerializeField] CameraEffectTween cameraEffectOnHurt;
        [SerializeField] CameraEffectTween cameraEffectOnHealthDepleted;
        
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
            damageable?.GetHealth().Register(this);
        }

        void PlayEffect(CameraEffectTween cameraEffectTween)
        {
            cameraEffectTween.StartTween();
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange)
        {
            PlayEffect(cameraEffectOnHurt);
        }

        void IHealthListener.OnHealthDepleted(HealthChange healthChange)
        {
            PlayEffect(cameraEffectOnHealthDepleted);
        }
    }
}