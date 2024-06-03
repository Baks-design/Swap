using UnityEngine;

namespace SwapChains.Runtime.Entities.Player.Movement.TraceUtil
{
    public class Tracer
    {
        static readonly RaycastHit[] resultsCapsule = new RaycastHit[16];
        static readonly RaycastHit[] resultsBox = new RaycastHit[16];

        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="end"></param>
        /// <param name="layerMask"></param>
        public static Trace TraceCollider(Collider collider, Vector3 origin, Vector3 end, int layerMask, float colliderScale = 1f)
        {
            if (collider is BoxCollider)
                return TraceBox(origin, end, collider.bounds.extents, collider.contactOffset, layerMask, colliderScale);
            else if (collider is CapsuleCollider capc)
            {
                SurfPhysics.GetCapsulePoints(capc, origin, out var point1, out var point2);
                return TraceCapsule(point1, point2, capc.radius, origin, end, capc.contactOffset, layerMask, colliderScale);
            }

            throw new System.NotImplementedException($"Trace missing for collider: {collider.GetType()}");
        }

        public static Trace TraceCapsule(
            Vector3 point1, Vector3 point2, float radius, Vector3 start, Vector3 destination,
            float contactOffset, int layerMask, float colliderScale = 1f)
        {
            var result = new Trace()
            {
                startPos = start,
                endPos = destination
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            radius *= 1f - contactOffset;
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance(start, destination) + longSide;

            var hits = Physics.CapsuleCastNonAlloc(
                point1 - 0.5f * colliderScale * Vector3.up,
                point2 + 0.5f * colliderScale * Vector3.up,
                radius * colliderScale,
                direction,
                resultsCapsule,
                maxDistance,
                layerMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hits; i++)
            {
                result.fraction = resultsCapsule[i].distance / maxDistance;
                result.hitCollider = resultsCapsule[i].collider;
                result.hitPoint = resultsCapsule[i].point;
                result.planeNormal = resultsCapsule[i].normal;
                result.distance = resultsCapsule[i].distance;

                Ray normalRay = default;
                normalRay.origin = resultsCapsule[i].point - direction * 0.001f;
                normalRay.direction = direction;

                if (resultsCapsule[i].collider.Raycast(normalRay, out var normalHit, 0.002f))
                    result.planeNormal = normalHit.normal;
            }
            if (hits is 0)
                result.fraction = 1f;

            return result;
        }

        public static Trace TraceBox(
            Vector3 start, Vector3 destination, Vector3 extents,
            float contactOffset, int layerMask, float colliderScale = 1f)
        {
            var result = new Trace()
            {
                startPos = start,
                endPos = destination
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - start).normalized;
            var maxDistance = Vector3.Distance(start, destination) + longSide;
            extents *= 1f - contactOffset;

            var hits = Physics.BoxCastNonAlloc(
                start,
                extents * colliderScale,
                direction,
                resultsBox,
                Quaternion.identity,
                maxDistance,
                layerMask,
                QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hits; i++)
            {
                result.fraction = resultsBox[i].distance / maxDistance;
                result.hitCollider = resultsBox[i].collider;
                result.hitPoint = resultsBox[i].point;
                result.planeNormal = resultsBox[i].normal;
                result.distance = resultsBox[i].distance;

                Ray normalRay = default;
                normalRay.origin = resultsBox[i].point - direction * 0.001f;
                normalRay.direction = direction;
                
                if (resultsBox[i].collider.Raycast(normalRay, out var normalHit, 0.002f))
                    result.planeNormal = normalHit.normal;
            }
            if (hits is 0)
                result.fraction = 1f;

            return result;
        }
    }
}