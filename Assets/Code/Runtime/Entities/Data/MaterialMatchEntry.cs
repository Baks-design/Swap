using UnityEngine;
using UnityEngine.Audio;

namespace SwapChains.Runtime.Entities
{
    [CreateAssetMenu(menuName = "SwapChain/Audio/MaterialMatchEntryType")]
    public class MaterialMatchEntry : ScriptableObject
    {
        public AudioResource RandomContainer = null;
        public Material[] Materials = null;
    }
}