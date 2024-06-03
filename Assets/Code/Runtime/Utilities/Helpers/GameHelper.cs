using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SwapChains.Runtime.Utilities.Helpers
{
    public static class GameHelper
    {
        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static void ShowCursor(bool isShow)
        {
            if (isShow)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public static void SetTimeScale(float time) => Time.timeScale = time;

        static readonly Dictionary<float, WaitForSeconds> WaitForSecondsDict = new(100, new FloatComparer());
        struct FloatComparer : IEqualityComparer<float>
        {
            public readonly bool Equals(float x, float y) => Mathf.Abs(x - y) <= Mathf.Epsilon;
            public readonly int GetHashCode(float obj) => obj.GetHashCode();
        }
        /// <summary>
        /// Returns a WaitForSeconds object for the specified duration. </summary>
        /// <param name="seconds">The duration in seconds to wait.</param>
        /// <returns>A WaitForSeconds object.</returns>
        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            if (seconds < 1f / Application.targetFrameRate)
                return null;

            if (WaitForSecondsDict.TryGetValue(seconds, out var forSeconds))
                return forSeconds;

            var waitForSeconds = new WaitForSeconds(seconds);
            WaitForSecondsDict[seconds] = waitForSeconds;

            return waitForSeconds;
        }

        #region Input Maps
        public static void EnableUIMap()
        {
            InputSystem.actions.FindActionMap("UI").Enable();
            InputSystem.actions.FindActionMap("Humanoid").Disable();
        }

        public static void EnableHumanoidMap()
        {
            InputSystem.actions.FindActionMap("UI").Disable();
            InputSystem.actions.FindActionMap("Humanoid").Enable();
        }

        public static void DisableAllInput()
        {
            InputSystem.actions.FindActionMap("UI").Disable();
            InputSystem.actions.FindActionMap("Humanoid").Disable();
        }
        #endregion

        #region Physics
        public static (bool, RaycastHit) CheckInteraction(
            Camera mainCam, RaycastHit[] hits, float range, LayerMask layer, QueryTriggerInteraction query)
        {
            bool isColliding = default;
            RaycastHit hit = default;
            var ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

            var hitCount = Physics.RaycastNonAlloc(ray, hits, range, layer, query);
            if (hitCount > 0)
            {
                hit = hits[0];
                isColliding = true;
            }

            return (isColliding, hit);
        }

        /// <param name="angle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f)
                angle = 360f + angle;

            if (angle > 180f)
                return Mathf.Max(angle, 360f + from);

            return Mathf.Min(angle, to);
        }
        #endregion
    }
}
