using SwapChains.Runtime.Utilities.ServicesLocator;
using UnityEngine;

namespace SwapChains.Runtime.VFX
{
    public class InterfaceEffectsController : MonoBehaviour
    {
        [SerializeField] GameObject transition;

        void Awake() => ServiceLocator.Global.Register(this);

        public void ActiveTransition(bool isActive) => transition.SetActive(isActive);
    }
}