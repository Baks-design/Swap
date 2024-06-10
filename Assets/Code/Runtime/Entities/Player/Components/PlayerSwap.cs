using System;
using System.Buffers;
using System.Collections;
using KBCore.Refs;
using SwapChains.Runtime.Utilities.ServicesLocator;
using SwapChains.Runtime.VFX;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerSwap : MonoBehaviour
    {
        [Header("Swap Settings")]
        [SerializeField] LayerMask swapLayer;
        [SerializeField, Range(10f, 50f)] float swapRange = 50f;
        [SerializeField, Range(0.1f, 1f)] float swapfireRate = 0.25f;

        [Header("Reveal Settings")]
        [SerializeField] LayerMask revealLayer;
        [SerializeField, Range(0.1f, 1f)] float revealfireRate = 0.7f;
        [SerializeField, Range(1f, 50f)] float revealDistance = 20f;

        [Header("Refs")]
        [SerializeField, Self] InterfaceRef<IPlayerInput> playerInput;

        float nextFire = 0f;
        Coroutine revealEffectCoroutine;
        Coroutine swapRoutine;
        InterfaceEffectsController interfaceEffects;
        readonly WaitForEndOfFrame endOfFrame;
        readonly WaitForSeconds timeForEndTransition = new(1f);

        void OnValidate() => this.ValidateRefs();

        void Awake() => ServiceLocator.Global.Register(interfaceEffects);

        void Update()
        {
            SwapHandle();
            RevealEnemyHandle();
        }

        void SwapHandle()
        {
            if (playerInput.Value.GetSelectBody() && playerInput.Value.GetSwitchBody() && Time.time > nextFire) //TODO: CHECK 
            {
                nextFire = Time.time + swapfireRate;

                Ray ray = default;
                ray.origin = Camera.main.transform.position;
                ray.direction = Camera.main.transform.forward;
                var raycastHitBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);

                var hitCount = Physics.RaycastNonAlloc(ray, raycastHitBuffer, swapRange, swapLayer, QueryTriggerInteraction.Ignore);
                for (var i = 0; i < hitCount; i++)
                    swapRoutine ??= StartCoroutine(SwapRoutine(raycastHitBuffer[i]));
            }
        }

        IEnumerator SwapRoutine(RaycastHit hitPoint)
        {
            swapRoutine = null;

            ServiceLocator.For(this).Get(out interfaceEffects);
            interfaceEffects.ActiveTransition(true);

            if (hitPoint.transform.TryGetComponent<PlayerController>(out var outherController))
                outherController.NPCComponentsHandle(true);
            outherController.NPCComponentsHandle(false);

            yield return timeForEndTransition;

            interfaceEffects.ActiveTransition(false);
        }

        void RevealEnemyHandle()
        {
            if (playerInput.Value.GetShowBody() && Time.time > nextFire)
            {
                nextFire = Time.time + revealfireRate;
                revealEffectCoroutine ??= StartCoroutine(RevealEnemyRoutine());
            }
        }

        IEnumerator RevealEnemyRoutine()
        {
            revealEffectCoroutine = null;

            Ray ray = default;
            ray.origin = Camera.main.transform.position;
            ray.direction = Camera.main.transform.forward;
            var raycastHitBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);

            var hitCount = Physics.RaycastNonAlloc(ray, raycastHitBuffer, revealDistance, revealLayer, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < hitCount; i++)
                if (raycastHitBuffer[i].collider.TryGetComponent<MeshRenderer>(out var renderer))
                    if (!renderer.enabled)
                        renderer.enabled = true;

            yield return endOfFrame;
        }
    }
}