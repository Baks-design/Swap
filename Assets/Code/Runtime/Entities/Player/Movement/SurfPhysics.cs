using SwapChains.Runtime.Entities.Player.Movement.TraceUtil;
using SwapChains.Runtime.Utilities.Extensions;
using UnityEngine;

namespace SwapChains.Runtime.Entities.Player.Movement
{
    public class SurfPhysics
    {
        /// <summary>
        /// Change this if your ground is on a different layer
        /// </summary>
        public static int groundLayerMask = LayerMask.GetMask(new string[] { "Default", "Ground", "Player clip" });
        const float SurfSlope = 0.7f;
        const int maxCollisions = 128;
        const int maxClipPlanes = 5;
        const int numBumps = 1;
        static readonly Collider[] colliders = new Collider[maxCollisions];
        static readonly Vector3[] planes = new Vector3[maxClipPlanes];

        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="velocity"></param>
        /// http://www.00jknight.com/blog/unity-character-controller
        public static void ResolveCollisions(
            Collider collider, ref Vector3 origin, ref Vector3 velocity, float rigidbodyPushForce, float deltaTime,
            float velocityMultiplier = 1f, float stepOffset = 0f, ISurfControllable surfer = null)
        {
            // manual collision resolving
            var numOverlaps = 0;
            if (collider is CapsuleCollider)
            {
                var capc = collider as CapsuleCollider;
                GetCapsulePoints(capc, origin, out var point1, out var point2);
                numOverlaps = Physics.OverlapCapsuleNonAlloc(
                    point1, point2, capc.radius, colliders, groundLayerMask, QueryTriggerInteraction.Ignore);
            }
            else if (collider is BoxCollider)
                numOverlaps = Physics.OverlapBoxNonAlloc(
                    origin, collider.bounds.extents, colliders, Quaternion.identity,
                    groundLayerMask, QueryTriggerInteraction.Ignore);

            var forwardVelocity = Vector3.Scale(velocity, Vector3.right + Vector3.forward);
            for (var i = 0; i < numOverlaps; i++)
            {
                if (Physics.ComputePenetration(collider, origin,
                    Quaternion.identity, colliders[i], colliders[i].transform.position,
                    colliders[i].transform.rotation, out var direction, out var distance))
                {
                    // Step offset
                    if (stepOffset > 0f && surfer != null && surfer.MoveData.useStepOffset)
                        if (StepOffset(collider, ref origin, ref velocity, stepOffset, forwardVelocity, surfer, deltaTime))
                            return;

                    // Handle collision
                    direction.Normalize();
                    var penetrationVector = direction * distance;
                    var velocityProjected = Vector3.Project(velocity, -direction);
                    velocityProjected.y = 0f; // don't touch y velocity, we need it to calculate fall damage elsewhere
                    origin += penetrationVector;
                    velocity -= velocityProjected * velocityMultiplier;

                    var rb = colliders[i].GetComponentInParent<Rigidbody>();
                    if (rb != null && !rb.isKinematic)
                        rb.AddForceAtPosition(rigidbodyPushForce * velocityMultiplier * velocityProjected, origin, ForceMode.Impulse);
                }
            }
        }

        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="velocity"></param>
        /// <param name="stepOffset"></param>
        /// <param name="forwardVelocity"></param>
        /// <param name="surfer"></param>
        public static bool StepOffset(
            Collider collider, ref Vector3 origin, ref Vector3 velocity,
            float stepOffset, Vector3 forwardVelocity, ISurfControllable surfer, float deltaTime)
        {
            // Return if step offset is 0
            if (stepOffset <= 0f)
                return false;

            // Get forward direction (return if we aren't moving/are only moving vertically)
            var forwardDirection = forwardVelocity.normalized;
            if (forwardDirection.sqrMagnitude is 0f)
                return false;

            // Trace ground
            var groundTrace = Tracer.TraceCollider(collider, origin, origin + Vector3.down * 0.1f, groundLayerMask);
            if (groundTrace.hitCollider == null || Vector3.Angle(Vector3.up, groundTrace.planeNormal) > surfer.MoveData.slopeLimit)
                return false;

            // Trace wall
            var wallTrace = Tracer.TraceCollider(collider, origin, origin + velocity, groundLayerMask, 0.9f);
            if (wallTrace.hitCollider == null || Vector3.Angle(Vector3.up, wallTrace.planeNormal) <= surfer.MoveData.slopeLimit)
                return false;

            // Trace upwards (check for roof etc)
            var upDistance = stepOffset;
            var upTrace = Tracer.TraceCollider(collider, origin, origin + Vector3.up * stepOffset, groundLayerMask);
            if (upTrace.hitCollider != null)
                upDistance = upTrace.distance;

            // Don't bother doing the rest if we can't move up at all anyway
            if (upDistance <= 0f)
                return false;

            var upOrigin = origin + Vector3.up * upDistance;

            // Trace forwards (check for walls etc)
            var forwardMagnitude = stepOffset;
            var forwardDistance = forwardMagnitude;
            var forwardTrace = Tracer.TraceCollider(
                collider, upOrigin, upOrigin + forwardDirection * Mathf.Max(0.2f, forwardMagnitude), groundLayerMask);
            if (forwardTrace.hitCollider != null)
                forwardDistance = forwardTrace.distance;

            // Don't bother doing the rest if we can't move forward anyway
            if (forwardDistance <= 0f)
                return false;

            var upForwardOrigin = upOrigin + forwardDirection * forwardDistance;

            // Trace down (find ground)
            var downDistance = upDistance;
            var downTrace = Tracer.TraceCollider(collider, upForwardOrigin, upForwardOrigin + Vector3.down * upDistance, groundLayerMask);
            if (downTrace.hitCollider != null)
                downDistance = downTrace.distance;

            // Check step size/angle
            var verticalStep = Mathf.Clamp(upDistance - downDistance, 0f, stepOffset);
            Vector3 stepDir;
            stepDir.x = 0f;
            stepDir.y = verticalStep;
            stepDir.z = forwardDistance;
            var stepAngle = Vector3.Angle(Vector3.forward, stepDir);
            if (stepAngle > surfer.MoveData.slopeLimit)
                return false;

            // Get new position
            var endOrigin = origin + Vector3.up * verticalStep;

            // Actually move
            if (origin != endOrigin && forwardDistance > 0f)
            {
                origin = endOrigin + forwardDistance * deltaTime * forwardDirection;
                return true;
            }
            else
                return false;
        }

        /// <param name="velocity"></param>
        /// <param name="stopSpeed"></param>
        /// <param name="friction"></param>
        /// <param name="deltaTime"></param>
        public static void Friction(ref Vector3 velocity, float stopSpeed, float friction, float deltaTime)
        {
            var speed = velocity.magnitude;
            if (speed < 0.0001905f)
                return;

            var drop = 0f;
            // apply ground friction
            var control = (speed < stopSpeed) ? stopSpeed : speed;
            drop += control * friction * deltaTime;

            // scale the velocity
            var newspeed = speed - drop;
            if (newspeed < 0f)
                newspeed = 0f;
            if (newspeed != speed)
            {
                newspeed /= speed;
                velocity *= newspeed;
            }
        }

        /// <param name="velocity"></param>
        /// <param name="wishdir"></param>
        /// <param name="wishspeed"></param>
        /// <param name="accel"></param>
        /// <param name="airCap"></param>
        /// <param name="deltaTime"></param>
        public static Vector3 AirAccelerate(
            Vector3 velocity, Vector3 wishdir, float wishspeed, float accel, float airCap, float deltaTime)
        {
            // Cap speed
            var wishspd = wishspeed;
            wishspd = Mathf.Min(wishspd, airCap);

            // Determine veer amount
            var currentspeed = Vector3.Dot(velocity, wishdir);

            // See how much to add
            var addspeed = wishspd - currentspeed;
            // If not adding any, done.
            if (addspeed <= 0f)
                return Vector3.zero;

            // Determine acceleration speed after acceleration
            var accelspeed = accel * wishspeed * deltaTime;
            // Cap it
            accelspeed = Mathf.Min(accelspeed, addspeed);

            var result = Vector3.zero;
            // Adjust pmove vel.
            for (var i = 0; i < 3; i++)
                result[i] += accelspeed * wishdir[i];
            return result;
        }

        /// <param name="wishdir"></param>
        /// <param name="wishspeed"></param>
        /// <param name="accel"></param>
        public static Vector3 Accelerate(
            Vector3 currentVelocity, Vector3 wishdir, float wishspeed, float accel, float deltaTime, float surfaceFriction)
        {
            // See if we are changing direction a bit
            var currentspeed = Vector3.Dot(currentVelocity, wishdir);

            // Reduce wishspeed by the amount of veer.
            var addspeed = wishspeed - currentspeed;
            // If not going to add any speed, done.
            if (addspeed <= 0f)
                return Vector3.zero;

            // Determine amount of accleration.
            var accelspeed = accel * deltaTime * wishspeed * surfaceFriction;
            // Cap at addspeed
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            var result = Vector3.zero;
            // Adjust velocity.
            for (var i = 0; i < 3; i++)
                result[i] += accelspeed * wishdir[i];
            return result;
        }

        /// <param name="velocity"></param>
        /// <param name="origin"></param>
        /// <param name="firstDestination"></param>
        /// <param name="firstTrace"></param>
        public static int Reflect(ref Vector3 velocity, Collider collider, Vector3 origin, float deltaTime)
        {
            float d;
            var newVelocity = Vector3.zero;
            var blocked = 0;           // Assume not blocked
            var numplanes = 0;           //  and not sliding along any planes
            var originalVelocity = velocity;  // Store original velocity
            var primalVelocity = velocity;
            var allFraction = 0f;
            var timeLeft = deltaTime;   // Total time for this movement operation.

            for (var bumpcount = 0; bumpcount < numBumps; bumpcount++)
            {
                if (velocity.magnitude == 0f)
                    break;

                // Assume we can move all the way from the current origin to the
                //  end point.
                var end = VectorExtensions.VectorMa(origin, timeLeft, velocity);
                var trace = Tracer.TraceCollider(collider, origin, end, groundLayerMask);

                allFraction += trace.fraction;

                if (trace.fraction > 0)
                {
                    // actually covered some distance
                    originalVelocity = velocity;
                    numplanes = 0;
                }

                // If we covered the entire distance, we are done
                //  and can return.
                if (trace.fraction == 1)
                    break;      // moved the entire distance

                // If the plane we hit has a high z component in the normal, then
                //  it's probably a floor
                if (trace.planeNormal.y > SurfSlope)
                    blocked |= 1;       // floor

                // If the plane has a zero z component in the normal, then it's a 
                //  step or wall
                if (trace.planeNormal.y == 0)
                    blocked |= 2;       // step / wall

                // Reduce amount of m_flFrameTime left by total time left * fraction
                //  that we covered.
                timeLeft -= timeLeft * trace.fraction;

                // Did we run out of planes to clip against?
                if (numplanes >= maxClipPlanes)
                {
                    // this shouldn't really happen
                    //  Stop our movement if so.
                    velocity = Vector3.zero;
                    break;
                }

                // Set up next clipping plane
                planes[numplanes] = trace.planeNormal;
                numplanes++;

                // modify original_velocity so it parallels all of the clip planes
                // reflect player velocity 
                // Only give this a try for first impact plane because you can get yourself stuck in an acute corner by jumping in place
                //  and pressing forward and nobody was really using this bounce/reflection feature anyway...
                if (numplanes == 1)
                {
                    for (var i = 0; i < numplanes; i++)
                    {
                        if (planes[i][1] > SurfSlope)
                            // floor or slope
                            return blocked;
                        else
                            ClipVelocity(originalVelocity, planes[i], ref newVelocity, 1f);
                    }

                    velocity = newVelocity;
                    originalVelocity = newVelocity;
                }
                else
                {
                    int i;
                    for (i = 0; i < numplanes; i++)
                    {
                        ClipVelocity(originalVelocity, planes[i], ref velocity, 1f);

                        int j;
                        for (j = 0; j < numplanes; j++)
                            if (j != i)
                                if (Vector3.Dot(velocity, planes[j]) < 0f) // Are we now moving against this plane?
                                    break;

                        if (j == numplanes) break; // Didn't have to clip, so we're ok   
                    }

                    // Did we go all the way through plane set
                    if (i != numplanes)
                    {
                        // go along this plane
                        // pmove.velocity is set in clipping call, no need to set again.
                    }
                    else
                    {
                        // go along the crease
                        if (numplanes != 2)
                        {
                            velocity = Vector3.zero;
                            break;
                        }

                        var dir = Vector3.Cross(planes[0], planes[1]).normalized;
                        d = Vector3.Dot(dir, velocity);
                        velocity = dir * d;
                    }

                    // if original velocity is against the original velocity, stop dead
                    // to avoid tiny occilations in sloping corners
                    d = Vector3.Dot(velocity, primalVelocity);
                    if (d <= 0f)
                    {
                        velocity = Vector3.zero;
                        break;
                    }
                }
            }

            if (allFraction == 0f)
                velocity = Vector3.zero;

            return blocked;
        }

        /// <param name="input"></param>
        /// <param name="normal"></param>
        /// <param name="output"></param>
        /// <param name="overbounce"></param>
        public static int ClipVelocity(Vector3 input, Vector3 normal, ref Vector3 output, float overbounce)
        {
            var angle = normal[1];
            var blocked = 0x00;     // Assume unblocked.
            if (angle > 0)          // If the plane that is blocking us has a positive z component, then assume it's a floor.
                blocked |= 0x01;    // 
            if (angle == 0)         // If the plane has no Z, it is vertical (wall/step)
                blocked |= 0x02;    // 

            // Determine how far along plane to slide based on incoming direction.
            var backoff = Vector3.Dot(input, normal) * overbounce;

            for (var i = 0; i < 3; i++)
            {
                var change = normal[i] * backoff;
                output[i] = input[i] - change;
            }

            // iterate once to make sure we aren't still moving through the plane
            var adjust = Vector3.Dot(output, normal);
            if (adjust < 0f)
                output -= normal * adjust;

            // Return blocking flags.
            return blocked;
        }

        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public static void GetCapsulePoints(CapsuleCollider capc, Vector3 origin, out Vector3 p1, out Vector3 p2)
        {
            var distanceToPoints = capc.height / 2f - capc.radius;
            p1 = origin + capc.center + Vector3.up * distanceToPoints;
            p2 = origin + capc.center - Vector3.up * distanceToPoints;
        }
    }
}
