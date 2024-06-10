namespace SwapChains.Runtime.Entities.Damages
{
    public readonly struct HealthChange
    {
        public readonly int maxHealth;
        public readonly int currentHealth;

        public HealthChange(int maxHealth, int currentHealth) : this()
        {
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
        }
    }
}