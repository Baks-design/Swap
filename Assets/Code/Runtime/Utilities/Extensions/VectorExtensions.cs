using UnityEngine;

namespace SwapChains.Runtime.Utilities.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 VectorMa(Vector3 start, float scale, Vector3 direction)
        {
            Vector3 dest;
            dest.x = start.x + direction.x * scale;
            dest.y = start.y + direction.y * scale;
            dest.z = start.z + direction.z * scale;
            return dest;
        }
    }
}