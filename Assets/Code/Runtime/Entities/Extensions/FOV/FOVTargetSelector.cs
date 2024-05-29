using System.Buffers;
using UnityEngine;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Common.FOV
{
    public class FOVTargetSelector
    {
        public Collider currentTarget { get; private set; }
        
        readonly int obstacleLayerMask;
        readonly int targetLayerMask;

        public FOVTargetSelector(int obstacleLayerMask, int targetLayerMask)
        {
            this.obstacleLayerMask = obstacleLayerMask;
            this.targetLayerMask = targetLayerMask;
        }

        public void UpdateTarget(FieldOfViewData fieldOfViewData)
        {
            if (ValidateTarget(currentTarget, fieldOfViewData))
            {
                return;
            }

            // if (currentTarget)
            // {
            //     // Deselect current target
            // }
            currentTarget = null;

            var buffer = ArrayPool<Collider>.Shared.Rent(2);
            int hitCount = FOVHelper.GetTargetsInsideFOVNonAlloc(buffer, fieldOfViewData, targetLayerMask, obstacleLayerMask);
            for (int i = 0; i < hitCount; i++)
            {
                if (buffer[i].TryGetComponent<IDamageable>(out var damageable) && damageable.CanReceiveDamage())
                {
                    currentTarget = buffer[i];
                    break;
                }
            }
            ArrayPool<Collider>.Shared.Return(buffer, false);
        }

        bool ValidateTarget(Collider target, FieldOfViewData fieldOfViewData)
        {
            return target &&
                   FOVHelper.ValidateTargetPosition(target.bounds.center, fieldOfViewData, obstacleLayerMask)
                   && target.GetComponent<IDamageable>().CanReceiveDamage();
        }
    }
}