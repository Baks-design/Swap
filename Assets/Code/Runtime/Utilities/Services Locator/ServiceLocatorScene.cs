using UnityEngine;

namespace SwapChains.Runtime.Utilities.ServicesLocator
{
    [AddComponentMenu("SwapChains/Utilities/ServiceLocator/ServiceLocator Scene")]
    public class ServiceLocatorScene : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap() => Container.ConfigureForScene();
    }
}