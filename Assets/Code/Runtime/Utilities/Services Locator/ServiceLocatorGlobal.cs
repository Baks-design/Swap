using UnityEngine;

namespace SwapChains.Runtime.Utilities.ServicesLocator
{
    [AddComponentMenu("SwapChains/Utilities/ServiceLocator/ServiceLocator Global")]
    public class ServiceLocatorGlobal : ServiceLocatorBootstrapper
    {
        [SerializeField] bool dontDestroyOnLoad = true;

        protected override void Bootstrap() => Container.ConfigureAsGlobal(dontDestroyOnLoad);
    }
}