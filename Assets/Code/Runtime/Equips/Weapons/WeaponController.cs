using System;
using System.Buffers;
using System.Collections.Generic;
using SwapChains.Runtime.Entities.Damages;
using SwapChains.Runtime.Utilities.Extensions;
using SwapChains.Runtime.VFX;
using UnityEngine;
using UnityEngine.Pool;

namespace SwapChains.Runtime.Entities.Weapons
{
    [Serializable]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] LayerMask targetLayerMask;
        [SerializeField] Transform firePos;
        [SerializeField] WeaponConfigSO config;
        
        readonly List<ProjectileFireData> firedProjectiles = new();
        readonly List<ParticleDestroyData> activeParticles = new();
        ObjectPool<GameObject> projectilePool;
        ObjectPool<GameObject> projectileParticlePool;
        float fireTime = 0f;
        float timer = 0f;

        void Awake()
        {
            projectilePool = new ObjectPool<GameObject>(CreateProjectile, OnGetProjectile, OnReleaseProjectile);
            projectileParticlePool = new ObjectPool<GameObject>(CreateProjectileParticle, OnGetProjectileParticle, OnReleaseProjectileParticle);
        }

        void Update()
        {
            CheckActiveProjectiles();
            MoveActiveProjectiles();
            HandleActiveParticleLifetime();

            timer = Time.deltaTime;
            fireTime += timer;
        }

        public void Fire()
        {
            if (fireTime < config.fireRate) return;
            fireTime = 0f;

            var projectileGo = projectilePool.Get();
            var accuracy = 1f - config.accuracy;
            var direction = firePos.forward + (Vector3)(UnityEngine.Random.insideUnitCircle * accuracy);
            firedProjectiles.Add(new ProjectileFireData(projectileGo, direction, config.fireDistance));
        }

        void CheckActiveProjectiles()
        {
            var count = firedProjectiles.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var projectileData = firedProjectiles[i];
                if (Vector3.Distance(projectileData.projectile.transform.position, firePos.position) > projectileData.maxDistance)
                {
                    projectilePool.Release(projectileData.projectile);
                    firedProjectiles.RemoveAt(i);
                }
            }
        }

        void MoveActiveProjectiles()
        {
            var raycastHitBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);
            var count = firedProjectiles.Count;
            for (var i = 0; i < count; i++)
            {
                var projectileData = firedProjectiles[i];
                var pos = projectileData.projectile.transform.position;
                var nextPos = pos + (projectileData.direction * (config.projectileSpeed * timer));
                var diff = nextPos - pos;

                var hitCount = Physics.RaycastNonAlloc(new Ray(pos, diff), raycastHitBuffer, diff.magnitude, targetLayerMask);
                if (hitCount is 0)
                {
                    projectileData.projectile.transform.position = nextPos;
                    continue;
                }

                var closestHit = raycastHitBuffer.GetClosestHit(hitCount, pos);
                projectileData.maxDistance = 0f;
                projectileData.projectile.transform.position = closestHit.point;
                firedProjectiles[i] = projectileData;
                HandleParticleOnHit(closestHit);
            }
            ArrayPool<RaycastHit>.Shared.Return(raycastHitBuffer, false);
        }

        void HandleParticleOnHit(RaycastHit raycastHit)
        {
            var particleGo = projectileParticlePool.Get();
            var dir = raycastHit.normal;
            particleGo.transform.SetPositionAndRotation(raycastHit.point + (dir * 0.01f), Quaternion.LookRotation(dir));
            activeParticles.Add(new ParticleDestroyData { duration = 5f, particleGo = particleGo });
            
            var parent = raycastHit.transform.root;
            if (parent.TryGetComponent<IDamageable>(out var damageable) && damageable.CanReceiveDamage())
                damageable.ReceiveDamage(config.damage);
        }

        void HandleActiveParticleLifetime()
        {
            var count = activeParticles.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var pData = activeParticles[i];
                pData.duration -= timer;

                if (pData.duration < 0f)
                {
                    activeParticles.RemoveAt(i);
                    projectileParticlePool.Release(pData.particleGo);
                    continue;
                }
                activeParticles[i] = pData;
            }
        }

        // Projectile Particle Pool functions
        GameObject CreateProjectileParticle() => Instantiate(config.projectileHitParticlePrefab);

        void OnGetProjectileParticle(GameObject particleGo) => particleGo.SetActive(true);

        void OnReleaseProjectileParticle(GameObject particleGo) => particleGo.SetActive(false);

        // Projectile Pool functions
        GameObject CreateProjectile() => ProjectileFactory.CreateProjectile(ProjectileUser.Gun);

        void OnGetProjectile(GameObject projectileGo)
        {
            projectileGo.transform.position = firePos.position;
            var trailRenderer = projectileGo.GetComponent<TrailRenderer>();
            trailRenderer.time = config.trailTime;
            trailRenderer.Clear();
            projectileGo.SetActive(true);
        }

        void OnReleaseProjectile(GameObject projectileGo) => projectileGo.SetActive(false);
    }
}