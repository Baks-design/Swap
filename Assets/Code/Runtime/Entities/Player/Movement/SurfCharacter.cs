using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

namespace SwapChains.Runtime.Entities.Player.Movement
{
    [Serializable]
    public class SurfCharacter : ISurfControllable
    {
        public enum ColliderType
        {
            Capsule,
            Box
        }

        [Header("Physics Settings")]
        [SerializeField] Vector3 colliderSize = new(1f, 2f, 1f);
        [SerializeField] float weight = 75f;
        [SerializeField] float rigidbodyPushForce = 2f;
        [SerializeField] bool solidCollider = false;

        [Header("View Settings")]
        [SerializeField] Transform viewTransform;
        [SerializeField] Transform playerRotationTransform;

        [Header("Crouching setup")]
        [SerializeField] float crouchingHeightMultiplier = 0.5f;
        [SerializeField] float crouchingSpeed = 10f;
        float defaultHeight;
        bool allowCrouch = true; // This is separate because you shouldn't be able to toggle crouching on and off during gameplay for various reasons

        [Header("Features")]
        [SerializeField] bool crouchingEnabled = true;
        [SerializeField] bool slidingEnabled = false;
        [SerializeField] bool laddersEnabled = true;
        [SerializeField] bool supportAngledLadders = true;

        [Header("Step offset (can be buggy, enable at your own risk)")]
        [SerializeField] bool useStepOffset = false;
        [SerializeField] float stepOffset = 0.35f;

        [Space]
        [SerializeField] MovementConfig movementConfig;

        int numberOfTriggers = 0;
        bool underwater = false;
        Vector3 prevPosition;
        GameObject _groundObject;
        Vector3 _baseVelocity;
        Collider _collider;
        Vector3 _angles;
        InputAction moveAction;
        GameObject _colliderObject;
        GameObject _cameraWaterCheckObject;
        CameraWaterCheck _cameraWaterCheck;
        Rigidbody rb;
        SurfController _controller;
        readonly MoveData _moveData = new();
        readonly List<Collider> triggers = new();

        public List<Collider> Triggers => triggers;
        public ColliderType CollisionType => ColliderType.Box;
        public MoveType MoveType => MoveType.Walk;
        public MovementConfig MoveConfig => movementConfig;
        public MoveData MoveData => _moveData;
        public Collider Collider => _collider;
        public Vector3 ColliderSize => colliderSize;
        public GameObject GroundObject
        {
            get => _groundObject;
            set => _groundObject = value;
        }
        public Vector3 BaseVelocity => _baseVelocity;
        public Vector3 Forward => viewTransform.forward;
        public Vector3 Right => viewTransform.right;
        public Vector3 Up => viewTransform.up;

        public void Awake()
        {
            _controller = new() { playerTransform = playerRotationTransform };

            if (viewTransform != null)
            {
                _controller.camera = viewTransform;
                _controller.cameraYPos = viewTransform.localPosition.y;
            }

            moveAction = InputSystem.actions.FindAction("Move");
        }

        public void Start(PlayerController controller)
        {
            _colliderObject = new GameObject("PlayerCollider") { layer = controller.gameObject.layer };
            _colliderObject.transform.SetParent(controller.Transform);
            _colliderObject.transform.localPosition = Vector3.zero;
            _colliderObject.transform.rotation = Quaternion.identity;
            _colliderObject.transform.SetSiblingIndex(0);

            // Water check
            _cameraWaterCheckObject = new GameObject("Camera water check") { layer = controller.gameObject.layer };
            _cameraWaterCheckObject.transform.localPosition = viewTransform.localPosition;

            var _cameraWaterCheckSphere = _cameraWaterCheckObject.AddComponent<SphereCollider>();
            _cameraWaterCheckSphere.radius = 0.1f;
            _cameraWaterCheckSphere.isTrigger = true;

            var _cameraWaterCheckRb = _cameraWaterCheckObject.AddComponent<Rigidbody>();
            _cameraWaterCheckRb.useGravity = false;
            _cameraWaterCheckRb.isKinematic = true;

            _cameraWaterCheck = _cameraWaterCheckObject.AddComponent<CameraWaterCheck>();

            prevPosition = controller.Transform.position;

            if (viewTransform == null)
                viewTransform = Camera.main.transform;

            if (playerRotationTransform == null && controller.Transform.childCount > 0)
                playerRotationTransform = controller.Transform.GetChild(0);

            // if (controller.gameObject.TryGetComponent(out _collider))
            //     controller.Destroy(_collider);

            // rigidbody is required to collide with triggers
            if (!controller.gameObject.TryGetComponent(out rb))
                rb = controller.gameObject.AddComponent<Rigidbody>();

            allowCrouch = crouchingEnabled;

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.angularDamping = 0f;
            rb.linearDamping = 0f;
            rb.mass = weight;

            switch (CollisionType)
            {
                // Box collider
                case ColliderType.Box:

                    _collider = _colliderObject.AddComponent<BoxCollider>();

                    var boxc = (BoxCollider)_collider;
                    boxc.size = colliderSize;

                    defaultHeight = boxc.size.y;

                    break;

                // Capsule collider
                case ColliderType.Capsule:

                    _collider = _colliderObject.AddComponent<CapsuleCollider>();

                    var capc = (CapsuleCollider)_collider;
                    capc.height = colliderSize.y;
                    capc.radius = colliderSize.x / 2f;

                    defaultHeight = capc.height;

                    break;
            }

            _moveData.slopeLimit = movementConfig.slopeLimit;

            _moveData.rigidbodyPushForce = rigidbodyPushForce;

            _moveData.slidingEnabled = slidingEnabled;
            _moveData.laddersEnabled = laddersEnabled;
            _moveData.angledLaddersEnabled = supportAngledLadders;

            _moveData.playerTransform = controller.Transform;
            _moveData.viewTransform = viewTransform;
            _moveData.viewTransformDefaultLocalPos = viewTransform.localPosition;

            _moveData.defaultHeight = defaultHeight;
            _moveData.crouchingHeight = crouchingHeightMultiplier;
            _moveData.crouchingSpeed = crouchingSpeed;

            _collider.isTrigger = !solidCollider;
            _moveData.origin = controller.Transform.position;

            _moveData.useStepOffset = useStepOffset;
            _moveData.stepOffset = stepOffset;
        }

        public void Update(PlayerController controller)
        {
            _colliderObject.transform.rotation = Quaternion.identity;

            UpdateMoveData();

            // Previous movement code
            var positionalMovement = controller.Transform.position - prevPosition;
            controller.Transform.position = prevPosition;
            MoveData.origin += positionalMovement;

            // Triggers
            if (numberOfTriggers != triggers.Count)
            {
                numberOfTriggers = triggers.Count;

                underwater = false;
                triggers.RemoveAll(item => item == null);
                for (var i = 0; i < triggers.Count; i++)
                {
                    if (triggers[i] == null) 
                        continue;
                    if (triggers[i].GetComponentInParent<Water>())
                        underwater = true;
                }
            }

            _moveData.cameraUnderwater = _cameraWaterCheck.IsUnderwater();
            _cameraWaterCheckObject.transform.position = viewTransform.position;
            MoveData.underwater = underwater;

            if (allowCrouch)
                _controller.Crouch(this, movementConfig, controller.DeltaTime);

            _controller.ProcessMovement(this, movementConfig, controller.DeltaTime);

            controller.Transform.position = MoveData.origin;
            prevPosition = controller.Transform.position;

            _colliderObject.transform.rotation = Quaternion.identity;
        }

        void UpdateMoveData()
        {
            _moveData.verticalAxis = moveAction.ReadValue<Vector2>().y;
            _moveData.horizontalAxis = moveAction.ReadValue<Vector2>().x;

            _moveData.sprinting = Keyboard.current.shiftKey.isPressed;

            if (Keyboard.current.cKey.wasPressedThisFrame)
                _moveData.crouching = true;
            if (!Keyboard.current.cKey.wasPressedThisFrame)
                _moveData.crouching = false;

            var moveLeft = _moveData.horizontalAxis < 0f;
            var moveRight = _moveData.horizontalAxis > 0f;
            var moveFwd = _moveData.verticalAxis > 0f;
            var moveBack = _moveData.verticalAxis < 0f;

            if (!moveLeft && !moveRight)
                _moveData.sideMove = 0f;
            else if (moveLeft)
                _moveData.sideMove = -MoveConfig.acceleration;
            else if (moveRight)
                _moveData.sideMove = MoveConfig.acceleration;
            if (!moveFwd && !moveBack)
                _moveData.forwardMove = 0f;
            else if (moveFwd)
                _moveData.forwardMove = MoveConfig.acceleration;
            else if (moveBack)
                _moveData.forwardMove = -MoveConfig.acceleration;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                _moveData.wishJump = true;
            if (!Keyboard.current.spaceKey.isPressed)
                _moveData.wishJump = false;

            _moveData.viewAngles = _angles;
        }
    }
}