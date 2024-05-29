namespace XIV.DesignPatterns.Common.HealthSystem
{
    public interface IDamageable
    {
        bool CanReceiveDamage();

        void ReceiveDamage(float amount);
        Health GetHealth();
    }
}