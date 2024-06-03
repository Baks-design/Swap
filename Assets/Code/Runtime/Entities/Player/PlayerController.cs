using System;
using KBCore.Refs;
using SwapChains.Runtime.Audio;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Entities.Player.Movement;
using SwapChains.Runtime.Utilities.Extensions;
using SwapChains.Runtime.Utilities.Helpers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerController : MonoBehaviour, IDamageable, IHealthListener
    {
        [Header("Refs")]
        [SerializeField, Child] CinemachineCamera cinemachine;
        [SerializeField, Child] SkinnedMeshRenderer[] meshRenderers;
        IDamageable damageable;

        [Header("Components")]
        [SerializeField] Health health = new();
        [SerializeField] PlayerAiming aiming = new();
        [SerializeField] PlayerSwap swap = new();
        [SerializeField] PlayerSound sound = new();
        [SerializeField] SurfCharacter character = new();

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

            health.Awake();
            aiming.Awake();
            character.Awake();

            GameEvents.InvokeOnDamageableLoaded(this);
        }

        void OnEnable()
        {
            GameEvents.OnDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void Start() => character.Start(this);

        void FixedUpdate() => FixedDeltaTime = Time.fixedDeltaTime;

        void Update()
        {
            DeltaTime = Time.deltaTime;
            GameTime = Time.time;

            //FIXME: swap.Update(this); bugado
            aiming.Update(this);
            character.Update(this);
            sound.Update(this);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!character.Triggers.Contains(other))
                character.Triggers.Add(other);
        }

        void OnCollisionStay(Collision collision)
        {
            if (collision.rigidbody == null)
                return;

            var relativeVelocity = collision.relativeVelocity * collision.rigidbody.mass / 50f;
            Vector3 impactVelocity;
            impactVelocity.x = relativeVelocity.x * 0.0025f;
            impactVelocity.y = relativeVelocity.y * 0.00025f;
            impactVelocity.z = relativeVelocity.z * 0.0025f;

            var maxYVel = Mathf.Max(character.MoveData.velocity.y, 10f);
            Vector3 newVelocity;
            newVelocity.x = character.MoveData.velocity.x + impactVelocity.x;
            newVelocity.y = Mathf.Clamp(character.MoveData.velocity.y + Mathf.Clamp(impactVelocity.y, -0.5f, 0.5f), -maxYVel, maxYVel);
            newVelocity.z = character.MoveData.velocity.z + impactVelocity.z;

            newVelocity = Vector3.ClampMagnitude(newVelocity, Mathf.Max(character.MoveData.velocity.magnitude, 30f));
            character.MoveData.velocity = newVelocity;
        }

        void OnTriggerExit(Collider other)
        {
            if (character.Triggers.Contains(other))
                character.Triggers.Remove(other);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, character.ColliderSize);
        }

        void OnDisable()
        {
            GameEvents.OnDamageableLoaded -= OnDamageableLoaded;
            damageable?.GetHealth().Unregister(this);
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

        #region Health
        bool IDamageable.CanReceiveDamage() => health.IsDepleted is false;

        void IDamageable.ReceiveDamage(float amount) => health.DecreaseCurrentHealth(amount);

        Health IDamageable.GetHealth() => health;

        void OnDamageableLoaded(Component obj)
        {
            damageable?.GetHealth().Unregister(this);
            damageable = obj as IDamageable;
            damageable?.GetHealth().Register(this);
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => sound.Play(sound.HurtAudioClips.PickRandom());

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => sound.Play(sound.DeadAudioClips.PickRandom());
        #endregion
    }
}