using SwapChains.Runtime.Utilities.Extensions;
using UnityEngine;

namespace SwapChains.Runtime.Utilities.ServicesLocator
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class ServiceLocatorBootstrapper : MonoBehaviour
    {
        bool hasBeenBootstrapped;
        ServiceLocator container;

        internal ServiceLocator Container => container.OrNull() ?? (container = GetComponent<ServiceLocator>());

        void Awake() => BootstrapOnDemand();

        public void BootstrapOnDemand()
        {
            if (hasBeenBootstrapped)
                return;
            hasBeenBootstrapped = true;
            Bootstrap();
        }

        protected abstract void Bootstrap();
    }
}