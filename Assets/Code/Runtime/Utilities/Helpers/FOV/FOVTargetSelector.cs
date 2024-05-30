using System.Buffers;
using SwapChains.Runtime.Entities.Damages;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.Helpers.Cam
{
    public class FOVTargetSelector
    {
        readonly int obstacleLayerMask;
        readonly int targetLayerMask;

        public Collider CurrentTarget { get; private set; }

        public FOVTargetSelector(int obstacleLayerMask, int targetLayerMask)
        {
            this.obstacleLayerMask = obstacleLayerMask;
            this.targetLayerMask = targetLayerMask;
        }

        public void UpdateTarget(FieldOfViewData fieldOfViewData)
        {
            if (ValidateTarget(CurrentTarget, fieldOfViewData)) return;

            CurrentTarget = null;

            var buffer = ArrayPool<Collider>.Shared.Rent(2);
            var hitCount = FOVHelper.GetTargetsInsideFOVNonAlloc(buffer, fieldOfViewData, targetLayerMask, obstacleLayerMask);
            for (var i = 0; i < hitCount; i++)
            {
                if (buffer[i].TryGetComponent<IDamageable>(out var damageable) && damageable.CanReceiveDamage())
                {
                    CurrentTarget = buffer[i];
                    break;
                }
            }
            ArrayPool<Collider>.Shared.Return(buffer, false);
        }

        bool ValidateTarget(Collider target, FieldOfViewData fieldOfViewData)
        => target &&
            FOVHelper.ValidateTargetPosition(target.bounds.center, fieldOfViewData, obstacleLayerMask) &&
            target.GetComponent<IDamageable>().CanReceiveDamage();
    }
}