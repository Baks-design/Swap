using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Entities.Player
{
    public class PlayerInput : MonoBehaviour, IPlayerInput
    {
        InputAction pauseAction;
        InputAction unpauseAction;
        InputAction moveAction;
        InputAction lookAction;
        InputAction sprintAction;
        InputAction jumpAction;
        InputAction crouchAction;
        InputAction chargingAction;
        InputAction noClipAction;
        InputAction climberAction;
        InputAction shootAction;
        InputAction showBodyAction;
        InputAction selectBodyAction;
        InputAction switchBodyAction;

        void Start()
        {
            pauseAction = InputSystem.actions.FindAction("Pause");
            unpauseAction = InputSystem.actions.FindAction("Unpause");
            lookAction = InputSystem.actions.FindAction("Look");
            moveAction = InputSystem.actions.FindAction("Move");
            jumpAction = InputSystem.actions.FindAction("Jump");
            sprintAction = InputSystem.actions.FindAction("Sprint");
            crouchAction = InputSystem.actions.FindAction("Crouch");
            chargingAction = InputSystem.actions.FindAction("Charging");
            noClipAction = InputSystem.actions.FindAction("NoClip");
            shootAction = InputSystem.actions.FindAction("Shoot");
            climberAction = InputSystem.actions.FindAction("Climber");
            showBodyAction = InputSystem.actions.FindAction("ShowBody");
            selectBodyAction = InputSystem.actions.FindAction("SelectBody");
            switchBodyAction = InputSystem.actions.FindAction("SwitchBody");
        }

        bool IPlayerInput.GetPause() => pauseAction.WasPressedThisFrame();
        bool IPlayerInput.GetUnpause()
        {
            if (Time.timeScale != 0f)
                return false;

            return unpauseAction.WasPressedThisFrame();
        }
        Vector2 IPlayerInput.GetLook() => lookAction.ReadValue<Vector2>();
        Vector2 IPlayerInput.GetMovement() => moveAction.ReadValue<Vector2>();
        bool IPlayerInput.GetSprint() => sprintAction.IsPressed();
        bool IPlayerInput.GetJumpDown() => jumpAction.WasPressedThisFrame();
        bool IPlayerInput.GetJumpHeld() => jumpAction.IsPressed();
        bool IPlayerInput.GetCrouchUp() => crouchAction.WasPressedThisFrame();
        bool IPlayerInput.GetCrouchHeld() => crouchAction.IsPressed();
        bool IPlayerInput.GetCrouchDown() => crouchAction.WasReleasedThisFrame();
        bool IPlayerInput.ChargingDown() => chargingAction.WasPressedThisFrame();
        bool IPlayerInput.GetNoClipUp() => noClipAction.WasReleasedThisFrame();
        bool IPlayerInput.GetClimbLadder() => climberAction.WasPressedThisFrame();
        bool IPlayerInput.GetShoot() => shootAction.WasPressedThisFrame();
        bool IPlayerInput.GetShowBody() => showBodyAction.WasPressedThisFrame();
        bool IPlayerInput.GetSelectBody() => selectBodyAction.IsPressed();
        bool IPlayerInput.GetSwitchBody() => switchBodyAction.WasPressedThisFrame();
    }
}