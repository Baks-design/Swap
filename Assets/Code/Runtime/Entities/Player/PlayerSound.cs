using System;
using System.Linq;
using KBCore.Refs;
using SwapChains.Runtime.Entities;
using UnityEngine;
using XIV.DesignPatterns.Common.Extensions;
using XIV.DesignPatterns.Common.HealthSystem;
using Random = UnityEngine.Random;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class PlayerSound : ValidatedMonoBehaviour, IHealthListener
    {
        [Header("Settings")]
        [SerializeField, Range(0.1f, 1f)] float groundCheckRadius = 0.1f;
        [Header("Refs")]
        [SerializeField, Parent] PlayerController controller;
        [SerializeField, Anywhere] PlayerMovement movement;
        [SerializeField, Anywhere] AudioSource footStepsSource;
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] hurtAudioClips;
        [SerializeField] AudioClip[] deadAudioClips;
        [SerializeField] MaterialMatchEntry[] MaterialMatchList;
        IDamageable damageable;
        readonly Collider[] m_CollidersBuffer = new Collider[16];

        void OnEnable()
        {
            GameEvents.onDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void Start() => FootstepsHandle();

        void OnDisable()
        {
            GameEvents.onDamageableLoaded -= OnDamageableLoaded;
            damageable?.GetHealth().Unregister(this);
        }

        void Play(AudioClip clip)
        {
            var value = Random.value;
            audioSource.pitch = value < 0.5f ? value + 0.5f : value;
            audioSource.PlayOneShot(clip);
        }

        void FootstepsHandle()
        {
            //movement.Stepped.Subscribe(clips =>
            //{
                var count = Physics.OverlapSphereNonAlloc(controller.Transform.localPosition, groundCheckRadius, m_CollidersBuffer, ~(1 << 30));
                for (var i = 0; i < count; ++i)
                {
                    var renderer = m_CollidersBuffer[i].gameObject.GetComponentInChildren<Renderer>();
                    if (renderer)
                    {
                        for (var j = 0; j < renderer.sharedMaterials.Length; j++)
                        {
                            for (var k = 0; k < MaterialMatchList.Length; k++)
                            {
                                if (MaterialMatchList[i].Materials.Contains(renderer.sharedMaterials[i]))
                                {
                                    if (footStepsSource.resource != MaterialMatchList[i].RandomContainer)
                                    {
                                        footStepsSource.Stop();
                                        footStepsSource.resource = MaterialMatchList[i].RandomContainer;
                                        footStepsSource.Play();
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
           // }).AddTo(this);
        }

        #region Health
        void OnDamageableLoaded(Component obj)
        {
            damageable?.GetHealth().Unregister(this);
            damageable = obj as IDamageable;
            damageable?.GetHealth().Register(this);
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => Play(hurtAudioClips.PickRandom());

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => Play(deadAudioClips.PickRandom());
        #endregion
    }
}