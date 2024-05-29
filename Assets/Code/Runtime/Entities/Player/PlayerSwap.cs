using System;
using System.Collections;
using KBCore.Refs;
using R3;
using SwapChains.Runtime.UserInterface;
using SwapChains.Runtime.Utilities.Helpers;
using SwapChains.Runtime.Utilities.ServicesLocator;
using UnityEngine;

namespace SwapChains.Runtime.Entities
{
    public class PlayerSwap : ValidatedMonoBehaviour
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
        [SerializeField, Self] PlayerController controller;
        [SerializeField, Self] PlayerInput input;
        float nextFire = 0f;
        (bool, RaycastHit) raycast = (false, new());
        Coroutine revealEffectCoroutine;
        Coroutine swapRoutine;
        InterfaceEffectsController interfaceEffects;
        readonly WaitForEndOfFrame endOfFrame;
        readonly WaitForSeconds timeForEndTransition = new(1f);
        readonly RaycastHit[] hits = new RaycastHit[1];

        void Start()
        {
            SwapHandle();
            RevealEnemyHandle();
        }

        void SwapHandle()
        {
            input.SwitchBody.Subscribe(_ =>
            {
                if (controller.GameTime > nextFire)
                {
                    nextFire = controller.GameTime + swapfireRate;

                    (raycast.Item1, raycast.Item2) = GameHelper.CheckInteraction(
                        controller.Camera, hits, swapRange, swapLayer, QueryTriggerInteraction.Ignore);
                    if (raycast.Item1)
                        swapRoutine ??= StartCoroutine(SwapRoutine(raycast.Item2));
                }
            }).AddTo(this);
        }

        IEnumerator SwapRoutine(RaycastHit hitPoint)
        {
            swapRoutine = null;

            ServiceLocator.Global.Get(out interfaceEffects);
            interfaceEffects.ActiveTransition(true);

            if (hitPoint.transform.TryGetComponent<PlayerController>(out var outherController))
                outherController.NPCComponentsHandle(true);
            controller.NPCComponentsHandle(false);

            yield return timeForEndTransition;

            interfaceEffects.ActiveTransition(false);
        }

        void RevealEnemyHandle()
        {
            input.ShowBody.Subscribe(_ =>
            {
                if (controller.GameTime > nextFire)
                {
                    nextFire = controller.GameTime + revealfireRate;
                    revealEffectCoroutine ??= StartCoroutine(RevealEnemyRoutine());
                }
            }).AddTo(this);
        }

        IEnumerator RevealEnemyRoutine()
        {
            revealEffectCoroutine = null;

            (raycast.Item1, raycast.Item2) = GameHelper.CheckInteraction(
                controller.Camera, hits, revealDistance, revealLayer, QueryTriggerInteraction.Ignore);
            if (raycast.Item1 && raycast.Item2.collider.TryGetComponent<MeshRenderer>(out var renderer))
                if (!renderer.enabled)
                    renderer.enabled = true;

            yield return endOfFrame;
        }
    }
}