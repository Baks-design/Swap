using System;
using System.Linq;
using KBCore.Refs;
using SwapChains.Runtime.Audio;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;
using UnityEngine.Audio;

namespace SwapChains.Runtime.Entities.Player
{
    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class PlayerSound : MonoBehaviour, IHealthListener
    {
        [Header("Settings")]
        [SerializeField] float radius = 0.1f;
        [SerializeField] float TimeBetweenFootstep = 0.5f;
        [SerializeField] float MaxFallVelocityThreshold = 4.0f;

        [Header("Sources")]
        [SerializeField] AudioSource m_HealthChangeAudioSource;
        [SerializeField] AudioSource m_HealthDepleatedAudioSource;
        [SerializeField] AudioSource m_FootstepAudioSource;
        [SerializeField] AudioSource m_LandedFootstepAudioSource;
        [SerializeField] AudioSource m_LandedSharedAudioSource;

        [Header("Resources")]
        [SerializeField] AudioResource m_LandedSharedAudioResource;
        [SerializeField] AudioResource m_HealthDepleatedAudioResource;
        [SerializeField] AudioResource m_HealthChangeAudioResource;

        [Header("Materials")]
        [SerializeField] MaterialMatchEntry[] MaterialMatchList;

        [Header("Refs")]
        [SerializeField, Self] KinematicCharacterMotor Motor;
        [SerializeField, Self] InterfaceRef<IDamageable> damageable;

        bool m_WasGroundedInLastFrame = true;
        float m_SinceLastFootStep = 0f;
        float m_MaxFallVelocity = 0f;
        readonly Collider[] m_CollidersBuffer = new Collider[16];
        Transform _transform;

        void OnValidate() => this.ValidateRefs();

        void OnEnable()
        {
            GameEvents.OnDamageableLoaded += OnDamageableLoaded;
            damageable?.Value.GetHealth().Register(this);
        }

        void Start()
        {
            _transform = transform;
            m_LandedSharedAudioSource.resource = m_LandedSharedAudioResource;
            m_HealthChangeAudioSource.resource = m_HealthChangeAudioResource;
            m_HealthDepleatedAudioSource.resource = m_HealthDepleatedAudioResource;
        }

        void FixedUpdate() => FootStepsHandle();

        void OnDisable()
        {
            GameEvents.OnDamageableLoaded -= OnDamageableLoaded;
            damageable?.Value.GetHealth().Unregister(this);
        }

        #region Damage
        void OnDamageableLoaded(Component obj)
        {
            damageable?.Value.GetHealth().Unregister(this);
            damageable = (InterfaceRef<IDamageable>)(obj as IDamageable);
            damageable?.Value.GetHealth().Register(this);
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => m_HealthChangeAudioSource.Play();

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => m_HealthDepleatedAudioSource.Play();
        #endregion

        #region Footsteps
        void FootStepsHandle()
        {
            // Check for grounding and falling
            if (!Motor.GroundingStatus.IsStableOnGround)
                m_MaxFallVelocity = Mathf.Max(m_MaxFallVelocity, Motor.Velocity.y);

            // Check for landing
            var landed = Motor.GroundingStatus.IsStableOnGround && !m_WasGroundedInLastFrame && m_MaxFallVelocity >= MaxFallVelocityThreshold;
            if (landed)
            {
                m_LandedFootstepAudioSource.Play();
                m_LandedSharedAudioSource.Play();
                m_MaxFallVelocity = 0f;
            }

            // Update grounded status
            m_WasGroundedInLastFrame = Motor.GroundingStatus.IsStableOnGround;

            // Check for movement and grounding before proceeding
            if (Motor.Velocity.sqrMagnitude < 0.04f || !Motor.GroundingStatus.IsStableOnGround)
            {
                m_SinceLastFootStep = 0f;
                return;
            }

            // Timing for footstep sounds
            m_SinceLastFootStep += Time.deltaTime;
            if (m_SinceLastFootStep >= TimeBetweenFootstep)
            {
                while (m_SinceLastFootStep >= TimeBetweenFootstep)
                    m_SinceLastFootStep -= TimeBetweenFootstep;
                m_FootstepAudioSource.Play();
            }

            // Collision detection for footsteps
            var count = Physics.OverlapSphereNonAlloc(
                _transform.position, radius, m_CollidersBuffer, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            // Processing collisions to determine footstep sounds
            for (var i = 0; i < count; ++i)
            {
                var collider = m_CollidersBuffer[i];
                var renderer = collider.gameObject.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    var mats = renderer.sharedMaterials;
                    for (var j = 0; j < mats.Length; j++)
                    {
                        for (var k = 0; k < MaterialMatchList.Length; k++)
                        {
                            if (MaterialMatchList[i].Materials.Contains(mats[j]))
                            {
                                if (m_FootstepAudioSource.resource != MaterialMatchList[i].RandomContainer)
                                    SetFootstepContainer(MaterialMatchList[i].RandomContainer, landed);
                                return;
                            }
                        }
                    }
                }
            }
        }

        void SetFootstepContainer(AudioResource container, bool playNow)
        {
            m_FootstepAudioSource.Stop();
            m_FootstepAudioSource.resource = container;
            m_LandedFootstepAudioSource.resource = container;

            if (playNow)
                m_FootstepAudioSource.Play();
        }
        #endregion
    }
}