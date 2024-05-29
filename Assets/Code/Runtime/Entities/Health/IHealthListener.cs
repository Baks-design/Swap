namespace XIV.DesignPatterns.Common.HealthSystem
{
    public interface IHealthListener
    {
        void OnHealthChange(HealthChange healthChange);
        void OnHealthDepleted(HealthChange healthChange);
    }
}