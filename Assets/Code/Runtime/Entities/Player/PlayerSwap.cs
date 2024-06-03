using System;
using System.Collections;
using SwapChains.Runtime.Utilities.Helpers;
using SwapChains.Runtime.Utilities.ServicesLocator;
using SwapChains.Runtime.VFX;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    [Serializable]
    public class PlayerSwap
    {
        [Header("Swap Settings")]
        [SerializeField] LayerMask swapLayer;
        [SerializeField, Range(10f, 50f)] float swapRange = 50f;
        [SerializeField, Range(0.1f, 1f)] float swapfireRate = 0.25f;

        [Header("Reveal Settings")]
        [SerializeField] LayerMask revealLayer;
        [SerializeField, Range(0.1f, 1f)] float revealfireRate = 0.7f;
        [SerializeField, Range(1f, 50f)] float revealDistance = 20f;

        float nextFire = 0f;
        (bool, RaycastHit) raycast = (false, new());
        Coroutine revealEffectCoroutine;
        Coroutine swapRoutine;
        InterfaceEffectsController interfaceEffects;
        readonly WaitForEndOfFrame endOfFrame;
        readonly WaitForSeconds timeForEndTransition = new(1f);
        readonly RaycastHit[] hits = new RaycastHit[1];

        public void Update(PlayerController controller)
        {
            SwapHandle(controller);
            RevealEnemyHandle(controller);
        }

        void SwapHandle(PlayerController controller)
        {
            if (controller.GameTime > nextFire)
            {
                nextFire = controller.GameTime + swapfireRate;

                (raycast.Item1, raycast.Item2) = GameHelper.CheckInteraction(
                    controller.Camera, hits, swapRange, swapLayer, QueryTriggerInteraction.Ignore);

                if (raycast.Item1)
                    swapRoutine ??= controller.StartCoroutine(SwapRoutine(raycast.Item2, controller));
            }
        }

        IEnumerator SwapRoutine(RaycastHit hitPoint, PlayerController controller)
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

        void RevealEnemyHandle(PlayerController controller)
        {
            if (controller.GameTime > nextFire)
            {
                nextFire = controller.GameTime + revealfireRate;
                revealEffectCoroutine ??= controller.StartCoroutine(RevealEnemyRoutine(controller));
            }
        }

        IEnumerator RevealEnemyRoutine(PlayerController controller)
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