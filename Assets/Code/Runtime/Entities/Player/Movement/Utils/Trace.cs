using UnityEngine;

namespace SwapChains.Runtime.Entities.Player.Movement.TraceUtil
{
    public struct Trace
    {
        public Vector3 startPos;
        public Vector3 endPos;
        public float fraction;
        public bool startSolid;
        public Collider hitCollider;
        public Vector3 hitPoint;
        public Vector3 planeNormal;
        public float distance;
    }
}
