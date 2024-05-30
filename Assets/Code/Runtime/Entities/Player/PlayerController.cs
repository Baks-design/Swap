using System;
using KBCore.Refs;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Entities.Weapons;
using SwapChains.Runtime.Utilities.Helpers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("Refs")]
        [SerializeField] Health health;
        [SerializeField, Child] Weapon weapon;
        [SerializeField, Child] CinemachineCamera cinemachine;
        [SerializeField, Child] SkinnedMeshRenderer[] meshRenderers;
        [NonSerialized] public float DeltaTime = 0f;
        [NonSerialized] public float FixedDeltaTime = 0f;
        [NonSerialized] public float GameTime = 0f;
        [NonSerialized] public float Gravity = 0f;
        [NonSerialized] public Camera Camera;
        [NonSerialized] public Transform Transform;

        public bool IsNpcActive { get; private set; } = false;

        void OnValidate() => this.ValidateRefs();

        void Awake()
        {
            Camera = Camera.main;
            Transform = transform;
            Gravity = Physics.gravity.y;
            health.Initialize();
            GameEvents.InvokeOnDamageableLoaded(this);
        }

        void FixedUpdate() => FixedDeltaTime = Time.fixedDeltaTime;

        void Update()
        {
            DeltaTime = Time.deltaTime;
            GameTime = Time.time;
        }

        public void NPCComponentsHandle(bool isActive)
        {
            if (isActive)
            {
                IsNpcActive = true;
                cinemachine.gameObject.SetActive(true);
                for (var i = 0; i < meshRenderers.Length; i++)
                    meshRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                IsNpcActive = false;
                cinemachine.gameObject.SetActive(false);
                for (var i = 0; i < meshRenderers.Length; i++)
                    meshRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            }
        }

        bool IDamageable.CanReceiveDamage() => health.IsDepleted is false;

        void IDamageable.ReceiveDamage(float amount) => health.DecreaseCurrentHealth(amount);

        Health IDamageable.GetHealth() => health;
    }
}