using UnityEngine;

namespace SwapChains.Runtime.Utilities.Extensions
{
    public static class RaycastHitCollectionExtensions
    {
        public static RaycastHit GetClosestHit(this RaycastHit[] arr, int length, Vector3 pos)
        {
            var distance = float.MaxValue;
            RaycastHit closestHit = default;
            for (var i = 0; i < length; i++)
            {
                var tempDistance = (arr[i].point - pos).sqrMagnitude;
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    closestHit = arr[i];
                }
            }
            return closestHit;
        }
    }
}