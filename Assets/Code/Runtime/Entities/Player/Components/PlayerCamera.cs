using KBCore.Refs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Entities.Player
{
	public class PlayerCamera : MonoBehaviour
	{
		[Header("References")]
		[SerializeField, Anywhere] Transform CinemachineCameraTarget;
		[SerializeField, Self] InterfaceRef<IPlayerInput> playerInput;

		[Header("Settings")]
		[SerializeField] float TopClamp = 70.0f;
		[SerializeField] float BottomClamp = -30.0f;
		[SerializeField] float CameraAngleOverride = 0f;
		[SerializeField] bool LockCameraPosition = false;

		float _cinemachineTargetYaw;
		float _cinemachineTargetPitch;
		const float THRESHOLD = 0.01f;

		bool IsCurrentDeviceMouse => InputSystem.actions.FindControlScheme("KeyboardMouse").HasValue;

		void OnValidate() => this.ValidateRefs();

        void Start() => _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        void LateUpdate() => ProcessCamera();

        void ProcessCamera()
		{
			// if there is an input and camera position is not fixed
			if (playerInput.Value.GetLook().sqrMagnitude >= THRESHOLD && !LockCameraPosition)
			{
				//Don't multiply mouse input by Time.deltaTime;
				var deltaTimeMultiplier = IsCurrentDeviceMouse ? 1f : Time.deltaTime;

				_cinemachineTargetYaw += playerInput.Value.GetLook().x * deltaTimeMultiplier;
				_cinemachineTargetPitch += playerInput.Value.GetLook().y * deltaTimeMultiplier;
			}

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Cinemachine will follow this target
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
				_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0f);
		}

		static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f)
				lfAngle += 360f;

			if (lfAngle > 360f)
				lfAngle -= 360f;

			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
	}
}