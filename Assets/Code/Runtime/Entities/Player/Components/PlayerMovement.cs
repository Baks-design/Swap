using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using SwapChains.Runtime.Entities.Environment;

namespace SwapChains.Runtime.Entities.Player
{
    public enum CharacterState { Default, Charging, NoClip, Swimming, Climbing }

    public enum ClimbingState { Anchoring, Climbing, DeAnchoring }

    public class PlayerMovement : MonoBehaviour, ICharacterController
    {
        [Header("Stable Movement")]
        [SerializeField] float MaxStableMoveSpeed = 10f;
        [SerializeField] float StableMovementSharpness = 15f;
        [SerializeField] float OrientationSharpness = 10f;
        bool _isStopped = false;
        bool _shouldBeCrouching = false;
        bool _isCrouching = false;
        bool _mustStopVelocity = false;
        bool _crouchInputIsHeld = false;
        Vector3 _internalVelocityAdd = Vector3.zero;
        Vector3 _wallJumpNormal = Vector3.zero;
        Vector3 _moveInputVector = Vector3.zero;
        Vector3 _lookInputVector = Vector3.zero;
        readonly Collider[] _probedColliders = new Collider[8];

        [Header("Air Movement")]
        [SerializeField] float MaxAirMoveSpeed = 10f;
        [SerializeField] float AirAccelerationSpeed = 5f;
        [SerializeField] float Drag = 0.1f;
        [SerializeField] float FallTimeout = 0.15f;
        bool _canWallJump = false;

        [Header("Jumping")]
        [SerializeField] bool AllowJumpingWhenSliding = false;
        [SerializeField] bool AllowDoubleJump = false;
        [SerializeField] bool AllowWallJump = false;
        [SerializeField] float JumpSpeed = 10f;
        [SerializeField] float JumpPreGroundingGraceTime = 0f;
        [SerializeField] float JumpPostGroundingGraceTime = 0f;
        bool _jumpRequested = false;
        bool _jumpConsumed = false;
        bool _doubleJumpConsumed = false;
        bool _jumpedThisFrame = false;
        bool _jumpInputIsHeld = false;
        float _timeSinceJumpRequested = Mathf.Infinity;
        float _timeSinceLastAbleToJump = 0f;

        [Header("Charging")]
        [SerializeField] float ChargeSpeed = 15f;
        [SerializeField] float MaxChargeTime = 1.5f;
        [SerializeField] float StoppedTime = 1f;
        float _timeSinceStartedCharge = 0f;
        float _timeSinceStopped = 0f;
        Vector3 _currentChargeVelocity = Vector3.zero;

        [Header("NoClip")]
        [SerializeField] float NoClipMoveSpeed = 10f;
        [SerializeField] float NoClipSharpness = 15f;

        [Header("Swimming")]
        [SerializeField] LayerMask WaterLayer;
        [SerializeField] Transform SwimmingReferencePoint;
        [SerializeField] float SwimmingSpeed = 4f;
        [SerializeField] float SwimmingMovementSharpness = 3f;
        Collider _waterZone = new();

        [Header("Ladder Climbing")]
        [SerializeField] LayerMask InteractionLayer;
        [SerializeField] float ClimbingSpeed = 4f;
        [SerializeField] float AnchoringDuration = 0.25f;
        float _onLadderSegmentState = 0f;
        float _anchoringTimer = 0f;
        float _ladderUpDownInput = 0f;
        Vector3 _ladderTargetPosition = Vector3.zero;
        Vector3 _anchoringStartPosition = Vector3.zero;
        Quaternion _ladderTargetRotation = Quaternion.identity;
        Quaternion _anchoringStartRotation = Quaternion.identity;
        Quaternion _rotationBeforeClimbing = Quaternion.identity;
        ClimbingState _internalClimbingState;
        Ladder ActiveLadder { get; set; }
        ClimbingState ClimbingState
        {
            get => _internalClimbingState;
            set
            {
                _internalClimbingState = value;
                _anchoringTimer = 0f;
                _anchoringStartPosition = Motor.TransientPosition;
                _anchoringStartRotation = Motor.TransientRotation;
            }
        }

        [Header("Misc")]
        [SerializeField, Self] KinematicCharacterMotor Motor;
        [SerializeField] List<Collider> IgnoredColliders;
        [SerializeField] bool OrientTowardsGravity = false;
        [SerializeField] Vector3 Gravity = new(0f, -30f, 0f);
        [SerializeField] Transform MeshRoot;
        [SerializeField, Child] PlayerAnimation playerAnimation;
        [SerializeField, Self] InterfaceRef<IPlayerInput> input;
        Camera cam = new();
        public CharacterState CurrentCharacterState { get; set; }

        void OnValidate() => this.ValidateRefs();

        void Awake()
        {
            Motor.CharacterController = this;
            TransitionToState(CharacterState.Default);
            cam = Camera.main;
        }

        void Update() => SetInputs();

        #region State Machine
        public void TransitionToState(CharacterState newState)
        {
            var tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Charging:
                    {
                        _currentChargeVelocity = Motor.CharacterForward * ChargeSpeed;
                        _isStopped = false;
                        _timeSinceStartedCharge = 0f;
                        _timeSinceStopped = 0f;
                        break;
                    }
                case CharacterState.NoClip:
                    {
                        Motor.SetCapsuleCollisionsActivation(false);
                        Motor.SetMovementCollisionsSolvingActivation(false);
                        Motor.SetGroundSolvingActivation(false);
                        break;
                    }
                case CharacterState.Swimming:
                    {
                        Motor.SetGroundSolvingActivation(false);
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        _rotationBeforeClimbing = Motor.TransientRotation;

                        Motor.SetMovementCollisionsSolvingActivation(false);
                        Motor.SetGroundSolvingActivation(false);
                        ClimbingState = ClimbingState.Anchoring;

                        // Store the target position and rotation to snap to
                        _ladderTargetPosition = ActiveLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                        _ladderTargetRotation = ActiveLadder.transform.rotation;
                        break;
                    }
            }
        }

        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.NoClip:
                    {
                        Motor.SetCapsuleCollisionsActivation(true);
                        Motor.SetMovementCollisionsSolvingActivation(true);
                        Motor.SetGroundSolvingActivation(true);
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        Motor.SetMovementCollisionsSolvingActivation(true);
                        Motor.SetGroundSolvingActivation(true);
                        break;
                    }
            }
        }
        #endregion

        #region Inputs
        void SetInputs()
        {
            // Handle state transition from input
            if (input.Value.ChargingDown())
                TransitionToState(CharacterState.Charging);

            if (input.Value.GetNoClipUp())
            {
                if (CurrentCharacterState == CharacterState.Default)
                    TransitionToState(CharacterState.NoClip);
                else if (CurrentCharacterState == CharacterState.NoClip)
                    TransitionToState(CharacterState.Default);
            }

            // Handle ladder transitions
            _ladderUpDownInput = input.Value.GetMovement().y;
            if (input.Value.GetClimbLadder())
            {
                if (Motor.CharacterOverlap(
                    Motor.TransientPosition, Motor.TransientRotation, _probedColliders,
                    InteractionLayer, QueryTriggerInteraction.Collide) > 0)
                {
                    if (_probedColliders[0] != null)
                    {
                        // Handle ladders
                        _probedColliders[0].TryGetComponent<Ladder>(out var ladder);
                        if (ladder)
                        {
                            // Transition to ladder climbing state
                            if (CurrentCharacterState == CharacterState.Default)
                            {
                                ActiveLadder = ladder;
                                TransitionToState(CharacterState.Climbing);
                            }
                            // Transition back to default movement state
                            else if (CurrentCharacterState == CharacterState.Climbing)
                            {
                                ClimbingState = ClimbingState.DeAnchoring;
                                _ladderTargetPosition = Motor.TransientPosition;
                                _ladderTargetRotation = _rotationBeforeClimbing;
                            }
                        }
                    }
                }
            }

            // inputs
            Vector3 moveInputVector;
            moveInputVector.x = input.Value.GetMovement().x;
            moveInputVector.y = 0f;
            moveInputVector.z = input.Value.GetMovement().y;
            _jumpInputIsHeld = input.Value.GetJumpHeld();
            _crouchInputIsHeld = input.Value.GetCrouchHeld();

            // Calculate camera direction and rotation on the character plane
            var cameraPlanarDirection = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
                cameraPlanarDirection = Vector3.ProjectOnPlane(cam.transform.rotation * Vector3.up, Motor.CharacterUp).normalized;
            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        _moveInputVector = cameraPlanarRotation * moveInputVector;
                        _lookInputVector = cameraPlanarDirection;

                        if (input.Value.GetJumpDown())
                        {
                            _timeSinceJumpRequested = 0f;
                            _jumpRequested = true;
                        }

                        if (input.Value.GetCrouchDown())
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                                Vector3 scale;
                                scale.x = 1f;
                                scale.y = 0.5f;
                                scale.z = 1f;
                                MeshRoot.localScale = scale;
                            }
                        }
                        else if (input.Value.GetCrouchUp())
                            _shouldBeCrouching = false;

                        break;
                    }
                case CharacterState.NoClip:
                    {
                        _moveInputVector = cam.transform.rotation * moveInputVector;
                        _lookInputVector = cameraPlanarDirection;
                        break;
                    }
                case CharacterState.Swimming:
                    {
                        _jumpRequested = input.Value.GetJumpHeld();

                        _moveInputVector = cam.transform.rotation * moveInputVector;
                        _lookInputVector = cameraPlanarDirection;
                        break;
                    }
            }
        }
        #endregion

        public void BeforeCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Charging:
                    {
                        // Update times
                        _timeSinceStartedCharge += deltaTime;
                        if (_isStopped)
                            _timeSinceStopped += deltaTime;
                        break;
                    }
            }

            // Handle detecting water surfaces
            // Do a character overlap test to detect water surfaces
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, WaterLayer, QueryTriggerInteraction.Collide) > 0)
            {
                // If a water surface was detected
                if (_probedColliders[0] != null)
                {
                    // If the swimming reference point is inside the box, make sure we are in swimming state
                    if (Physics.ClosestPoint(
                        SwimmingReferencePoint.position, _probedColliders[0], _probedColliders[0].transform.position,
                        _probedColliders[0].transform.rotation) == SwimmingReferencePoint.position)
                    {
                        if (CurrentCharacterState == CharacterState.Default)
                        {
                            TransitionToState(CharacterState.Swimming);
                            _waterZone = _probedColliders[0];
                        }
                    }
                    // otherwise; default state
                    else
                    {
                        if (CurrentCharacterState == CharacterState.Swimming)
                            TransitionToState(CharacterState.Default);
                    }
                }
            }
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = playerAnimation.RootMotionRotationDelta * currentRotation;

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            var t = 1f - Mathf.Exp(-OrientationSharpness * deltaTime);
                            var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, t).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        if (OrientTowardsGravity)
                            // Rotate from current up to invert gravity
                            currentRotation = Quaternion.FromToRotation(currentRotation * Vector3.up, -Gravity) * currentRotation;
                        break;
                    }
                case CharacterState.NoClip:
                    {
                        if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            var t = 1f - Mathf.Exp(-OrientationSharpness * deltaTime);
                            var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, t).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        if (OrientTowardsGravity)
                            // Rotate from current up to invert gravity
                            currentRotation = Quaternion.FromToRotation(currentRotation * Vector3.up, -Gravity) * currentRotation;
                        break;
                    }
                case CharacterState.Swimming:
                    {
                        if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            var t = 1f - Mathf.Exp(-OrientationSharpness * deltaTime);
                            var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, t).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }

                        if (OrientTowardsGravity)
                            // Rotate from current up to invert gravity
                            currentRotation = Quaternion.FromToRotation(currentRotation * Vector3.up, -Gravity) * currentRotation;
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        switch (ClimbingState)
                        {
                            case ClimbingState.Climbing:
                                currentRotation = ActiveLadder.transform.rotation;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                currentRotation = Quaternion.Slerp(_anchoringStartRotation, _ladderTargetRotation, _anchoringTimer / AnchoringDuration);
                                break;
                        }
                        break;
                    }
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity = playerAnimation.RootMotionPositionDelta / deltaTime;

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        Vector3 targetMovementVelocity;
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            // The final velocity is the velocity from root motion reoriented on the ground plane
                            // Reorient velocity on slope
                            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                            // Calculate target velocity
                            var inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                            var reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                            targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                            // Smooth movement Velocity
                            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
                        }
                        else
                        {
                            // Add move input
                            if (_moveInputVector.sqrMagnitude > 0f)
                            {
                                targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                                // Prevent climbing on un-stable slopes with air movement
                                if (Motor.GroundingStatus.FoundAnyGround)
                                {
                                    var lhs = Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal);
                                    var perpenticularObstructionNormal = Vector3.Cross(lhs, Motor.CharacterUp).normalized;
                                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                                }

                                var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                                currentVelocity += AirAccelerationSpeed * deltaTime * velocityDiff;
                            }

                            // Gravity
                            currentVelocity += Gravity * deltaTime;
                            // Drag
                            currentVelocity *= 1f / (1f + (Drag * deltaTime));
                        }

                        // Handle jumping
                        _jumpedThisFrame = false;
                        _timeSinceJumpRequested += deltaTime;
                        if (_jumpRequested)
                        {
                            // Handle double jump
                            if (AllowDoubleJump)
                            {
                                if (_jumpConsumed && !_doubleJumpConsumed &&
                                (AllowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround))
                                {
                                    Motor.ForceUnground(0.1f);

                                    // Add to the return velocity and reset jump state
                                    currentVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                                    _jumpRequested = false;
                                    _doubleJumpConsumed = true;
                                    _jumpedThisFrame = true;
                                }
                            }

                            // See if we actually are allowed to jump
                            if (_canWallJump ||
                                (!_jumpConsumed &&
                                ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) ||
                                _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime)))
                            {
                                // Calculate jump direction before ungrounding
                                var jumpDirection = Motor.CharacterUp;
                                if (_canWallJump)
                                    jumpDirection = _wallJumpNormal;
                                else if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                                    jumpDirection = Motor.GroundingStatus.GroundNormal;

                                // Makes the character skip ground probing/snapping on its next update. 
                                // If this line weren't here, the character would remain snapped to the ground when trying to jump. 
                                // Try commenting this line out and see.
                                Motor.ForceUnground(0.1f);

                                // Add to the return velocity and reset jump state
                                currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                                _jumpRequested = false;
                                _jumpConsumed = true;
                                _jumpedThisFrame = true;
                            }
                        }

                        // Reset wall jump
                        _canWallJump = false;

                        // Take into account additive velocity
                        if (_internalVelocityAdd.sqrMagnitude > 0f)
                        {
                            currentVelocity += _internalVelocityAdd;
                            _internalVelocityAdd = Vector3.zero;
                        }
                        break;
                    }
                case CharacterState.Charging:
                    {
                        // If we have stopped and need to cancel velocity, do it here
                        if (_mustStopVelocity)
                        {
                            currentVelocity = Vector3.zero;
                            _mustStopVelocity = false;
                        }

                        if (_isStopped)
                            // When stopped, do no velocity handling except gravity
                            currentVelocity += Gravity * deltaTime;
                        else
                        {
                            // When charging, velocity is always constant
                            var previousY = currentVelocity.y;
                            currentVelocity = _currentChargeVelocity;
                            currentVelocity.y = previousY;
                            currentVelocity += Gravity * deltaTime;
                        }
                        break;
                    }
                case CharacterState.NoClip:
                    {
                        var verticalInput = 0f + (_jumpInputIsHeld ? 1f : 0f) + (_crouchInputIsHeld ? -1f : 0f);

                        // Smoothly interpolate to target velocity
                        var targetMovementVelocity = (_moveInputVector + (Motor.CharacterUp * verticalInput)).normalized * NoClipMoveSpeed;
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-NoClipSharpness * deltaTime));
                        break;
                    }
                case CharacterState.Swimming:
                    {
                        var verticalInput = 0f + (_jumpInputIsHeld ? 1f : 0f) + (_crouchInputIsHeld ? -1f : 0f);

                        // Smoothly interpolate to target swimming velocity
                        var targetMovementVelocity = (_moveInputVector + (Motor.CharacterUp * verticalInput)).normalized * SwimmingSpeed;
                        var smoothedVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-SwimmingMovementSharpness * deltaTime));

                        // See if our swimming reference point would be out of water after the movement from our velocity has been applied
                        var resultingSwimmingReferancePosition = Motor.TransientPosition + (smoothedVelocity * deltaTime) +
                                                                (SwimmingReferencePoint.position - Motor.TransientPosition);
                        var closestPointWaterSurface = Physics.ClosestPoint(
                            resultingSwimmingReferancePosition, _waterZone, _waterZone.transform.position, _waterZone.transform.rotation);

                        // if our position would be outside the water surface on next update, 
                        // project the velocity on the surface normal so that it would not take us out of the water
                        if (closestPointWaterSurface != resultingSwimmingReferancePosition)
                        {
                            var waterSurfaceNormal = (resultingSwimmingReferancePosition - closestPointWaterSurface).normalized;
                            smoothedVelocity = Vector3.ProjectOnPlane(smoothedVelocity, waterSurfaceNormal);

                            // Jump out of water
                            if (_jumpRequested)
                                smoothedVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                        }

                        currentVelocity = smoothedVelocity;
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        currentVelocity = Vector3.zero;

                        switch (ClimbingState)
                        {
                            case ClimbingState.Climbing:
                                currentVelocity = (_ladderUpDownInput * ActiveLadder.transform.up).normalized * ClimbingSpeed;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                var tmpPosition = Vector3.Lerp(_anchoringStartPosition, _ladderTargetPosition, _anchoringTimer / AnchoringDuration);
                                currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpPosition, deltaTime);
                                break;
                        }
                        break;
                    }
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // Reset root motion deltas
            playerAnimation.RootMotionPositionDelta = Vector3.zero;
            playerAnimation.RootMotionRotationDelta = Quaternion.identity;

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // Handle jumping pre-ground grace period
                        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                            _jumpRequested = false;

                        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                        {
                            // If we're on a ground surface, reset jumping values
                            if (!_jumpedThisFrame)
                            {
                                _doubleJumpConsumed = false;
                                _jumpConsumed = false;
                            }
                            _timeSinceLastAbleToJump = 0f;
                        }
                        else
                            // Keep track of time since we were last able to jump (for grace period)
                            _timeSinceLastAbleToJump += deltaTime;

                        // Handle uncrouching
                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            // Do an overlap test with the character's standing height to see if there are any obstructions
                            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                            if (Motor.CharacterOverlap(
                                Motor.TransientPosition, Motor.TransientRotation, _probedColliders,
                                Motor.CollidableLayers, QueryTriggerInteraction.Ignore) > 0)
                                // If obstructions, just stick to crouching dimensions
                                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                            else
                            {
                                // If no obstructions, uncrouch
                                MeshRoot.localScale = Vector3.one;
                                _isCrouching = false;
                            }
                        }
                        break;
                    }
                case CharacterState.Charging:
                    {
                        // Detect being stopped by elapsed time
                        if (!_isStopped && _timeSinceStartedCharge > MaxChargeTime)
                        {
                            _mustStopVelocity = true;
                            _isStopped = true;
                        }

                        // Detect end of stopping phase and transition back to default movement state
                        if (_timeSinceStopped > StoppedTime)
                            TransitionToState(CharacterState.Default);
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        switch (ClimbingState)
                        {
                            case ClimbingState.Climbing:
                                // Detect getting off ladder during climbing
                                ActiveLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                                if (Mathf.Abs(_onLadderSegmentState) > 0.05f)
                                {
                                    ClimbingState = ClimbingState.DeAnchoring;

                                    // If we're higher than the ladder top point
                                    if (_onLadderSegmentState > 0)
                                    {
                                        _ladderTargetPosition = ActiveLadder.TopReleasePoint.position;
                                        _ladderTargetRotation = ActiveLadder.TopReleasePoint.rotation;
                                    }
                                    // If we're lower than the ladder bottom point
                                    else if (_onLadderSegmentState < 0)
                                    {
                                        _ladderTargetPosition = ActiveLadder.BottomReleasePoint.position;
                                        _ladderTargetRotation = ActiveLadder.BottomReleasePoint.rotation;
                                    }
                                }
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                // Detect transitioning out from anchoring states
                                if (_anchoringTimer >= AnchoringDuration)
                                {
                                    if (ClimbingState == ClimbingState.Anchoring)
                                        ClimbingState = ClimbingState.Climbing;
                                    else if (ClimbingState == ClimbingState.DeAnchoring)
                                        TransitionToState(CharacterState.Default);
                                }

                                // Keep track of time since we started anchoring
                                _anchoringTimer += deltaTime;
                                break;
                        }
                        break;
                    }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Contains(coll))
                return false;
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // We can wall jump only if we are not stable on ground and are moving against an obstruction
                        if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable)
                        {
                            _canWallJump = true;
                            _wallJumpNormal = hitNormal;
                        }
                        break;
                    }
                case CharacterState.Charging:
                    {
                        // Detect being stopped by obstructions
                        if (!_isStopped && !hitStabilityReport.IsStable && Vector3.Dot(-hitNormal, _currentChargeVelocity.normalized) > 0.5f)
                        {
                            _mustStopVelocity = true;
                            _isStopped = true;
                        }
                        break;
                    }
            }
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        _internalVelocityAdd += velocity;
                        break;
                    }
            }
        }

        public void ProcessHitStabilityReport(
            Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        { }

        public void PostGroundingUpdate(float deltaTime) { }

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    }
}