using UnityEngine;

namespace XIV.DesignPatterns.Common
{
    public class FrameRateSetter : MonoBehaviour
    {
        [SerializeField, Range(61, 120)] int targetFrameRate;
        
        void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}
