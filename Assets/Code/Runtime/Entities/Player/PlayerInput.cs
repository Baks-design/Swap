using R3;
using R3.Triggers;
using SwapChains.Runtime.Utilities.Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public Observable<Vector2> Mouselook { get; private set; }
        public Observable<Vector2> Movement { get; private set; }
        public ReadOnlyReactiveProperty<bool> Run { get; private set; }
        public ReadOnlyReactiveProperty<bool> SelectionBody { get; private set; }
        public Observable<Unit> Jump { get; private set; }
        public Observable<Unit> Shoot { get; private set; }
        public Observable<Unit> ShowBody { get; private set; }
        public Observable<Unit> SwitchBody { get; private set; }
        public Observable<MoveInputs> Inputs { get; private set; }

        void Awake()
        {
            var movement = InputSystem.actions.FindAction("Move");
            // Movement inputs tick on FixedUpdate
            Movement = this.FixedUpdateAsObservable().Select(_ => { return movement.ReadValue<Vector2>(); });

            var sprint = InputSystem.actions.FindAction("Sprint");
            // Run while held.
            Run = this.UpdateAsObservable().Select(_ => sprint.IsPressed()).ToReadOnlyReactiveProperty();

            var jump = InputSystem.actions.FindAction("Jump");
            // Jump: sample during Update...
            Jump = this.UpdateAsObservable().Where(_ => jump.WasPressedThisFrame());

            // ... But latch it until FixedUpdate.
            var jumpLatch = CustomObservables.Latch(this.FixedUpdateAsObservable(), Jump, false);

            // Now zip jump and movement together so we can handle them at the same time.
            // Zip only works here because both Movement and jumpLatch will emit at the same
            // frequency: during FixedUpdate.
            Inputs = Movement.Zip(jumpLatch, (m, j) => new MoveInputs(m, j));

            var look = InputSystem.actions.FindAction("Look");
            // Mouse look ticks on Update
            Mouselook = this.UpdateAsObservable().Select(_ => { return look.ReadValue<Vector2>(); });

            var shoot = InputSystem.actions.FindAction("Shoot");
            Shoot = this.UpdateAsObservable().Where(_ => shoot.WasPressedThisFrame());

            var showBody = InputSystem.actions.FindAction("ShowBody");
            ShowBody = this.UpdateAsObservable().Where(_ => showBody.WasPressedThisFrame());

            var selectionBody = InputSystem.actions.FindAction("SelectBody");
            SelectionBody = this.UpdateAsObservable().Select(_ => selectionBody.IsPressed()).ToReadOnlyReactiveProperty();

            var switchBody = InputSystem.actions.FindAction("SwitchBody");
            SwitchBody = this.UpdateAsObservable().Where(_ => switchBody.WasPressedThisFrame());
        }

        public readonly struct MoveInputs
        {
            public readonly Vector2 movement;
            public readonly bool jump;

            public MoveInputs(Vector2 movement, bool jump)
            {
                this.movement = movement;
                this.jump = jump;
            }
        }
    }
}
