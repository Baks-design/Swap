namespace SwapChains.Runtime.Entities.Damages
{
    public interface IDamageable
    {
        bool CanReceiveDamage();
        void ReceiveDamage(int amount);
        Health GetHealth();
    }
}