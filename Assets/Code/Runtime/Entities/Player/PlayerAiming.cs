using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Entities.Player.Movement
{
	[Serializable]
	public class PlayerAiming
	{
		[Header("References")]
		[SerializeField] Transform bodyTransform;
		InputAction lookaction;

		[Header("Sensitivity")]
		[SerializeField] float sensitivityMultiplier = 1f;
		[SerializeField] float horizontalSensitivity = 1f;
		[SerializeField] float verticalSensitivity = 1f;

		[Header("Restrictions")]
		[SerializeField] float minYRotation = -45f;
		[SerializeField] float maxYRotation = 45f;
		Vector3 realRotation; //The real rotation of the camera without recoil

		[Header("Aimpunch")]
		[Tooltip("bigger number makes the response more damped, currently the system will overshoot")]
		[SerializeField] float punchDamping = 9.0f;
		[Tooltip("bigger number increases the speed at which the view corrects")]
		[SerializeField] float punchSpringConstant = 65.0f;
		[SerializeField] Vector2 punchAngle;
		[SerializeField] Vector2 punchAngleVel;

		public void Awake() => lookaction = InputSystem.actions.FindAction("Look");

		public void Update(PlayerController controller)
		{
			// Fix pausing
			if (Mathf.Abs(Time.timeScale) <= 0f)
				return;

			DecayPunchAngle(controller);

			// Input
			var xMovement = lookaction.ReadValue<Vector2>().x * horizontalSensitivity * sensitivityMultiplier;
			var yMovement = lookaction.ReadValue<Vector2>().y * verticalSensitivity * sensitivityMultiplier;

			// Calculate real rotation from input
			realRotation.x = Mathf.Clamp(realRotation.x + yMovement, minYRotation, maxYRotation);
			realRotation.y += xMovement;
			realRotation.z = Mathf.Lerp(realRotation.z, 0f, controller.DeltaTime * 3f);

			//Apply real rotation to body
			bodyTransform.eulerAngles = Vector3.Scale(realRotation, Vector3.up);

			//Apply rotation and recoil
			var cameraEulerPunchApplied = realRotation;
			cameraEulerPunchApplied.x += punchAngle.x;
			cameraEulerPunchApplied.y += punchAngle.y;

			controller.Transform.eulerAngles = cameraEulerPunchApplied;
		}

		public void ViewPunch(Vector2 punchAmount)
		{
			//Remove previous recoil
			punchAngle = Vector2.zero;
			//Recoil go up
			punchAngleVel -= punchAmount * 20f;
		}

		void DecayPunchAngle(PlayerController controller)
		{
			if (punchAngle.sqrMagnitude > 0.001f || punchAngleVel.sqrMagnitude > 0.001f)
			{
				punchAngle += punchAngleVel * controller.DeltaTime;

				var damping = 1f - (punchDamping * controller.DeltaTime);
				if (damping < 0f)
					damping = 0f;

				punchAngleVel *= damping;

				var springForceMagnitude = punchSpringConstant * controller.DeltaTime;
				punchAngleVel -= punchAngle * springForceMagnitude;
			}
			else
			{
				punchAngle = Vector2.zero;
				punchAngleVel = Vector2.zero;
			}
		}
	}
}