using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [CreateAssetMenu(menuName = "SO/" + nameof(GunConfigSO))]
    public class GunConfigSO : ScriptableObject
    {
        [Tooltip("x meter/second")]
        public float projectileSpeed = 40f;
        [Range(0f, 1f), Tooltip("Normalized fire rate - x projectile in second")]
        public float fireRate = 0.01f;
        [Range(0f, 1f)]
        public float accuracy = 0.95f;
        [Min(0f)]
        public float fireDistance = 80f;
        public GameObject projectileHitParticlePrefab;
        public float damage = 10f;
        public float trailTime = 0.01f;
    }
}