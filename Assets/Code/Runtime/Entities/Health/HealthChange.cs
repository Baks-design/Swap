namespace XIV.DesignPatterns.Common.HealthSystem
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
            this.normalized = currentHealth / maxHealth;
        }
    }
}