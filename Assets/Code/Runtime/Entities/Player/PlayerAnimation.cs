using System;
using KBCore.Refs;
using R3;
using UnityEngine;
using UnityEngine.AI;

namespace SwapChains.Runtime.Entities
{
    public class PlayerAnimation : ValidatedMonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Range(0.1f, 1f)] float smoothTime = 0.1f;
        [Header("Refs")]
        [SerializeField, Parent] PlayerController controller;
        [SerializeField, Child] Animator animator;
        [SerializeField, Parent] NavMeshAgent agent;
        float currentSpeed = 0f;
        float currentVelocity = 0f;
        readonly int Speed = Animator.StringToHash("Speed");
        IDisposable animationSubscription;

        void Start() => ApplyAnimation();

        void OnDestroy() => animationSubscription?.Dispose();

        void ApplyAnimation()
        {
            animationSubscription = Observable.EveryValueChanged(agent, a => a.desiredVelocity.magnitude).Subscribe(desiredVelocity =>
            {
                var smoothDamp = Mathf.SmoothDamp(currentSpeed, desiredVelocity, ref currentVelocity, smoothTime);
                currentSpeed = controller.DeltaTime != 0f ? smoothDamp : 0f;
                animator.SetFloat(Speed, currentSpeed);
            }).AddTo(this);
        }
    }
}