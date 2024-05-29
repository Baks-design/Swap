using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01
{
    public static class ProjectileFactory
    {
        public static GameObject CreateProjectile(ProjectileUser projectileUser)
        {
            switch (projectileUser)
            {
                case ProjectileUser.Gun:
                    return CreateGunProjectile();
                case ProjectileUser.Turret:
                    return CreateTurretProjectile();
                default:
                    Debug.LogError(projectileUser + " is not implemented");
                    return CreateGunProjectile();
            }
        }

        static GameObject CreateGunProjectile()
        {
            var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(cubeGo.GetComponent<Collider>());
            cubeGo.transform.localScale = Vector3.one * 0.01f;
            var trailRenderer = cubeGo.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.05f;
            trailRenderer.startWidth = 0.001f;
            trailRenderer.endWidth = 0.01f;
            trailRenderer.material = new Material(cubeGo.GetComponent<Renderer>().material);
            trailRenderer.material.color = Color.red;
            return cubeGo;
        }

        static GameObject CreateTurretProjectile()
        {
            var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(cubeGo.GetComponent<Collider>());
            cubeGo.transform.localScale = Vector3.one * 0.025f;
            var trailRenderer = cubeGo.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.1f;
            trailRenderer.startWidth = 0.1f;
            trailRenderer.endWidth = 0.01f;
            trailRenderer.material = new Material(cubeGo.GetComponent<Renderer>().material);
            return cubeGo;
        }
    }
}