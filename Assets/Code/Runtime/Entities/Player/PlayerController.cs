using System;
using KBCore.Refs;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using XIV.DesignPatterns.Common.HealthSystem;
using XIV.DesignPatterns.Observer.Example01;

namespace SwapChains.Runtime.Entities
{
    public class PlayerController : ValidatedMonoBehaviour, IDamageable
    {
        [SerializeField, Self] Health health;
        [SerializeField, Child] Gun gun;
        [SerializeField, Self] NavMeshAgent agent;
        [SerializeField, Child] CinemachineCamera cinemachine;
        [SerializeField, Child] SkinnedMeshRenderer[] meshRenderers;
        [NonSerialized] public float DeltaTime = 0f;
        [NonSerialized] public float FixedDeltaTime = 0f;
        [NonSerialized] public float GameTime = 0f;
        [NonSerialized] public float Gravity = 0f;
        [NonSerialized] public Camera Camera;
        [NonSerialized] public Transform Transform;

        public bool IsNpcActive { get; private set; } = false;

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
                agent.enabled = false;
                cinemachine.gameObject.SetActive(true);
                for (var i = 0; i < meshRenderers.Length; i++)
                    meshRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                IsNpcActive = false;
                agent.enabled = true;
                cinemachine.gameObject.SetActive(false);
                for (var i = 0; i < meshRenderers.Length; i++)
                    meshRenderers[i].shadowCastingMode = ShadowCastingMode.On;
            }
        }

        bool IDamageable.CanReceiveDamage() => health.isDepleted is false;

        void IDamageable.ReceiveDamage(float amount) => health.DecreaseCurrentHealth(amount);

        Health IDamageable.GetHealth() => health;
    }
}