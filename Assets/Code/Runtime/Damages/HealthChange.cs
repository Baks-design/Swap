namespace SwapChains.Runtime.Entities.Damages
{
    public readonly struct HealthChange
    {
        public readonly float maxHealth;
        public readonly float currentHealth;
        public readonly float normalized;

        public HealthChange(float maxHealth, float currentHealth) : this()
        {
            this.currentHealth = currentHealth;
            this.maxHealth = maxHealth;
            normalized = currentHealth / maxHealth;
        }
    }
}