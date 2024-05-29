using System;
using KBCore.Refs;
using R3;
using UnityEngine;

namespace SwapChains.Runtime.Entities
{
    public class PlayerMovement : PlayerSignals
    {
        [Header("Settings")]
        [SerializeField, Range(1f, 10f)] float walkSpeed = 5f;
        [SerializeField, Range(1f, 10f)] float runSpeed = 10f;
        [SerializeField, Range(1f, 10f)] float jumpSpeed = 2f;
        [SerializeField, Range(1f, 10f)] float strideLength = 2.5f;
        [SerializeField, Range(1f, 10f)] float stickToGround = 5f;
        [Header("Refs")]
        [SerializeField, Self] CharacterController character;
        [SerializeField, Self] PlayerInput input;
        [SerializeField, Self] PlayerController controller;
        Subject<Vector3> walked;
        Subject<Unit> landed;
        Subject<Unit> jumped;
        Subject<Unit> stepped;

        public override float StrideLength => strideLength;
        public override Observable<Vector3> Walked => walked;
        public override Observable<Unit> Landed => landed;
        public override Observable<Unit> Jumped => jumped;
        public override Observable<Unit> Stepped => stepped;

        void Awake()
        {
            walked = new Subject<Vector3>().AddTo(this);
            jumped = new Subject<Unit>().AddTo(this);
            landed = new Subject<Unit>().AddTo(this);
            stepped = new Subject<Unit>().AddTo(this);
        }

        void Start() => ApplyMovement();

        void ApplyMovement()
        {
            character.Move(-stickToGround * controller.Transform.up);

            // Handle movement input (WASD-style) with run (Shift).
            input.Inputs.Subscribe(i =>
            {
                // Note: CharacterController is a stateful object. But as long as I only modify it from this
                // function, I can be reasonably sure things will work as expected.
                var wasGrounded = character.isGrounded;

                // Vertical movements (jumping and gravity) are the player's y-axis.
                var verticalVelocity = 0f;
                if (i.jump && wasGrounded)
                {
                    // We're on the ground and want to jump.
                    verticalVelocity = jumpSpeed;
                    jumped.OnNext(Unit.Default);
                }
                else if (!wasGrounded)
                    // We're in the air: apply gravity.
                    verticalVelocity = character.velocity.y + controller.Gravity * controller.FixedDeltaTime;
                else
                    // We're otherwise on the ground: push us down a little.
                    // (Required for character.isGrounded to work.)
                    verticalVelocity = -Mathf.Abs(stickToGround);

                // Horizontal movements are the player's x- and z-axes.
                // Calculate velocity (direction * speed).
                var horizontalVelocity = i.movement * (input.Run.CurrentValue ? runSpeed : walkSpeed);

                // Combine horizontal and vertical into player coordinate space.
                Vector3 directional = default;
                directional.x = horizontalVelocity.x; // input x (+/-) corresponds to strafe right/left (player x-axis)
                directional.y = verticalVelocity;
                directional.z = horizontalVelocity.y; // input y (+/-) corresponds to forward/back (player z-axis)
                var playerVelocity = controller.Transform.TransformVector(directional);

                // Apply movement.
                var distance = playerVelocity * controller.FixedDeltaTime;
                character.Move(distance);

                // "Output" signals.
                if (wasGrounded && character.isGrounded)
                    // Both started and ended this frame on the ground.
                    walked.OnNext(character.velocity * controller.FixedDeltaTime);
                if (!wasGrounded && character.isGrounded)
                    // Didn't start on the ground, but ended up there.
                    landed.OnNext(Unit.Default);
            }).AddTo(this);

            // Track distance walked to emit step events.
            var stepDistance = 0f;
            Walked.Subscribe(w =>
            {
                stepDistance += w.magnitude;
                if (stepDistance > strideLength)
                    stepped.OnNext(Unit.Default);
                stepDistance %= strideLength;
            }).AddTo(this);
        }
    }
}