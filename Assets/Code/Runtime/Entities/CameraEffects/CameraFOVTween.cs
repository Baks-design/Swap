using Unity.Cinemachine;
using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.PlayerDamageEffects.CameraEffects
{
    public class CameraFOVTween : CameraEffectTween
    {
        [SerializeField] CinemachineCamera cam;
        [SerializeField] float targetFOV;
        float initialFOV;

        void Awake()
        {
            this.enabled = false;
            initialFOV = cam.Lens.FieldOfView;
        }

        void Update()
        {
            if (timer.Update(Time.deltaTime))
            {
                this.enabled = false;
            }
            cam.Lens.FieldOfView = Mathf.Lerp(initialFOV, targetFOV, timer.normalizedTimePingPong);
        }

        public override void StartTween()
        {
            if (timer.IsDone() == false)
            {
                cam.Lens.FieldOfView = initialFOV;
            }
            base.timer.Reset();
            this.enabled = true;
        }

#if UNITY_EDITOR

        void OnValidate()
        {
            if (cam) return;
            cam = GetComponent<CinemachineCamera>();
        }

#endif
    }
}