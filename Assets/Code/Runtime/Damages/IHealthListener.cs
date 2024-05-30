namespace SwapChains.Runtime.Entities.Damages
{
    public interface IHealthListener
    {
        void OnHealthChange(HealthChange healthChange);
        void OnHealthDepleted(HealthChange healthChange);
    }
}