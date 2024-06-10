using UnityEngine;

namespace SwapChains.Runtime.Entities.Player
{
    public interface IPlayerInput
    {
        bool GetPause();
        bool GetUnpause();

        Vector2 GetLook();
        Vector2 GetMovement();
        bool GetSprint();
        bool GetJumpDown();
        bool GetJumpHeld();
        bool GetCrouchDown();
        bool GetCrouchHeld();
        bool GetCrouchUp();
        bool ChargingDown();
        bool GetNoClipUp();
        bool GetClimbLadder();

        bool GetShoot();
        bool GetShowBody();
        bool GetSelectBody();
        bool GetSwitchBody();
    }
}