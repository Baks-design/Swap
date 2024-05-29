using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01
{
    public struct ProjectileFireData
    {
        public GameObject projectile;
        public Vector3 direction;
        public float maxDistance;

        public ProjectileFireData(GameObject projectileGO, Vector3 direction, float maxDistance)
        {
            projectile = projectileGO;
            this.direction = direction;
            this.maxDistance = maxDistance;
        }
    }
}