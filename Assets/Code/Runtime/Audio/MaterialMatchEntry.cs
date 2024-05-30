using UnityEngine;
using UnityEngine.Audio;

namespace SwapChains.Runtime.Audio
{
    [CreateAssetMenu(menuName = "SwapChain/Audio/MaterialMatchEntryType")]
    public class MaterialMatchEntry : ScriptableObject
    {
        public AudioResource RandomContainer;
        public Material[] Materials;
    }
}