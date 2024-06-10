using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public class InterfaceEffectsController : MonoBehaviour
    {
        [SerializeField] GameObject transition;

        public void ActiveTransition(bool isActive) => transition.SetActive(isActive);
    }
}