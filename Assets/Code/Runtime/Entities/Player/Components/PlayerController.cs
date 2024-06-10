using KBCore.Refs;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using Unity.Cinemachine;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Refs")]
        [SerializeField, Child] CinemachineCamera cinemachine;

        [Header("Classes")]
        [SerializeField] Health health;

        public bool IsNpcActive { get; set; } = false;

        void OnValidate() => this.ValidateRefs();

        void Awake()
        {
            health.Initialize();
            GameEvents.InvokeOnDamageableLoaded(this);
        }

        #region Health
        public bool CanReceiveDamage() => health.IsDepleted is false;

        public void ReceiveDamage(int amount) => health.DecreaseCurrentHealth(amount);

        public Health GetHealth() => health;
        #endregion

        public void NPCComponentsHandle(bool isActive)
        {
            if (isActive)
            {
                IsNpcActive = true;
                cinemachine.enabled = true;
            }
            else
            {
                IsNpcActive = false;
                cinemachine.enabled = false;
            }
        }
    }
}