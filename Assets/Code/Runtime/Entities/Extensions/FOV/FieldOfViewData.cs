using UnityEngine;

namespace XIV.DesignPatterns.Common.FOV
{
    public ref struct FieldOfViewData
    {
        public Vector3 position;
        public Vector3 forward;
        public float fovAngle;
        public float fovDistance;

        public FieldOfViewData(Vector3 position, Vector3 forward, float fovAngle, float fovDistance) : this()
        {
            this.position = position;
            this.forward = forward;
            this.fovAngle = fovAngle;
            this.fovDistance = fovDistance;
        }
    }
}