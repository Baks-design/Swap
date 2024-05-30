using System;
using KBCore.Refs;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField, Range(-90f, 0f)] float minViewAngle = -60f;
        [SerializeField, Range(0f, 90f)] float maxViewAngle = 60f;
        [Header("FOV Settings")]
        [SerializeField, Range(30f, 50f)] float targetFOVMin = 30f;
        [SerializeField, Range(60f, 90f)] float targetFOVMax = 60f;
        [SerializeField, Range(1f, 10f)] float zoomSpeed = 1f;
        [Header("HeadBob Settings")]
        [SerializeField, Range(0.01f, 0.1f)] float walkBobMagnitude = 0.05f;
        [SerializeField, Range(0.1f, 0.1f)] float runBobMagnitude = 0.10f;
        [SerializeField]
        AnimationCurve bob = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.50f, 0f),
            new Keyframe(0.75f, -1f),
            new Keyframe(1f, 0f));
        Vector3 initialPosition = new(0f, 0f, 0f);
        [Header("Refs Settings")]
        [SerializeField, Parent] PlayerController controller;
        [SerializeField, Parent] PlayerInput input;
        [SerializeField, Anywhere] PlayerMovement movement;
        [SerializeField, Anywhere] Transform player;
        [SerializeField, Child] CinemachineCamera virtualCamera;

        void OnValidate() => this.ValidateRefs();

        void Awake() => initialPosition = virtualCamera.transform.localPosition;

        void Start()
        {
            ApplyCameraMovement();
            ApplyCameraFOV();
            HeadBob();
        }

        void ApplyCameraMovement()
        {
            input.Mouselook.Where(v => v != Vector2.zero).Subscribe(inputLook =>
            {
                //horizontal
                var horzLook = inputLook.x * controller.DeltaTime * Vector3.up;
                player.localRotation *= Quaternion.Euler(horzLook);

                //vertical
                var vertLook = inputLook.y * controller.DeltaTime * Vector3.left;
                var newQ = virtualCamera.transform.localRotation * Quaternion.Euler(vertLook);

                //Apply
                virtualCamera.transform.localRotation = ClampRotationAroundXAxis(newQ, -maxViewAngle, -minViewAngle);
            }).AddTo(this);
        }

        static Quaternion ClampRotationAroundXAxis(Quaternion q, float minAngle, float maxAngle)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1f;
            var angleX = 2f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minAngle, maxAngle);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
            return q;
        }

        void ApplyCameraFOV()
        {
            input.SelectionBody.Subscribe(inputSelection =>
            {
                var currentValue = inputSelection ? 30 : 60;
                SetFOV(currentValue);
            }).AddTo(this);
        }

        void SetFOV(float target)
        {
            var currentFOV = virtualCamera.Lens.FieldOfView;
            var targetFOV = Mathf.Clamp(target, targetFOVMin, targetFOVMax);
            var time = controller.DeltaTime * zoomSpeed;
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, time);
        }

        void HeadBob()
        {
            var distance = 0f;
            movement.Walked.Subscribe(w =>
            {
                // Accumulate distance walked (modulo stride length).
                distance += w.magnitude;
                distance %= movement.StrideLength;

                // Use distance to evaluate the bob curve.
                var magnitude = input.Run.CurrentValue ? runBobMagnitude : walkBobMagnitude;
                var deltaPos = magnitude * bob.Evaluate(distance / movement.StrideLength) * Vector3.up;

                // Adjust camera position.
                virtualCamera.transform.localPosition = initialPosition + deltaPos;
            }).AddTo(this);
        }
    }
}