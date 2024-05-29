using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using XIV.DesignPatterns.Common.Extensions;
using XIV.DesignPatterns.Common.HealthSystem;
using XIV.DesignPatterns.Observer.Example01.ScriptableObjects;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] LayerMask targetLayerMask;
        [SerializeField] Transform firePos;
        [SerializeField] GunConfigSO config;

        List<ProjectileFireData> firedProjectiles = new List<ProjectileFireData>();
        List<ParticleDestroyData> activeParticles = new List<ParticleDestroyData>();
        ObjectPool<GameObject> projectilePool;
        ObjectPool<GameObject> projectileParticlePool;
        float fireTime = 0f;

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

            fireTime += Time.deltaTime;
        }

        public void Fire()
        {
            if (fireTime < config.fireRate) return;
            fireTime = 0f;

            var projectileGo = projectilePool.Get();
            float accuracy = 1f - config.accuracy;
            Vector3 direction = firePos.forward + (Vector3)(UnityEngine.Random.insideUnitCircle * accuracy);
            firedProjectiles.Add(new ProjectileFireData(projectileGo, direction, config.fireDistance));
        }

        void CheckActiveProjectiles()
        {
            var count = firedProjectiles.Count;
            for (int i = count - 1; i >= 0; i--)
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
            for (int i = 0; i < count; i++)
            {
                var projectileData = firedProjectiles[i];
                var pos = projectileData.projectile.transform.position;
                var nextPos = pos + (projectileData.direction * (config.projectileSpeed * Time.deltaTime));
                var diff = nextPos - pos;

                int hitCount = Physics.RaycastNonAlloc(new Ray(pos, diff), raycastHitBuffer, diff.magnitude, targetLayerMask);
                if (hitCount == 0)
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
            particleGo.transform.rotation = Quaternion.LookRotation(dir);
            particleGo.transform.position = raycastHit.point + (dir * 0.01f);
            activeParticles.Add(new ParticleDestroyData { duration = 5f, particleGo = particleGo });
            
            var parent = raycastHit.transform.root;
            if (parent.TryGetComponent<IDamageable>(out var damageable) && damageable.CanReceiveDamage())
            {
                damageable.ReceiveDamage(config.damage);
            }
        }

        void HandleActiveParticleLifetime()
        {
            int count = activeParticles.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var pData = activeParticles[i];
                pData.duration -= Time.deltaTime;

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
        GameObject CreateProjectileParticle()
        {
            return Instantiate(config.projectileHitParticlePrefab);
        }

        void OnGetProjectileParticle(GameObject particleGo)
        {
            particleGo.SetActive(true);
        }

        void OnReleaseProjectileParticle(GameObject particleGo)
        {
            particleGo.SetActive(false);
        }

        // Projectile Pool functions
        GameObject CreateProjectile()
        {
            return ProjectileFactory.CreateProjectile(ProjectileUser.Gun);
        }

        void OnGetProjectile(GameObject projectileGo)
        {
            projectileGo.transform.position = firePos.position;
            var trailRenderer = projectileGo.GetComponent<TrailRenderer>();
            trailRenderer.time = config.trailTime;
            trailRenderer.Clear();
            projectileGo.SetActive(true);
        }

        void OnReleaseProjectile(GameObject projectileGo)
        {
            projectileGo.SetActive(false);
        }
    }
}