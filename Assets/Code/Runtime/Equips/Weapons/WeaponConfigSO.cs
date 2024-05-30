using UnityEngine;

namespace SwapChains.Runtime.Entities.Weapons
{
    [CreateAssetMenu(menuName = "SwpChains/Weapon" + nameof(WeaponConfigSO))]
    public class WeaponConfigSO : ScriptableObject
    {
        public float projectileSpeed = 40f;
        [Range(0f, 1f)] public float fireRate = 0.01f;
        [Range(0f, 1f)] public float accuracy = 0.95f;
        [Min(0f)] public float fireDistance = 80f;
        public GameObject projectileHitParticlePrefab;
        public float damage = 10f;
        public float trailTime = 0.01f;
    }
}