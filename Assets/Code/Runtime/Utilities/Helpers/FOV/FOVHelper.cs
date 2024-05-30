using System.Buffers;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.Helpers.Cam
{
    public static class FOVHelper
    {
        public static int GetTargetsInsideFOVNonAlloc(Collider[] buffer, FieldOfViewData fovData, int targetLayer, int obstacleLayer)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(fovData.position, fovData.fovDistance, buffer, targetLayer);
            var bufferLength = buffer.Length;
            var targetIndex = 0;
            for (var i = 0; i < hitCount && targetIndex < bufferLength; i++)
            {
                var target = buffer[i];
                if (ValidateTargetPosition(target.bounds.center, fovData, obstacleLayer))
                    buffer[targetIndex++] = target;
            }
            return targetIndex;
        }

        public static bool ValidateTargetPosition(Vector3 targetPos, FieldOfViewData fovData, int obstacleLayer)
        {
            var dirToTarget = (targetPos - fovData.position).normalized;
            if (Vector3.Angle(fovData.forward, dirToTarget) >= fovData.fovAngle / 2f) return false;

            var distance = Vector3.Distance(targetPos, fovData.position);
            var raycastBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);
            var raycastHitCount = Physics.RaycastNonAlloc(fovData.position, dirToTarget, raycastBuffer, distance, obstacleLayer);
            ArrayPool<RaycastHit>.Shared.Return(raycastBuffer, false);
            return raycastHitCount == 0 && distance < fovData.fovDistance;
        }
    }
}