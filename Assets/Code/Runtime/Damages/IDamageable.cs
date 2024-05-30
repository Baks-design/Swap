namespace SwapChains.Runtime.Entities.Damages
{
    public interface IDamageable
    {
        bool CanReceiveDamage();
        void ReceiveDamage(float amount);
        Health GetHealth();
    }
}