using KBCore.Refs;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Self] Animator CharacterAnimator;
        [SerializeField, Parent] KinematicCharacterMotor Motor;
        [SerializeField, Parent] InterfaceRef<IPlayerInput> input;

        [Header("Settings")]
        [SerializeField] float ForwardAxisSharpness = 1f;
        [SerializeField] float TurnAxisSharpness = 5f;

        Quaternion _rootMotionRotationDelta;
        Vector3 _rootMotionPositionDelta;
        static readonly int ForwardHash = Animator.StringToHash("Forward");
        static readonly int TurnHash = Animator.StringToHash("Turn");
        static readonly int OnGroundHash = Animator.StringToHash("OnGround");

        public Quaternion RootMotionRotationDelta
        {
            get => _rootMotionRotationDelta;
            set => _rootMotionRotationDelta = value;
        }
        public Vector3 RootMotionPositionDelta
        {
            get => _rootMotionPositionDelta;
            set => _rootMotionPositionDelta = value;
        }

        void OnValidate() => this.ValidateRefs();

        void Start()
        {
            _rootMotionPositionDelta = Vector3.zero;
            _rootMotionRotationDelta = Quaternion.identity;
        }

        void Update() => Animation();

        void Animation()
        {
            CharacterAnimator.SetBool(OnGroundHash, Motor.GroundingStatus.IsStableOnGround);
            //CharacterAnimator.SetBool(JumpHash, !Motor.GroundingStatus.IsStableOnGround);

            float _forwardAxis = default;
            _forwardAxis = Mathf.Lerp(_forwardAxis, input.Value.GetMovement().y, 1f - Mathf.Exp(-ForwardAxisSharpness * Time.deltaTime));
            print(_forwardAxis);
            float _rightAxis = default;
            _rightAxis = Mathf.Lerp(_rightAxis, input.Value.GetMovement().x, 1f - Mathf.Exp(-TurnAxisSharpness * Time.deltaTime));
            CharacterAnimator.SetFloat(ForwardHash, _forwardAxis);
            CharacterAnimator.SetFloat(TurnHash, _rightAxis);

            // if (Motor.GroundingStatus.IsStableOnGround)
            //     CharacterAnimator.SetBool(FreeFallHash, false);
            // if (playerMovement.FallTimeoutDelta <= 0f)
            //     CharacterAnimator.SetBool(FreeFallHash, true);
        }

        void OnAnimatorMove()
        {
            // Accumulate rootMotion deltas between character updates 
            _rootMotionPositionDelta += CharacterAnimator.deltaPosition;
            _rootMotionRotationDelta = CharacterAnimator.deltaRotation * _rootMotionRotationDelta;
        }
    }
}