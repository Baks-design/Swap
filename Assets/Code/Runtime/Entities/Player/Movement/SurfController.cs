using SwapChains.Runtime.Entities.Player.Movement.TraceUtil;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Entities.Player.Movement
{
    public class SurfController
    {
        public Transform playerTransform;
        public Transform camera;
        public float cameraYPos = 0f;
        bool jumping = false;
        bool crouching = false;
        bool wasSliding = false;
        bool uncrouchDown = false;
        float slideSpeedCurrent = 0f;
        float slideDelay = 0f;
        float crouchLerp = 0f;
        float _deltaTime;
        readonly float frictionMult = 1f;
        Vector3 slideDirection = Vector3.forward;
        Vector3 groundNormal = Vector3.up;
        ISurfControllable _surfer;
        MovementConfig _config;
        static readonly RaycastHit[] resultsLadder = new RaycastHit[64];

        public void ProcessMovement(ISurfControllable surfer, MovementConfig config, float deltaTime)
        {
            // cache instead of passing around parameters
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;

            Vector3 dir;
            dir.x = 1f;
            dir.y = 0.95f;
            dir.z = 1f;
            if (_surfer.MoveData.laddersEnabled && !_surfer.MoveData.climbingLadder)
                LadderCheck(dir, _surfer.MoveData.velocity * Mathf.Clamp(_deltaTime * 2f, 0.025f, 0.25f));
                
            if (_surfer.MoveData.laddersEnabled && _surfer.MoveData.climbingLadder)
                LadderPhysics();
            else if (!_surfer.MoveData.underwater)
            {
                if (_surfer.MoveData.velocity.y <= 0f)
                    jumping = false;

                // apply gravity
                if (_surfer.GroundObject == null)
                {
                    _surfer.MoveData.velocity.y -= _surfer.MoveData.gravityFactor * _config.gravity * _deltaTime;
                    _surfer.MoveData.velocity.y += _surfer.BaseVelocity.y * _deltaTime;
                }

                // input velocity, check for ground
                CheckGrounded();
                CalculateMovementVelocity();
            }
            else
                // Do underwater logic
                UnderwaterPhysics();

            var yVel = _surfer.MoveData.velocity.y;
            _surfer.MoveData.velocity.y = 0f;
            _surfer.MoveData.velocity = Vector3.ClampMagnitude(_surfer.MoveData.velocity, _config.maxVelocity);
            _surfer.MoveData.velocity.y = yVel;

            if (_surfer.MoveData.velocity.sqrMagnitude is 0f)
                // Do collisions while standing still
                SurfPhysics.ResolveCollisions(
                    _surfer.Collider, ref _surfer.MoveData.origin, ref _surfer.MoveData.velocity, _deltaTime,
                    _surfer.MoveData.rigidbodyPushForce, 1f, _surfer.MoveData.stepOffset, _surfer);
            else
            {
                var maxDistPerFrame = 0.2f;
                var velocityThisFrame = _surfer.MoveData.velocity * _deltaTime;
                var velocityDistLeft = velocityThisFrame.magnitude;
                var initialVel = velocityDistLeft;

                while (velocityDistLeft > 0f)
                {
                    var amountThisLoop = Mathf.Min(maxDistPerFrame, velocityDistLeft);
                    velocityDistLeft -= amountThisLoop;

                    // increment origin
                    var velThisLoop = velocityThisFrame * (amountThisLoop / initialVel);
                    _surfer.MoveData.origin += velThisLoop;

                    // don't penetrate walls
                    SurfPhysics.ResolveCollisions(
                        _surfer.Collider, ref _surfer.MoveData.origin, ref _surfer.MoveData.velocity, _deltaTime,
                        _surfer.MoveData.rigidbodyPushForce, amountThisLoop / initialVel, _surfer.MoveData.stepOffset, _surfer);
                }
            }

            _surfer.MoveData.groundedTemp = _surfer.MoveData.grounded;
            _surfer = null;
        }

        void CalculateMovementVelocity()
        {
            switch (_surfer.MoveType)
            {
                case MoveType.Walk:

                    if (_surfer.GroundObject == null) // AIR MOVEMENT
                    {
                        wasSliding = false;

                        // apply movement from input
                        _surfer.MoveData.velocity += AirInputMovement();

                        // let the magic happen
                        SurfPhysics.Reflect(ref _surfer.MoveData.velocity, _surfer.Collider, _surfer.MoveData.origin, _deltaTime);
                    }
                    else //  GROUND MOVEMENT
                    {
                        // Sliding
                        if (!wasSliding)
                        {
                            Vector3 velocitySliding;
                            velocitySliding.x = _surfer.MoveData.velocity.x;
                            velocitySliding.y = 0f;
                            velocitySliding.z = _surfer.MoveData.velocity.z;
                            slideDirection = velocitySliding.normalized;
                            slideSpeedCurrent = Mathf.Max(_config.maximumSlideSpeed, velocitySliding.magnitude);
                        }

                        if (_surfer.MoveData.velocity.magnitude > _config.minimumSlideSpeed &&
                            _surfer.MoveData.slidingEnabled && _surfer.MoveData.crouching && slideDelay <= 0f)
                        {
                            if (!wasSliding)
                                slideSpeedCurrent = Mathf.Clamp(
                                    slideSpeedCurrent * _config.slideSpeedMultiplier, _config.minimumSlideSpeed, _config.maximumSlideSpeed);

                            wasSliding = true;

                            SlideMovement();

                            return;
                        }
                        else
                        {
                            if (slideDelay > 0f)
                                slideDelay -= _deltaTime;

                            if (wasSliding)
                                slideDelay = _config.slideDelay;

                            wasSliding = false;
                        }

                        _ = crouching ? _config.crouchFriction : _config.friction;
                        var accel = crouching ? _config.crouchAcceleration : _config.acceleration;
                        _ = crouching ? _config.crouchDeceleration : _config.deceleration;

                        // Get movement directions
                        var forward = Vector3.Cross(groundNormal, -playerTransform.right);
                        var right = Vector3.Cross(groundNormal, forward);

                        var speed = _surfer.MoveData.sprinting ? _config.sprintSpeed : _config.walkSpeed;
                        if (crouching)
                            speed = _config.crouchSpeed;

                        // Jump and friction
                        if (_surfer.MoveData.wishJump)
                        {
                            ApplyFriction(0f, true, true);
                            Jump();
                            return;
                        }
                        else
                            ApplyFriction(1f * frictionMult, true, true);

                        var forwardMove = _surfer.MoveData.verticalAxis;
                        var rightMove = _surfer.MoveData.horizontalAxis;

                        Vector3 _wishDir;
                        _wishDir = forwardMove * forward + rightMove * right;
                        _wishDir.Normalize();

                        Vector3 velocity;
                        velocity.x = _surfer.MoveData.velocity.x;
                        velocity.y = 0f;
                        velocity.z = _surfer.MoveData.velocity.z;

                        var forwardVelocity = Vector3.Cross(groundNormal, Quaternion.AngleAxis(-90f, Vector3.up) * velocity);

                        // Set the target speed of the player
                        var _wishSpeed = _wishDir.magnitude;
                        _wishSpeed *= speed;

                        // Accelerate
                        var yVel = _surfer.MoveData.velocity.y;
                        Accelerate(_wishDir, _wishSpeed, accel * Mathf.Min(frictionMult, 1f), false);

                        var maxVelocityMagnitude = _config.maxVelocity;
                        _surfer.MoveData.velocity = Vector3.ClampMagnitude(velocity, maxVelocityMagnitude);
                        _surfer.MoveData.velocity.y = yVel;

                        // Calculate how much slopes should affect movement
                        var yVelocityNew = forwardVelocity.normalized.y * velocity.magnitude;

                        // Apply the Y-movement from slopes
                        _surfer.MoveData.velocity.y = yVelocityNew * (_wishDir.y < 0f ? 1.2f : 1f);
                        _ = _surfer.MoveData.velocity.y - yVelocityNew;
                    }

                    break;
            }
        }

        void UnderwaterPhysics()
        {
            _surfer.MoveData.velocity = Vector3.Lerp(
                _surfer.MoveData.velocity, Vector3.zero, _config.underwaterVelocityDampening * _deltaTime);

            // Gravity
            if (!CheckGrounded())
                _surfer.MoveData.velocity.y -= _config.underwaterGravity * _deltaTime;

            // Swimming upwards
            if (Keyboard.current.spaceKey.isPressed)
                _surfer.MoveData.velocity.y += _config.swimUpSpeed * _deltaTime;
            _ = _config.underwaterFriction;
            var accel = _config.underwaterAcceleration;
            _ = _config.underwaterDeceleration;

            ApplyFriction(1f, true, false);

            // Get movement directions
            var forward = Vector3.Cross(groundNormal, -playerTransform.right);
            var right = Vector3.Cross(groundNormal, forward);

            var speed = _config.underwaterSwimSpeed;

            var forwardMove = _surfer.MoveData.verticalAxis;
            var rightMove = _surfer.MoveData.horizontalAxis;

            Vector3 _wishDir;
            _wishDir = forwardMove * forward + rightMove * right;
            _wishDir.Normalize();

            Vector3 surferVelocity;
            surferVelocity.x = _surfer.MoveData.velocity.x;
            surferVelocity.y = 0f;
            surferVelocity.z = _surfer.MoveData.velocity.z;

            var forwardVelocity = Vector3.Cross(groundNormal, Quaternion.AngleAxis(-90f, Vector3.up) * surferVelocity);

            // Set the target speed of the player
            var _wishSpeed = _wishDir.magnitude;
            _wishSpeed *= speed;

            // Accelerate
            var yVel = _surfer.MoveData.velocity.y;
            Accelerate(_wishDir, _wishSpeed, accel, false);

            var maxVelocityMagnitude = _config.maxVelocity;
            _surfer.MoveData.velocity = Vector3.ClampMagnitude(surferVelocity, maxVelocityMagnitude);
            _surfer.MoveData.velocity.y = yVel;

            var yVelStored = _surfer.MoveData.velocity.y;
            _surfer.MoveData.velocity.y = 0f;

            // Calculate how much slopes should affect movement
            var yVelocityNew = forwardVelocity.normalized.y * surferVelocity.magnitude;

            // Apply the Y-movement from slopes
            _surfer.MoveData.velocity.y = Mathf.Min(Mathf.Max(0f, yVelocityNew) + yVelStored, speed);

            // Jumping out of water
            var movingForwards = playerTransform.InverseTransformVector(_surfer.MoveData.velocity).z > 0f;
            var waterJumpTrace = TraceBounds(
                playerTransform.position, playerTransform.position + playerTransform.forward * 0.1f, SurfPhysics.groundLayerMask);

            if (waterJumpTrace.hitCollider != null && Vector3.Angle(Vector3.up, waterJumpTrace.planeNormal) >=
                _config.slopeLimit && Keyboard.current.spaceKey.isPressed && !_surfer.MoveData.cameraUnderwater && movingForwards)
                _surfer.MoveData.velocity.y = Mathf.Max(_surfer.MoveData.velocity.y, _config.jumpForce);
        }

        void LadderCheck(Vector3 colliderScale, Vector3 direction)
        {
            if (_surfer.MoveData.velocity.sqrMagnitude <= 0f) return;

            var foundLadder = false;

            var hits = Physics.BoxCastNonAlloc(
                _surfer.MoveData.origin, Vector3.Scale(_surfer.Collider.bounds.size * 0.5f, colliderScale),
                Vector3.Scale(direction, Vector3.right + Vector3.forward), resultsLadder, Quaternion.identity,
                direction.magnitude, SurfPhysics.groundLayerMask, QueryTriggerInteraction.Collide);

            for (var i = 0; i < hits; i++)
            {
                var ladder = resultsLadder[i].transform.GetComponentInParent<Ladder>();
                if (ladder != null)
                {
                    var allowClimb = true;
                    var ladderAngle = Vector3.Angle(Vector3.up, resultsLadder[i].normal);

                    if (_surfer.MoveData.angledLaddersEnabled)
                    {
                        if (resultsLadder[i].normal.y < 0f)
                            allowClimb = false;
                        else
                        {
                            if (ladderAngle <= _surfer.MoveData.slopeLimit)
                                allowClimb = false;
                        }
                    }
                    else if (resultsLadder[i].normal.y != 0f)
                        allowClimb = false;

                    if (allowClimb)
                    {
                        foundLadder = true;
                        if (_surfer.MoveData.climbingLadder == false)
                        {
                            _surfer.MoveData.climbingLadder = true;
                            _surfer.MoveData.ladderNormal = resultsLadder[i].normal;
                            _surfer.MoveData.ladderDirection = 2f * direction.magnitude * -resultsLadder[i].normal;

                            if (_surfer.MoveData.angledLaddersEnabled)
                            {
                                var sideDir = resultsLadder[i].normal;
                                sideDir.y = 0f;
                                sideDir = Quaternion.AngleAxis(-90f, Vector3.up) * sideDir;

                                _surfer.MoveData.ladderClimbDir = Quaternion.AngleAxis(90f, sideDir) * resultsLadder[i].normal;
                                _surfer.MoveData.ladderClimbDir *= 1f / _surfer.MoveData.ladderClimbDir.y; // Make sure Y is always 1
                            }
                            else
                                _surfer.MoveData.ladderClimbDir = Vector3.up;
                        }
                    }
                }
            }

            if (!foundLadder)
            {
                _surfer.MoveData.ladderNormal = Vector3.zero;
                _surfer.MoveData.ladderVelocity = Vector3.zero;
                _surfer.MoveData.climbingLadder = false;
                _surfer.MoveData.ladderClimbDir = Vector3.up;
            }
        }

        void LadderPhysics()
        {
            _surfer.MoveData.ladderVelocity = _surfer.MoveData.verticalAxis * 6f * _surfer.MoveData.ladderClimbDir;
            _surfer.MoveData.velocity = Vector3.Lerp(_surfer.MoveData.velocity, _surfer.MoveData.ladderVelocity, _deltaTime * 10f);

            LadderCheck(Vector3.one, _surfer.MoveData.ladderDirection);

            var floorTrace = TraceToFloor();
            if (_surfer.MoveData.verticalAxis < 0f && floorTrace.hitCollider != null &&
                Vector3.Angle(Vector3.up, floorTrace.planeNormal) <= _surfer.MoveData.slopeLimit)
            {
                _surfer.MoveData.velocity = _surfer.MoveData.ladderNormal * 0.5f;
                _surfer.MoveData.ladderVelocity = Vector3.zero;
                _surfer.MoveData.climbingLadder = false;
            }

            if (_surfer.MoveData.wishJump)
            {
                _surfer.MoveData.velocity = _surfer.MoveData.ladderNormal * 4f;
                _surfer.MoveData.ladderVelocity = Vector3.zero;
                _surfer.MoveData.climbingLadder = false;
            }
        }

        void Accelerate(Vector3 wishDir, float wishSpeed, float acceleration, bool yMovement)
        {
            // again, no idea
            var _currentSpeed = Vector3.Dot(_surfer.MoveData.velocity, wishDir);
            var _addSpeed = wishSpeed - _currentSpeed;

            // If you're not actually increasing your speed, stop here.
            if (_addSpeed <= 0f) return;

            // won't bother trying to understand any of this, really
            var _accelerationSpeed = Mathf.Min(acceleration * _deltaTime * wishSpeed, _addSpeed);

            // Add the velocity.
            _surfer.MoveData.velocity.x += _accelerationSpeed * wishDir.x;
            if (yMovement)
                _surfer.MoveData.velocity.y += _accelerationSpeed * wishDir.y;
            _surfer.MoveData.velocity.z += _accelerationSpeed * wishDir.z;
        }

        void ApplyFriction(float t, bool yAffected, bool grounded)
        {
            // Set Y to 0, speed to the magnitude of movement and drop to 0. 
            // I think drop is the amount of speed that is lost, 
            // but I just stole this from the internet, idk.
            var _vel = _surfer.MoveData.velocity;
            _vel.y = 0f;
            var _speed = _vel.magnitude;
            var _drop = 0f;

            var fric = crouching ? _config.crouchFriction : _config.friction;
            _ = crouching ? _config.crouchAcceleration : _config.acceleration;
            var decel = crouching ? _config.crouchDeceleration : _config.deceleration;

            // Only apply friction if the player is grounded
            if (grounded)
            {
                // i honestly have no idea what this does tbh
                _vel.y = _surfer.MoveData.velocity.y;
                var _control = _speed < decel ? decel : _speed;
                _drop = _control * fric * _deltaTime * t;
            }

            // again, no idea, but comments look cool
            var _newSpeed = Mathf.Max(_speed - _drop, 0f);
            if (_speed > 0f)
                _newSpeed /= _speed;

            // Set the end-velocity
            _surfer.MoveData.velocity.x *= _newSpeed;
            if (yAffected is true)
                _surfer.MoveData.velocity.y *= _newSpeed;
            _surfer.MoveData.velocity.z *= _newSpeed;
        }

        Vector3 AirInputMovement()
        {
            GetWishValues(out var _, out var wishDir, out var wishSpeed);

            if (_config.clampAirSpeed && wishSpeed != 0f && (wishSpeed > _config.maxSpeed))
            {
                _ = _config.maxSpeed / wishSpeed;
                wishSpeed = _config.maxSpeed;
            }

            return SurfPhysics.AirAccelerate(_surfer.MoveData.velocity, wishDir, wishSpeed, _config.airAcceleration, _config.airCap, _deltaTime);
        }

        /// <param name="wishVel"></param>
        /// <param name="wishDir"></param>
        /// <param name="wishSpeed"></param>
        void GetWishValues(out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed)
        {
            wishVel = Vector3.zero;
            _ = Vector3.zero;

            var forward = _surfer.Forward;
            var right = _surfer.Right;
            forward[1] = 0f;
            right[1] = 0f;
            forward.Normalize();
            right.Normalize();

            for (var i = 0; i < 3; i++)
                wishVel[i] = forward[i] * _surfer.MoveData.forwardMove + right[i] * _surfer.MoveData.sideMove;
            wishVel[1] = 0;
            wishSpeed = wishVel.magnitude;
            wishDir = wishVel.normalized;
        }

        /// <param name="velocity"></param>
        /// <param name="jumpPower"></param>
        void Jump()
        {
            if (!_config.autoBhop)
                _surfer.MoveData.wishJump = false;

            _surfer.MoveData.velocity.y += _config.jumpForce;

            jumping = true;
        }

        bool CheckGrounded()
        {
            _surfer.MoveData.surfaceFriction = 1f;
            var movingUp = _surfer.MoveData.velocity.y > 0f;
            var trace = TraceToFloor();

            var groundSteepness = Vector3.Angle(Vector3.up, trace.planeNormal);

            if (trace.hitCollider == null || groundSteepness > _config.slopeLimit || (jumping && _surfer.MoveData.velocity.y > 0f))
            {
                SetGround(null);

                if (movingUp && _surfer.MoveType != MoveType.Noclip)
                    _surfer.MoveData.surfaceFriction = _config.airFriction;

                return false;
            }
            else
            {
                groundNormal = trace.planeNormal;
                SetGround(trace.hitCollider.gameObject);
                return true;
            }
        }

        /// <param name="obj"></param>
        void SetGround(GameObject obj)
        {
            if (obj != null)
            {
                _surfer.GroundObject = obj;
                _surfer.MoveData.velocity.y = 0f;
            }
            else
                _surfer.GroundObject = null;
        }

        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        Trace TraceBounds(Vector3 start, Vector3 end, int layerMask) => Tracer.TraceCollider(_surfer.Collider, start, end, layerMask);

        Trace TraceToFloor()
        {
            var down = _surfer.MoveData.origin;
            down.y -= 0.15f;
            return Tracer.TraceCollider(_surfer.Collider, _surfer.MoveData.origin, down, SurfPhysics.groundLayerMask);
        }

        public void Crouch(ISurfControllable surfer, MovementConfig config, float deltaTime)
        {
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;

            if (_surfer == null) return;
            if (_surfer.Collider == null) return;

            var grounded = _surfer.GroundObject != null;
            var wantsToCrouch = _surfer.MoveData.crouching;

            var crouchingHeight = Mathf.Clamp(_surfer.MoveData.crouchingHeight, 0.01f, 1f);
            var heightDifference = _surfer.MoveData.defaultHeight - _surfer.MoveData.defaultHeight * crouchingHeight;

            if (grounded)
                uncrouchDown = false;

            // Crouching input
            if (grounded)
                crouchLerp = Mathf.Lerp(crouchLerp, wantsToCrouch ? 1f : 0f, _deltaTime * _surfer.MoveData.crouchingSpeed);
            else if (!grounded && !wantsToCrouch && crouchLerp < 0.95f)
                crouchLerp = 0f;
            else if (!grounded && wantsToCrouch)
                crouchLerp = 1f;

            // Collider and position changing
            if (crouchLerp > 0.9f && !crouching)
            {
                // Begin crouching
                crouching = true;

                if (_surfer.Collider.GetType() == typeof(BoxCollider))
                {
                    var boxCollider = (BoxCollider)_surfer.Collider;
                    Vector3 boxSize;
                    boxSize.x = boxCollider.size.x;
                    boxSize.y = _surfer.MoveData.defaultHeight * crouchingHeight;
                    boxSize.z = boxCollider.size.z;
                    boxCollider.size = boxSize;
                }
                else if (_surfer.Collider.GetType() == typeof(CapsuleCollider))
                {
                    var capsuleCollider = (CapsuleCollider)_surfer.Collider;
                    capsuleCollider.height = _surfer.MoveData.defaultHeight * crouchingHeight;
                }

                // Move position and stuff
                _surfer.MoveData.origin += heightDifference / 2f * (grounded ? Vector3.down : Vector3.up);
                foreach (Transform child in playerTransform)
                {
                    if (child == _surfer.MoveData.viewTransform) continue;
                    child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y * crouchingHeight, child.localPosition.z);
                }

                uncrouchDown = !grounded;
            }
            else if (crouching)
            {
                // Check if the player can uncrouch
                var canUncrouch = true;

                if (_surfer.Collider.GetType() == typeof(BoxCollider))
                {
                    var boxCollider = (BoxCollider)_surfer.Collider;
                    var halfExtents = boxCollider.size * 0.5f;
                    var startPos = boxCollider.transform.position;
                    var endPos = boxCollider.transform.position + (uncrouchDown ? Vector3.down : Vector3.up) * heightDifference;

                    var trace = Tracer.TraceBox(startPos, endPos, halfExtents, boxCollider.contactOffset, SurfPhysics.groundLayerMask);
                    if (trace.hitCollider != null)
                        canUncrouch = false;
                }
                else if (_surfer.Collider.GetType() == typeof(CapsuleCollider))
                {
                    var capsuleCollider = (CapsuleCollider)_surfer.Collider;
                    var point1 = capsuleCollider.center + 0.5f * capsuleCollider.height * Vector3.up;
                    var point2 = capsuleCollider.center + 0.5f * capsuleCollider.height * Vector3.down;
                    var startPos = capsuleCollider.transform.position;
                    var endPos = capsuleCollider.transform.position + (uncrouchDown ? Vector3.down : Vector3.up) * heightDifference;

                    var trace = Tracer.TraceCapsule(
                        point1, point2, capsuleCollider.radius, startPos, endPos, capsuleCollider.contactOffset, SurfPhysics.groundLayerMask);
                    if (trace.hitCollider != null)
                        canUncrouch = false;
                }

                // Uncrouch
                if (canUncrouch && crouchLerp <= 0.9f)
                {
                    crouching = false;

                    if (_surfer.Collider.GetType() == typeof(BoxCollider))
                    {
                        var boxCollider = (BoxCollider)_surfer.Collider;

                        Vector3 boxSize;
                        boxSize.x = boxCollider.size.x;
                        boxSize.y = _surfer.MoveData.defaultHeight;
                        boxSize.z = boxCollider.size.z;
                        boxCollider.size = boxSize;
                    }
                    else if (_surfer.Collider.GetType() == typeof(CapsuleCollider))
                    {
                        var capsuleCollider = (CapsuleCollider)_surfer.Collider;
                        capsuleCollider.height = _surfer.MoveData.defaultHeight;
                    }

                    // Move position and stuff
                    _surfer.MoveData.origin += heightDifference / 2f * (uncrouchDown ? Vector3.down : Vector3.up);

                    foreach (Transform child in playerTransform)
                    {
                        Vector3 childPos;
                        childPos.x = child.localPosition.x;
                        childPos.y = child.localPosition.y / crouchingHeight;
                        childPos.z = child.localPosition.z;
                        child.localPosition = childPos;
                    }
                }

                if (!canUncrouch)
                    crouchLerp = 1f;
            }

            // Changing camera position
            if (!crouching)
                _surfer.MoveData.viewTransform.localPosition = Vector3.Lerp(
                    _surfer.MoveData.viewTransformDefaultLocalPos,
                    _surfer.MoveData.viewTransformDefaultLocalPos * crouchingHeight + 0.5f * heightDifference * Vector3.down, crouchLerp);
            else
                _surfer.MoveData.viewTransform.localPosition = Vector3.Lerp(
                    _surfer.MoveData.viewTransformDefaultLocalPos - 0.5f * heightDifference * Vector3.down,
                    _surfer.MoveData.viewTransformDefaultLocalPos * crouchingHeight, crouchLerp);
        }

        void SlideMovement()
        {
            // Gradually change direction
            Vector3 groundNormalDir;
            groundNormalDir.x = groundNormal.x;
            groundNormalDir.y = 0f;
            groundNormalDir.z = groundNormal.z;
            slideDirection += _deltaTime * slideSpeedCurrent * groundNormalDir;
            slideDirection = slideDirection.normalized;

            // Set direction
            var slideForward = Vector3.Cross(groundNormal, Quaternion.AngleAxis(-90f, Vector3.up) * slideDirection);

            // Set the velocity
            slideSpeedCurrent -= _config.slideFriction * _deltaTime;
            slideSpeedCurrent = Mathf.Clamp(slideSpeedCurrent, 0f, _config.maximumSlideSpeed);
            slideSpeedCurrent -= (slideForward * slideSpeedCurrent).y * _deltaTime * _config.downhillSlideSpeedMultiplier; // Accelerate downhill

            _surfer.MoveData.velocity = slideForward * slideSpeedCurrent;

            // Jump
            if (_surfer.MoveData.wishJump && slideSpeedCurrent < _config.minimumSlideSpeed * _config.slideSpeedMultiplier)
            {
                Jump();
                return;
            }
        }
    }
}
