using Unity.Cinemachine;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public class CameraFOVTween : CameraEffectTween
    {
        [SerializeField] CinemachineCamera cam;
        [SerializeField] float targetFOV;
        float initialFOV;

        void Awake()
        {
            enabled = false;
            initialFOV = cam.Lens.FieldOfView;
        }

        void Update()
        {
            if (timer.Update(Time.deltaTime)) enabled = false;
            cam.Lens.FieldOfView = Mathf.Lerp(initialFOV, targetFOV, timer.NormalizedTimePingPong);
        }

        public override void StartTween()
        {
            if (timer.IsDone() is false)
                cam.Lens.FieldOfView = initialFOV;

            timer.Reset();
            enabled = true;
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