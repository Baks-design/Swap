using UnityEngine;

namespace SwapChains.Runtime.Utilities.Helpers
{
    public class FrameRateSetter : MonoBehaviour
    {
        [SerializeField, Range(61, 120)] int targetFrameRate;
        
        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}
